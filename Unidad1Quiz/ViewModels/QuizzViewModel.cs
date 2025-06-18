using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Unidad1Quiz.Models;

namespace Unidad1Quiz.ViewModels
{
    public class QuizViewModel : INotifyPropertyChanged
    {
        private QuizServer? _servidor;
        private bool _servidorIniciado;

        private string _preguntaActual = "Esperando pregunta...";
        public string PreguntaActual
        {
            get => _preguntaActual;
            set => SetProperty(ref _preguntaActual, value);
        }

        private int _totalJugadores;
        public int TotalJugadores
        {
            get => _totalJugadores;
            set => SetProperty(ref _totalJugadores, value);
        }

        private string _estadoServidor = "Servidor no iniciado";
        public string EstadoServidor
        {
            get => _estadoServidor;
            set => SetProperty(ref _estadoServidor, value);
        }

        public ObservableCollection<string> Aciertos { get; set; } = new();
        public ObservableCollection<string> RegistroClientes { get; set; } = new();

        public ICommand IniciarServidorCommand { get; }
        public ICommand DetenerServidorCommand { get; }

        public QuizViewModel()
        {
            IniciarServidorCommand = new RelayCommand(IniciarServidor, () => !_servidorIniciado);
            DetenerServidorCommand = new RelayCommand(DetenerServidor, () => _servidorIniciado);
        }

        private void IniciarServidor()
        {
            _servidor = new QuizServer();

            // Puedes invocar estas acciones desde el servidor cuando implementes los eventos:
            _servidor.NuevaPreguntaRecibida += (pregunta) =>
            {
                PreguntaActual = pregunta;
            };

            _servidor.JugadoresConectadosActualizados += (total) =>
            {
                TotalJugadores = total;
            };

            _servidor.AciertosActualizados += (lista) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    Aciertos.Clear();
                    foreach (var nombre in lista)
                        Aciertos.Add(nombre);
                });
            };

            _servidor.ClienteRegistrado += (nombre) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    RegistroClientes.Add(nombre);
                    TotalJugadores = RegistroClientes.Count;
                });
            };

            EstadoServidor = "Servidor en ejecución";
            _servidorIniciado = true;
            RaiseCanExecuteChanged();
        }

        private void DetenerServidor()
        {
            _servidor?.Detener();
            _servidor = null;

            EstadoServidor = "Servidor detenido";
            _servidorIniciado = false;
            RaiseCanExecuteChanged();
        }

        private void RaiseCanExecuteChanged()
        {
            (IniciarServidorCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DetenerServidorCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propName = "")
        {
            if (!Equals(storage, value))
            {
                storage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
