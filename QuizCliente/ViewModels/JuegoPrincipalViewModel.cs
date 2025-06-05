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

namespace QuizCliente.ViewModels
{
    public class JuegoPrincipalViewModel :INotifyPropertyChanged
    {
        public string PlayerName { get; set; } = "";
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
        
    }
}
