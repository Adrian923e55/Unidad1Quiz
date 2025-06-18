using QuizCliente.Views;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using QuizCliente.Services;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace QuizCliente.ViewModels
{
    public class JuegoPrincipalViewModel :INotifyPropertyChanged
    {
        private DispatcherTimer? _timer;
        private int _tiempoRestante;
        public int TiempoRestante
        {
            get => _tiempoRestante;
            set
            {
                _tiempoRestante = value;
                OnPropertyChanged(nameof(TiempoRestante));
                OnPropertyChanged(nameof(TiempoRestanteTexto));
            }
        }
        public string PlayerName { get; set; } = "";
        public string TiempoRestanteTexto => $"00:{TiempoRestante:00}";
        public ICommand StartCommand { get; }

        private ClienteQuizServices clienteService;

        public JuegoPrincipalViewModel()
        {
            clienteService = new ClienteQuizServices();
            StartCommand = new RelayCommand(Iniciar);
        }

        private void Iniciar()
        {
            if (string.IsNullOrWhiteSpace(PlayerName)) return;

            clienteService.Registrar(PlayerName);

            var pantallaJuego = new PantallaJuego(clienteService);
            pantallaJuego.Show();

            Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w is JuegoPrincipal)?.Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
