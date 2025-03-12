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
        public ConfigurationView()
        {
            InitializeComponent();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Combinación de teclas Ctrl + Shift + E
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Key == Key.E)
                {
                    ComboBoxLOG.Visibility = ComboBoxLOG.Visibility != Visibility.Visible ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }

        public void CloseWindow()
        {
            Close(); // Cierra la ventana
        }
    }
}
