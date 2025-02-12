using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using CheckBox = System.Windows.Controls.CheckBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using System.Threading;

namespace CDS.Views
{
    /// <summary>
    /// Lógica de interacción para InitialConfiguration.xaml
    /// </summary>
    public partial class InitialConfiguration : Window
    {
        private int currentIndex = 0;
        private int protocol;
        public InitialConfiguration()
        {
            InitializeComponent();
            Loaded += InitialConfiguration_Loaded;
        }

        private void InitialConfiguration_Loaded(object sender, RoutedEventArgs e)
        {
            if (Configuration.ExistConfiguracion())
            {
                Data data = Configuration.GetConfiguration();

                TextBoxRazonSocial.Text = data.RazonSocial;
                TextBoxRutaProyecto.Text = data.RutaProyNuevo;
                switch (data.StationFlag)
                {
                    case "YPF":
                        ComboBoxStation.SelectedIndex = 0;
                        break;
                    case "AXION":
                        ComboBoxStation.SelectedIndex = 1;
                        break;
                    case "PUMA":
                        ComboBoxStation.SelectedIndex = 2;
                        break;
                    default:
                        ComboBoxStation.SelectedIndex = 0;   //YPF
                        break;
                }
                TextBoxIpControlador.Text = data.IP;

                protocol = Convert.ToInt32(data.Protocol);
                if (protocol == 16)
                {
                    CheckBoxProtocol16.IsChecked = true;
                }
                else if (protocol == 32)
                {
                    CheckBoxProtocol32.IsChecked = true;
                }
                TextBoxTimer.Text = data.Timer;
                switch (data.Modo)
                {
                    case "NORMAL":
                        ComboBoxMODO.SelectedIndex = 0;
                        break;
                    case "TEST":
                        ComboBoxMODO.SelectedIndex = 1;
                        break;
                    default:
                        ComboBoxMODO.SelectedIndex = 0;     //NORMAL
                        break;
                }

                switch (data.Logger)
                {
                    case "t_debug":
                        ComboBoxLOG.SelectedIndex = 0;
                        break;
                    case "t_info":
                        ComboBoxLOG.SelectedIndex = 1;
                        break;
                    case "t_error":
                        ComboBoxLOG.SelectedIndex = 2;
                        break;
                    default:
                        ComboBoxLOG.SelectedIndex = 1;   //t_info
                        break;
                }
            }
            else
            {
                ComboBoxStation.SelectedIndex = 0;              // YPF
                listBox.SelectedIndex = 1;                      // 4 segundos
            }
            ComboBoxMODO.Visibility = Visibility.Hidden;
            ComboBoxLOG.Visibility = Visibility.Hidden;
        }
        private void ComboBoxStation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem estacion = (ComboBoxItem)ComboBoxStation.SelectedItem;

            ComboBoxController.SelectedIndex = estacion.Content.ToString().Equals("YPF") ? 0 : 1;
        }
        private void ComboBoxController_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxController.SelectedIndex == 0)
            {
                ExpanderFusion.Visibility = Visibility.Hidden;
                ExpanderFusion.IsEnabled = false;
                ExpanderFusion.IsExpanded = false;
                ExpanderCEM.Visibility = Visibility.Visible;
                ExpanderCEM.IsEnabled = true;
                ExpanderCEM.IsExpanded = true;
            }
            else
            {
                ExpanderCEM.Visibility = Visibility.Hidden;
                ExpanderCEM.IsEnabled = false;
                ExpanderCEM.IsExpanded = false;
                ExpanderFusion.Visibility = Visibility.Visible;
                ExpanderFusion.IsEnabled = false;
                ExpanderFusion.IsExpanded = true;
            }
        }
        private void BtnRutaPN_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.MyComputer, // Carpeta raíz (opcional)
                Description = "Selecciona una carpeta" // Descripción del diálogo (opcional)
            };

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = folderBrowserDialog.SelectedPath;
                TextBoxRutaProyecto.Text = folderPath;
            }
        }
        private void BtnInit_Click(object sender, RoutedEventArgs e)
        {
            if (CheckParameters())
            {
                ComboBoxItem controlador = (ComboBoxItem)ComboBoxController.SelectedItem;
                ComboBoxItem estacion = (ComboBoxItem)ComboBoxStation.SelectedItem;
                ComboBoxItem modo = (ComboBoxItem)ComboBoxMODO.SelectedItem;
                ComboBoxItem logger = (ComboBoxItem)ComboBoxLOG.SelectedItem;
                MessageBoxResult result = MessageBox.Show($"Datos ingresador:\n\tRazon Social:\t{TextBoxRazonSocial.Text}" +
                                                     $"\n\tRuta Proyecto:\t{TextBoxRutaProyecto.Text}" +
                                                     $"\n\tBandera:\t\t{estacion.Content}" +
                                                     $"\n\tControlador:\t{controlador.Content}" +
                                                     $"\n\tIP Controlador:\t{TextBoxIpControlador.Text}" +
                                                     $"\n\tProtocolo:\t\t{protocol}" +
                                                     $"\n\tTimer de proceso:\t{TextBoxTimer.Text}\n" +
                                                     $"\n¿Los datos son correctos?", "Confirmación",
                                                          MessageBoxButton.YesNo,
                                                          MessageBoxImage.Question);
                // Verificar la respuesta del usuario
                if (result == MessageBoxResult.Yes)
                {
                    Data data = new Data
                    {
                        RazonSocial = TextBoxRazonSocial.Text,
                        RutaProyNuevo = TextBoxRutaProyecto.Text,
                        StationFlag = estacion.Content.ToString(),
                        Controller = controlador.Content.ToString(),
                        IP = TextBoxIpControlador.Text,
                        Protocol = protocol.ToString(),
                        Timer = TextBoxTimer.Text,
                        Modo = modo.Content.ToString(),
                        Logger = logger.Content.ToString()
                    };
                    Log.Instance.SetLogType(logger.ToString());

                    if (Configuration.SaveConfiguration(data))
                    {
                        Thread.Sleep(500);
                        _ = MessageBox.Show($"Configuracion guardada correctamente.");
                        Close();
                    }
                    else
                    {
                        _ = MessageBox.Show($"La configuracion no pudo ser guardada. Intente ejecutar el programa nuevamente.");
                        Close();
                    }
                }
            }
        }
        private bool CheckParameters()
        {
            bool parametersOk = false;
            if (TextBoxRazonSocial.Text != "")
            {
                if (TextBoxRutaProyecto.Text != "" && TextBoxRutaProyecto.Text.Trim().ToLower().EndsWith(@"sistema\proy_nuevo"))
                {
                    if (TextBoxIpControlador.Text != "")
                    {
                        ComboBoxItem station = (ComboBoxItem)ComboBoxStation.SelectedItem;
                        protocol = 0;
                        if (station.Content.ToString().Equals("YPF"))
                        {
                            if ((bool)CheckBoxProtocol16.IsChecked)
                            {
                                protocol = 16;
                                parametersOk = true;
                            }
                            else if ((bool)CheckBoxProtocol32.IsChecked)
                            {
                                protocol = 32;
                                parametersOk = true;
                            }
                            else
                            {
                                _ = MessageBox.Show($"Ha seleccionado bandera YPF y controlador CEM-44, debe seleccionar un protocolo.");
                            }
                        }
                        else
                        {
                            parametersOk = true;
                        }
                    }
                    else
                    {
                        _ = MessageBox.Show($"Debe ingresar la IP del Controlador para avanzar.");
                    }
                }
                else
                {
                    _ = MessageBox.Show($"Debe ingresar la Ruta del Proyecto para avanzar.");
                }
            }
            else
            {
                _ = MessageBox.Show($"Debe ingresar la Razon Social de la estación para avanzar.");
            }
            return parametersOk;
        }
        #region INFO BTNS
        private void BtnInfoProtocolo_Click(object sender, RoutedEventArgs e)
        {
            string info = $"El protocolo indica la cantidad maxima\n" +
                          $"de surtidores que puede tener la estacion.\n" +
                          $"Iniciar con 16, en su defecto, con 32.";
            _ = MessageBox.Show(info);
        }

        private void BtnInfoIP_Click(object sender, RoutedEventArgs e)
        {
            string info = $"La IP se debe consultar con la administración de la Estación\n";
            _ = MessageBox.Show($"{info}");
        }

        private void RazonSocialInfo_Click(object sender, RoutedEventArgs e)
        {
            string info = $"Debe ingresar la razon social de la Estacion\n" +
                          $"con el siguiente formato: Sistema SIGES S.A.\n";
            _ = MessageBox.Show($"{info}");
        }

        private void ControladorInfo_Click(object sender, RoutedEventArgs e)
        {
            string info = $"Debe seleccionar el controlador instalado en la estación\n";
            _ = MessageBox.Show($"{info}");
        }
        #endregion
        #region TIMER
        private void Up_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex < listBox.Items.Count - 1)
            {
                currentIndex++;
                UpdateTextBox();
            }
        }

        private void Down_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                UpdateTextBox();
            }
        }

        private void UpdateTextBox()
        {
            TextBoxTimer.Text = ((ListBoxItem)listBox.Items[currentIndex]).Content.ToString();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                currentIndex = listBox.SelectedIndex;
                UpdateTextBox();
                listBox.Visibility = Visibility.Collapsed;
            }
        }
        #endregion
        #region ComboBox
        private void CheckBoxProtocol16_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBoxProtocol16 = (CheckBox)sender;
            CheckBoxProtocol16.IsChecked = true;
            CheckBoxProtocol32.IsChecked = false;
        }
        private void CheckBoxProtocol32_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBoxProtocol32 = (CheckBox)sender;
            CheckBoxProtocol32.IsChecked = true;
            CheckBoxProtocol16.IsChecked = false;
        }
        private void CheckBoxProtocol16_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBoxProtocol16 = (CheckBox)sender;
            CheckBoxProtocol16.IsChecked = false;
            CheckBoxProtocol32.IsChecked = true;
        }
        private void CheckBoxProtocol32_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBoxProtocol32 = (CheckBox)sender;
            CheckBoxProtocol32.IsChecked = false;
            CheckBoxProtocol16.IsChecked = true;
        }
        #endregion
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Combinación de teclas Ctrl + Shift + E
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Key == Key.E)
                {
                    if (ComboBoxMODO.Visibility != Visibility.Visible)
                    {
                        ComboBoxMODO.Visibility = Visibility.Visible;
                        ComboBoxLOG.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ComboBoxMODO.Visibility = Visibility.Hidden;
                        ComboBoxLOG.Visibility = Visibility.Hidden;
                    }
                }
            }
        }
    }
}
