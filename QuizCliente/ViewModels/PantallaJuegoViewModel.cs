using GalaSoft.MvvmLight.Command;
using QuizCliente.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Unidad1Quiz.Models;

namespace QuizCliente.ViewModels
{
    public class PantallaJuegoViewModel : INotifyPropertyChanged
    {
        private ClienteQuizServices clienteService;
        private Timer? temporizador;

        // Variable interna y propiedad para el temporizador
        private int tiempoRestante;
        public int TiempoRestante
        {
            get => tiempoRestante;
            set
            {
                tiempoRestante = value;
                OnPropertyChanged(nameof(TiempoRestante));
                OnPropertyChanged(nameof(TiempoRestanteTexto));
            }
        }

        // Propiedad que formatea el tiempo en formato "00:15"
        public string TiempoRestanteTexto => $"00:{TiempoRestante:00}";

        // Propiedad que determina si se pueden enviar respuestas
        private bool respuestasHabilitadas;
        public bool RespuestasHabilitadas
        {
            get => respuestasHabilitadas;
            set
            {
                respuestasHabilitadas = value;
                OnPropertyChanged(nameof(RespuestasHabilitadas));
            }
        }

        // Propiedad para el enunciado de la pregunta
        private string enunciado = "";
        public string Enunciado
        {
            get => enunciado;
            set
            {
                enunciado = value;
                OnPropertyChanged(nameof(Enunciado));
            }
        }

        // Propiedad para la lista de opciones
        private List<string> opciones = new();
        public List<string> Opciones
        {
            get => opciones;
            set
            {
                opciones = value;
                OnPropertyChanged(nameof(Opciones));
            }
        }

        
        public ICommand SeleccionarOpcionCommand { get; }

        public PantallaJuegoViewModel(ClienteQuizServices service)
        {
            clienteService = service ?? throw new ArgumentNullException(nameof(service));

           
            clienteService.PreguntaRecibida += OnPreguntaRecibida;
            clienteService.HabilitarRespuesta += OnHabilitarRespuesta;
            clienteService.MensajeSistema += msg =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(msg);
                });
            };

            SeleccionarOpcionCommand = new RelayCommand<int>(SeleccionarOpcion);
        }

        
        private void OnPreguntaRecibida(Pregunta pregunta)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Enunciado = pregunta.Enunciado;
                Opciones = new List<string>(pregunta.Opciones);
                
                RespuestasHabilitadas = false;
                
                TiempoRestante = 15;
                
                temporizador?.Dispose();
            });
        }

        
        private void OnHabilitarRespuesta()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RespuestasHabilitadas = true;
                
                TiempoRestante = 15;
                temporizador?.Dispose();
               
                temporizador = new Timer(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TiempoRestante--;
                        if (TiempoRestante <= 0)
                        {
                            RespuestasHabilitadas = false;
                            temporizador?.Dispose();
                        }
                    });
                }, null, 0, 1000);
            });
        }

        
        private void SeleccionarOpcion(int index)
        {
            if (!RespuestasHabilitadas) return;
            clienteService.EnviarRespuesta(index);
            
            RespuestasHabilitadas = false;
        }

      
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}