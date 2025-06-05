using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Unidad1Quiz.Models;

namespace QuizCliente.Services
{
    public class ClienteQuizServices
    {
        public event Action<Pregunta>? PreguntaRecibida;
        public event Action? HabilitarRespuesta;
        public event Action<string>? MensajeSistema;
        public event Action<List<string>, int[]>? MostrarResultados;

        private UdpClient cliente;
        private IPEndPoint servidor = new IPEndPoint(IPAddress.Broadcast, 65000);
        private string? nombre;

        public ClienteQuizServices()
        {
            cliente = new UdpClient();
            cliente.EnableBroadcast = true;

            Thread escucha = new Thread(Escuchar);
            escucha.IsBackground = true;
            escucha.Start();
        }

        public void Registrar(string nombre)
        {
            this.nombre = nombre;

            var mensaje = new
            {
                tipo = "registro",
                nombre = nombre
            };

            Enviar(mensaje);
        }

        public void EnviarRespuesta(int opcion)
        {
            var mensaje = new
            {
                tipo = "respuesta",
                opcionSeleccionada = opcion
            };

            Enviar(mensaje);
        }

        private void Enviar(object obj)
        {
            byte[] datos = JsonSerializer.SerializeToUtf8Bytes(obj);
            cliente.Send(datos, datos.Length, servidor);
        }

        private void Escuchar()
        {
            while (true)
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                byte[] buffer = cliente.Receive(ref ep);
                string json = Encoding.UTF8.GetString(buffer);

                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var tipo = doc.RootElement.GetProperty("tipo").GetString();

                    switch (tipo)
                    {
                        case "pregunta":
                            var enunciado = doc.RootElement.GetProperty("enunciado").GetString()!;
                            var opciones = doc.RootElement.GetProperty("opciones").EnumerateArray()
                                .Select(o => o.GetString()!).ToList();
                            PreguntaRecibida?.Invoke(new Pregunta(enunciado, opciones, -1));
                            break;

                        case "habilitarRespuesta":
                            HabilitarRespuesta?.Invoke();
                            break;

                        case "resultados":
                            var conteo = doc.RootElement.GetProperty("conteo").EnumerateArray().Select(e => e.GetInt32()).ToArray();
                            var aciertos = doc.RootElement.GetProperty("aciertos").EnumerateArray().Select(e => e.GetString()!).ToList();
                            MostrarResultados?.Invoke(aciertos, conteo);
                            break;

                        case "rechazo":
                            MensajeSistema?.Invoke("⚠ Respuesta rechazada.");
                            break;
                    }
                }
                catch
                {
                    MensajeSistema?.Invoke("⚠ Error al procesar mensaje.");
                }
            }
        }
    }
}
