using GalaSoft.MvvmLight.Command;
using QuizCliente.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Unidad1Quiz.Models;

namespace QuizCliente.ViewModels
{
    public class PantallaJuegoViewModel:INotifyPropertyChanged
    {
        private ClienteQuizServices clienteService;
        public string Enunciado { get; set; } = "";
        public List<string> Opciones { get; set; } = new();
        public ICommand SeleccionarOpcionCommand { get; }

        public PantallaJuegoViewModel(ClienteQuizServices service)
        {
            clienteService = service;
            clienteService.PreguntaRecibida += OnPreguntaRecibida;
            clienteService.HabilitarRespuesta += OnHabilitarRespuesta;
            clienteService.MensajeSistema += msg => MessageBox.Show(msg);

            SeleccionarOpcionCommand = new RelayCommand<int>(SeleccionarOpcion);
        }

        private void OnPreguntaRecibida(Pregunta pregunta)
        {
            Enunciado = pregunta.Enunciado;
            Opciones = pregunta.Opciones;
            OnPropertyChanged(nameof(Enunciado));
            OnPropertyChanged(nameof(Opciones));
        }

        private void OnHabilitarRespuesta()
        {
            // Permitir botones
        }

        private void SeleccionarOpcion(int index)
        {
            clienteService.EnviarRespuesta(index);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
