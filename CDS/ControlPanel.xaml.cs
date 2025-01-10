using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using ContextMenu = System.Windows.Forms.ContextMenu;
using Image = System.Windows.Controls.Image;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;

namespace CDS
{
    /// <summary>
    /// Lógica de interacción para ControlPanel.xaml
    /// </summary>
    public partial class ControlPanel : Window
    {
        private NotifyIcon notifyIcon;
        private StackPanel stackPanel;
        private Label label;
        private DispatcherTimer timer;

        public Label Label { get; private set; }
        public PumpController PumpController { get; private set; }

        public ControlPanel()
        {
            InitializeComponent();
            PumpController = new PumpController();
            Loaded += ControlPanel_Loaded;
        }

        /// <summary>
        /// Inicia los componentes dinamicos y consulta por la configuración. Si no existe inicia una nueva,
        /// si ya equiste intenta iniciar la conexión con el controlador.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlPanel_Loaded(object sender, RoutedEventArgs e)
        {
            Icon = new BitmapImage(new Uri("pack://application:,,,/CDS;component/Images/LogoSurtidor.ico"));
            stackPanel = new StackPanel();
            label = new Label();

            CreateCustomLabel("Iniciando...", "#20B2AA");

            SetupNotifyIcon();

            if (!Configuration.ExistConfiguracion())
            {
                OpenConfigurationWindows();
            }
            else
            {
                Init();
            }
        }

        private void Init()
        {
            if (Configuration.ExistConfiguracion())
            {
                ConfigureExpander(Configuration.GetConfiguration().StationFlag, Configuration.GetConfiguration().Controller);

                if (PumpController.Data == null && ConnectorSQLite.Instance.CreateDatabase())
                {
                    if (PumpController.StartProcess(Configuration.GetConfiguration()))
                    {
                        _ = MessageBox.Show("Conexión iniciada, verifique el estado del controlador.");

                        timer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(5)  // El intervalo es de 5 segundos
                        };

                        timer.Tick += Timer_Tick;               // Event handler cuando el timer "hace tic"
                        timer.Start();                          // Iniciar el temporizador
                    }
                    else
                    {
                        _ = MessageBox.Show("Error al cargar los parametros.\n" +
                                        "Por favor, revise e intente nuevamente.");

                        Log.Instance.WriteLog("No fue posible iniciar el proceso", LogType.t_error);
                    }
                }
                else if (!PumpController.Data.Controller.Equals(Configuration.GetConfiguration().Controller))
                {
                    if (PumpController.UpdateProcess(Configuration.GetConfiguration()))
                    {
                        Log.Instance.WriteLog("Nueva conexión iniciada, verifique el estado del controlador.", LogType.t_info);
                    }
                    else
                    {
                        _ = MessageBox.Show("Error al cargar los parametros.\n" +
                                        "Por favor, revise e intente nuevamente.");

                        Log.Instance.WriteLog("No fue posible iniciar el proceso", LogType.t_error);
                    }
                }
                else
                {
                    if (PumpController.SetNewData(Configuration.GetConfiguration()))
                    {
                        _ = MessageBox.Show("Datos actualizados.");

                        Log.Instance.WriteLog("Datos actualizados.", LogType.t_info);
                    }

                }
            }
            else
            {
                Close();
            }
        }

        /// <summary>
        /// Inicia una nueva configuración. Al cerrarse la ventana de configuración, se vuelve a mostrar
        /// la ventana del panel de control y llama a iniciar la conexión.
        /// </summary>
        private void OpenConfigurationWindows()
        {
            Views.InitialConfiguration initialConfiguration = new Views.InitialConfiguration
            {
                Owner = this
            };
            initialConfiguration.Show();
            initialConfiguration.Closed += InitialConfiguration_Closed;
            Hide();
        }

        private void InitialConfiguration_Closed(object sender, EventArgs e)
        {
            Show();
            Init();
        }

        #region EVENTOS
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            // Mostrar un cuadro de diálogo de confirmación
            MessageBoxResult result = MessageBox.Show("¿Está seguro de que desea realizar esta acción?\n      El programa dejará de funcionar.",
                                                      "Confirmación",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);
            // Verificar la respuesta del usuario
            if (result == MessageBoxResult.Yes)
            {
                PumpController.EndProcess();
                notifyIcon.Dispose();
                base.OnClosed(e);
                Close();
            }
        }

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnCambiarConfig_Click(object sender, RoutedEventArgs e)
        {
            OpenConfigurationWindows();
        }

        private void BtnVerDespachos_Click(object sender, RoutedEventArgs e)
        {
            Views.DespachosViews despachosViews = new Views.DespachosViews();
            despachosViews.Show();
        }

        private void BtnVerSurtidores_Click(object sender, RoutedEventArgs e)
        {
            Views.SurtidoresViews surtidoresViews = new Views.SurtidoresViews();
            surtidoresViews.Show();
        }

        private void BtnVerTanques_Click(object sender, RoutedEventArgs e)
        {
            Views.TanquesViews tanquesViews = new Views.TanquesViews();
            tanquesViews.Show();
        }

        private void BtnVerProductos_Click(object sender, RoutedEventArgs e)
        {
            Views.ProductosViews productosViews = new Views.ProductosViews();
            productosViews.Show();
        }

        private void BtnVerCierres_Click(object sender, RoutedEventArgs e)
        {
            Views.CierresViews cierresViews = new Views.CierresViews();
            cierresViews.Show();
        }

        private void BtnCierreAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                _ = MessageBox.Show($"¡{clickedButton.Content} presionado!");
            }
        }

        private void BtnCIO_Click(object sender, RoutedEventArgs e)
        {
            Views.CIOViews cIOViews = new Views.CIOViews();
            cIOViews.Show();

            if (sender is Button clickedButton)
            {
                _ = MessageBox.Show($"¡{clickedButton.Content} presionado!");
            }
        }
        #endregion
        #region PROCESO EN SEGUNDO PLANO
        private void SetupNotifyIcon()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Images", "LogoSurtidor.ico")),
                Visible = false,
                Text = "Controlador De Surtidores"
            };

            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            ContextMenu contextMenu = new ContextMenu();
            _ = contextMenu.MenuItems.Add("Restaurar", (s, e) => RestoreFromTray());
            _ = contextMenu.MenuItems.Add("Salir", (s, e) => ExitApplication());

            notifyIcon.ContextMenu = contextMenu;
        }
        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            RestoreFromTray();
        }
        private void RestoreFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            notifyIcon.Visible = false;
        }
        private void ExitApplication()
        {
            notifyIcon.Visible = false;
            System.Windows.Application.Current.Shutdown();
        }
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            notifyIcon.Dispose();
            base.OnClosed(e);
        }
        #endregion
        #region ELEMENTS CREATED POO
        /// <summary>
        /// Crea y configura el expander dinamico que representa la bandera de la estación,
        /// asociada al controlador configurado.
        /// </summary>
        /// <param name="flagStation"></param>
        /// <param name="controller"></param>
        private void ConfigureExpander(string flagStation, string controller)
        {
            // Primero, limpiamos cualquier contenido previo en el StackPanel
            SPExpander.Children.Clear();
            stackPanel.Children.Clear();

            // Crear el Expander
            Expander expander = new Expander
            {
                Header = controller,
                IsExpanded = true,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#013220")),
                BorderThickness = new Thickness(2)
            };

            // Label
            label.Content = flagStation;
            label.Width = 176;
            label.Height = 35;
            label.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.FontSize = 20;
            label.Foreground = Brushes.White;
            label.Margin = new Thickness(2);

            if (flagStation.Equals("YPF"))
            {
                SolidColorBrush solidColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007ED6"));

                label.Background = solidColorBrush;

                // Crear un TextBlock
                TextBlock textBlock = new TextBlock
                {
                    Text = "Datos CIO",
                    Height = 16,
                    Width = 56,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2)
                };

                // Crear el primer botón
                Button btnCIO = new Button
                {
                    Width = 110,
                    Height = 35,
                    Background = solidColorBrush,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Style = (Style)Resources["Styles"],
                    Margin = new Thickness(2)
                };

                btnCIO.Click += BtnCIO_Click;

                // Crear un objeto Image
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Images/Data.ico", UriKind.RelativeOrAbsolute)) // Establece la ruta de la imagen
                };

                btnCIO.Content = image;

                // Crear el segundo botón
                Button btnCierreAnterior = new Button
                {
                    Content = "Cierre Anterior",
                    Foreground = Brushes.White,
                    FontSize = 15,
                    Width = 110,
                    Height = 35,
                    Background = solidColorBrush,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Style = (Style)Resources["Styles"],
                    Margin = new Thickness(2)
                };

                btnCierreAnterior.Click += BtnCierreAnterior_Click;

                _ = stackPanel.Children.Add(label);
                _ = stackPanel.Children.Add(textBlock);
                _ = stackPanel.Children.Add(btnCIO);
                _ = stackPanel.Children.Add(btnCierreAnterior);
            }
            else if (flagStation.Equals("AXION"))
            {
                label.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B008B"));
                expander.IsEnabled = false;
                _ = stackPanel.Children.Add(label);
            }
            else if (flagStation.Equals("PUMA"))
            {
                label.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A50000"));
                expander.IsEnabled = false;
                _ = stackPanel.Children.Add(label);
            }

            expander.Content = stackPanel;

            _ = SPExpander.Children.Add(expander);
        }

        /// <summary>
        /// Se ejecuta el metodo que comprueba la conexión con el controlador, segun lo guardado en la base de datos.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            string state = "Controlador OnLine";
            string color = "#00FF00";               // Green Color

            if (!ConnectorSQLite.Instance.ExecuteStateQuery("SELECT isConnected FROM CheckConnection WHERE idConnection = 1"))
            {
                state = "Controlador OffLine";
                color = "#FF0000";                 // Red Color
            }

            if (Label != null && !Label.Content.Equals(state))
            {
                CreateCustomLabel(state, color);
            }
        }

        /// <summary>
        /// Se encarga de actualizar la leyenda del label con su color de fondo. Es util para saber el estado de la conexión entre
        /// el sistema y el controlador.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="colorHex"></param>
        public void CreateCustomLabel(string content, string colorHex)
        {
            DockPanelBot.Children.Clear();

            // Inicializa el Label
            Label = new Label
            {
                // Establece el contenido del Label
                Content = content,
                Height = 50,
                Width = 170,

                // Cambia el color de fondo usando el valor hexadecimal recibido
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),

                // Personalizacion de otras propiedades.
                FontSize = 16,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Padding = new Thickness(10),
                Foreground = Brushes.White
            };
            _ = DockPanelBot.Children.Add(Label);
        }
        #endregion
    }
}
