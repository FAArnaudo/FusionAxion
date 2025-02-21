﻿using System;
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
        public PanelFusion()
        {
            InitializeComponent();
            Loaded += PanelFusion_Loaded;
        }

        #region CONFIGURACION INICIAL

        private void PanelFusion_Loaded(object sender, RoutedEventArgs e)
        {
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
            if (Configuration.ExistConfiguracion())
            {
                Show();
                Init();
            }
            else
            {
                Close();
            }
        }

        private void Init()
        {

        }

        #endregion

        #region BUTTONS

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
                ControllerFusion.Instance.CloseConnection();
                base.OnClosed(e);
                Close();
            }
        }

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnCambiarConfig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnVerifyConfig_Click(object sender, RoutedEventArgs e)
        {
            if (ControllerFusion.Instance.CheckConnection())
            {
                UpdateLabel("Controlador\nOnLine", "#00FF00");
            }
            else
            {
                UpdateLabel("Controlador\nOffLine", "#E57373");
            }
        }

        private void UpdateLabel(string state, string color)
        {
            //string green = "#00FF00";
            //string red = "#E57373";

            LStatus.Content = state;
            LStatus.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(color);
        }

        #endregion

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
