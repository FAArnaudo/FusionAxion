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

namespace FusionAxion.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PanelFusion : Window
    {
        private ConfigurationView configurationView;
        public PanelFusion()
        {
            InitializeComponent();
            //Loaded += PanelFusion_Loaded;
        }

        private void PanelFusion_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ConfigurationModel.ExistConfiguracion())
            {
                configurationView = new ConfigurationView
                {
                    Owner = this
                };
                configurationView.Show();
                configurationView.IsVisibleChanged += ConfigurationView_IsVisibleChanged;
                Hide();
            }
        }

        private void ConfigurationView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Show();
            configurationView.Close();
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
    }
}
