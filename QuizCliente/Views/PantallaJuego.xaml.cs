using QuizCliente.Services;
using QuizCliente.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuizCliente.Views
{
    /// <summary>
    /// Lógica de interacción para PantallaJuego.xaml
    /// </summary>
    public partial class PantallaJuego : Window
    {
        public PantallaJuego()
        {
            InitializeComponent();
        }

        public PantallaJuego(ClienteQuizServices clienteService)
        {
            InitializeComponent();
            DataContext= new PantallaJuegoViewModel(clienteService);
        }
    }
}
