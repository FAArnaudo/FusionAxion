using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Threading;

namespace FusionAxion.Views
{
    /// <summary>
    /// Lógica de interacción para ConfigurationView.xaml
    /// </summary>
    public partial class ConfigurationView : Window
    {
        private int currentIndex = 0;
        public ConfigurationView()
        {
            InitializeComponent();
            Loaded += ConfigurationView_Loaded;
        }

        private void ConfigurationView_Loaded(object sender, RoutedEventArgs e)
        {
            if (Configuration.ExistConfiguracion())
            {
                Data data = Configuration.GetConfiguration();

                TextBoxRazonSocial.Text = data.RazonSocial;
                TextBoxRutaProyecto.Text = data.RutaProyNuevo;
                TextBoxIpControlador.Text = data.IP;
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
                        ComboBoxMODO.SelectedIndex = 0; // NORMAL
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
                        ComboBoxLOG.SelectedIndex = 1;  // t_info
                        break;
                }
            }
            else
            {
                listBox.SelectedIndex = 1;              // 4 segundos
            }
            ComboBoxMODO.Visibility = Visibility.Hidden;
            ComboBoxLOG.Visibility = Visibility.Hidden;
        }

        private void BtnInit_Click(object sender, RoutedEventArgs e)
        {
            if (CheckParameters())
            {
                ComboBoxItem modo = (ComboBoxItem)ComboBoxMODO.SelectedItem;
                ComboBoxItem logger = (ComboBoxItem)ComboBoxLOG.SelectedItem;
                MessageBoxResult result = MessageBox.Show($"Datos ingresador:\n\tRazon Social:\t{TextBoxRazonSocial.Text}" +
                                                     $"\n\tRuta Proyecto:\t{TextBoxRutaProyecto.Text}" +
                                                     $"\n\tIP Controlador:\t{TextBoxIpControlador.Text}" +
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
                        IP = TextBoxIpControlador.Text,
                        Timer = TextBoxTimer.Text,
                        Modo = modo.Content.ToString(),
                        Logger = logger.Content.ToString()
                    };
                    Log.Instance.SetLogType(logger.Content.ToString());

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
                        parametersOk = true;
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

        private void RazonSocialInfo_Click(object sender, RoutedEventArgs e)
        {
            string info = $"Debe ingresar la razon social de la Estacion\n" +
                          $"con el siguiente formato: Sistema SIGES S.A.\n";
            _ = MessageBox.Show($"{info}");
        }

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
    }
}
