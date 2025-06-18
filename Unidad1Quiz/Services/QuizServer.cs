using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using Unidad1Quiz.Models;

public class QuizServer
{
    private UdpClient servidor;
    private Dictionary<IPEndPoint, string> clientes = new();
    private Dictionary<IPEndPoint, int> respuestas = new();
    private HashSet<string> nombresUsados = new();
    private bool aceptandoRespuestas = false;
    private bool ejecutando = true;
    private Thread? hiloRecepcion;
    private Pregunta? preguntaActual;

    // Eventos públicos para comunicación con el ViewModel
    public event Action<string>? NuevaPreguntaRecibida;
    public event Action<List<string>>? AciertosActualizados;
    public event Action<int>? JugadoresConectadosActualizados;
    public event Action<string>? ClienteRegistrado;

    public QuizServer()
    {
        servidor = new UdpClient(65000);
        servidor.EnableBroadcast = true;

        Console.WriteLine("Servidor iniciado en puerto 65000");

        hiloRecepcion = new Thread(EscucharMensajes)
        {
            IsBackground = true
        };
        hiloRecepcion.Start();
    }

    void EscucharMensajes()
    {
        while (ejecutando)
        {
            try
            {
                IPEndPoint cliente = new IPEndPoint(IPAddress.Any, 0);
                byte[] buffer = servidor.Receive(ref cliente);
                string json = Encoding.UTF8.GetString(buffer);

                var mensaje = JsonSerializer.Deserialize<MensajeCliente>(json);
                if (mensaje == null) continue;

                if (mensaje.Tipo == "registro")
                {
                    RegistrarCliente(cliente, mensaje.Nombre!);
                }
                else if (mensaje.Tipo == "respuesta")
                {
                    ProcesarRespuesta(cliente, mensaje.OpcionSeleccionada);
                }
            }
            catch (SocketException)
            {
                if (!ejecutando) break;
            }
            catch
            {
                Console.WriteLine("Mensaje inválido recibido.");
            }
        }

        Console.WriteLine("Hilo de recepción finalizado.");
    }

    void RegistrarCliente(IPEndPoint ep, string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            EnviarMensaje(ep, new { tipo = "error", mensaje = "Debe enviar un nombre válido para registrarse." });
            return;
        }
        string nombreFinal = nombre;
        int contador = 1;

        while (nombresUsados.Contains(nombreFinal))
        {
            nombreFinal = $"{nombre} ({contador++})";
        }

        clientes[ep] = nombreFinal;
        nombresUsados.Add(nombreFinal);

        Console.WriteLine($"Cliente registrado: {nombreFinal} ({ep})");

        EnviarMensaje(ep, new { tipo = "registro_confirmado", nombre = nombreFinal });

        // Notificar al ViewModel
        ClienteRegistrado?.Invoke(nombreFinal);
        JugadoresConectadosActualizados?.Invoke(clientes.Count);
    }

    void ProcesarRespuesta(IPEndPoint ep, int opcion)
    {
        if (!aceptandoRespuestas || respuestas.ContainsKey(ep))
        {
            EnviarMensaje(ep, new { tipo = "rechazo", razon = "Tiempo expirado o respuesta duplicada" });
            return;
        }

        respuestas[ep] = opcion;
        Console.WriteLine($"Respuesta recibida de {clientes[ep]}: {opcion}");
        EnviarMensaje(ep, new { tipo = "confirmacion", estado = "recibida" });
    }

    public void EnviarPregunta(Pregunta pregunta)
    {
        preguntaActual = pregunta;
        respuestas.Clear();
        aceptandoRespuestas = false;

        var mensaje = new
        {
            tipo = "pregunta",
            enunciado = pregunta.Enunciado,
            opciones = pregunta.Opciones
        };

        Broadcast(mensaje);
        Console.WriteLine($"Pregunta enviada: {pregunta.Enunciado}");

       
        NuevaPreguntaRecibida?.Invoke(pregunta.Enunciado);

        // Esperar 3 segundos antes de aceptar respuestas
        Thread.Sleep(3000);
        aceptandoRespuestas = true;
        Console.WriteLine("Respuestas habilitadas por 10 segundos.");
        Broadcast(new { tipo = "habilitarRespuesta" });

        // Esperar 10 segundos para las respuestas
        Thread.Sleep(10000);
        aceptandoRespuestas = false;

        Console.WriteLine("Tiempo finalizado. Respuestas deshabilitadas.");
        MostrarResultados();
    }

    void MostrarResultados()
    {
        if (preguntaActual == null) return;

        var conteo = new int[4];
        var aciertos = new List<string>();

        foreach (var kvp in respuestas)
        {
            int opcion = kvp.Value;
            conteo[opcion]++;
            if (opcion == preguntaActual.RespuestaCorrecta)
                aciertos.Add(clientes[kvp.Key]);
        }

        Console.WriteLine("Resultados de la pregunta:");
        for (int i = 0; i < 4; i++)
        {
            Console.WriteLine($"{(char)('A' + i)}: {conteo[i]} respuestas");
        }

        Console.WriteLine("Aciertos:");
        foreach (var nombre in aciertos)
        {
            Console.WriteLine($"✔ {nombre}");
        }

        var resumen = new
        {
            tipo = "resultados",
            conteo,
            aciertos
        };

        // Notificar al ViewModel
        AciertosActualizados?.Invoke(aciertos);

        Broadcast(resumen);
    }

    void Broadcast(object mensaje)
    {
        byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(mensaje);

        foreach (var ep in clientes.Keys)
        {
            servidor.Send(buffer, buffer.Length, ep);
        }
    }

    void EnviarMensaje(IPEndPoint ep, object mensaje)
    {
        byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(mensaje);
        servidor.Send(buffer, buffer.Length, ep);
    }

    public void Detener()
    {
        ejecutando = false;
        try
        {
            servidor.Close();
        }
        catch { }

        hiloRecepcion?.Join();
        Console.WriteLine("Servidor detenido.");
    }
}
