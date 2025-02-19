using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FusionAxion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Grid GButtons;
        private DispatcherTimer timer;
        private readonly int numeroDeSurtidores = 12;
        private int surtidorActual = 0;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GButtons = new Grid
            {
                Height = 320,
                Width = 500,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            CrearBotones();
            IniciarTimer();
        }

        private void IniciarTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5) // Ejecutar cada 5 segundos
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Código que se ejecuta periódicamente
            BuscarDespacho();
        }

        private async void BuscarDespacho()
        {
            if (surtidorActual <= GButtons.Children.Count && GButtons.Children[surtidorActual] is Button boton)
            {
                boton.Background = new SolidColorBrush(Color.FromRgb(255, 0, 255));

                await Task.Delay(3000);

                boton.Background = new SolidColorBrush(Color.FromRgb(0, 0, 255));
            }

            surtidorActual++;
            if (surtidorActual >= numeroDeSurtidores)
            {
                surtidorActual = 0;
            }
        }

        private void CrearBotones()
        {
            // Definir las filas y columnas
            for (int i = 0; i < 2; i++)
            {
                GButtons.RowDefinitions.Add(new RowDefinition());
            }

            for (int j = 0; j < 6; j++)
            {
                GButtons.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Crear y agregar los botones
            for (int i = 0; i < numeroDeSurtidores; i++)
            {
                BotonPersonalizado boton = new BotonPersonalizado($"Btn {i + 1}", i);
                Grid.SetRow(boton, i / 6);
                Grid.SetColumn(boton, i % 6);
                _ = GButtons.Children.Add(boton);
            }

            _ = GMain.Children.Add(GButtons);

            Content = GMain;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            // Mostrar un cuadro de diálogo de confirmación
            MessageBoxResult result = MessageBox.Show("¿Está seguro que desea realizar esta acción?\n      El programa dejará de funcionar.",
                                                      "Confirmación",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);
            // Verificar la respuesta del usuario
            if (result == MessageBoxResult.Yes)
            {
                // Mostrar un MessageBox para informar al usuario
                _ = MessageBox.Show("El sistema está por detenerse. Espere mientras se completa el proceso.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                base.OnClosed(e);

                Close();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }

    // Clase que representa un botón personalizado
    public class BotonPersonalizado : Button
    {
        public int Id { get; private set; }

        public BotonPersonalizado(string texto, int id)
        {
            Id = id;
            Content = texto;
            Margin = new Thickness(5);
            Background = Brushes.Azure;
            Click += Boton_Click;
        }

        private void Boton_Click(object sender, RoutedEventArgs e)
        {
            _ = MessageBox.Show($"Se presionó el botón {Id + 1}");
        }
    }
}
