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
    public partial class PanelFusion : Window
    {
        private Grid GButtons;
        private DispatcherTimer timer;
        private readonly int numeroDeSurtidores = 12;
        private int surtidorActual = 0;

        public PanelFusion()
        {
            InitializeComponent();
            Loaded += PanelFusion_Loaded;
        }

        private void PanelFusion_Loaded(object sender, RoutedEventArgs e)
        {
            GButtons = new Grid
            {
                Height = 320,
                Width = 520,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#ECF0F1"),
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            if (!Configuration.ExistConfiguracion())
            {
                OpenConfigurationWindows();
            }
            else
            {
                Init();
            }
        }

        /// <summary>
        /// Inicia una nueva configuración. Al cerrarse la ventana de configuración, se vuelve a mostrar
        /// la ventana del panel de control y llama a iniciar la conexión.
        /// </summary>
        private void OpenConfigurationWindows()
        {
            Views.ConfigurationView configurationView = new Views.ConfigurationView
            {
                Owner = this
            };
            configurationView.Show();
            configurationView.Closed += ConfigurationView_Closed;
            Hide();
        }

        private void ConfigurationView_Closed(object sender, EventArgs e)
        {
            Show();
            Init();
        }

        private void Init()
        {
            CrearBotones();
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
                BotonPersonalizado boton = new BotonPersonalizado($"Surtidor {i + 1}", i);
                Grid.SetRow(boton, i / 6);
                Grid.SetColumn(boton, i % 6);
                _ = GButtons.Children.Add(boton);
            }

            _ = GMain.Children.Add(GButtons);

            Content = GMain;
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
                boton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#90EE90");

                await Task.Delay(3000);

                boton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#ADD8E6");
            }

            surtidorActual++;

            if (surtidorActual >= numeroDeSurtidores)
            {
                surtidorActual = 0;
            }
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

        private void BtnCambiarConfig_Click(object sender, RoutedEventArgs e)
        {

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
            Height = 100;
            Width = 60;
            Margin = new Thickness(5);
            Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#ADD8E6");
            Click += Boton_Click;
        }

        private void Boton_Click(object sender, RoutedEventArgs e)
        {
            _ = MessageBox.Show($"Se presionó el botón {Id + 1}");
        }
    }
}
