using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unidad1Quiz.Models
{
    public class Pregunta
    {
        public string Enunciado { get; set; }
        public List<string> Opciones { get; set; }
        public int RespuestaCorrecta { get; set; }

        public Pregunta(string enunciado, List<string> opciones, int respuestaCorrecta)
        {
            Enunciado = enunciado;
            Opciones = opciones;
            RespuestaCorrecta = respuestaCorrecta;
        }
    }

    public class MensajeCliente
    {
        public string? Tipo { get; set; }
        public string? Nombre { get; set; }
        public int OpcionSeleccionada { get; set; }
    }
}