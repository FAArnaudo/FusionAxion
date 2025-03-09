using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FusionAxion
{
    // Clase que representa un botón personalizado
    public class BotonPersonalizado : Button
    {
        public int Id { get; private set; }

        public BotonPersonalizado(string texto, int id)
        {
            Id = id;
            Content = texto;
            Height = 80;
            Width = 60;
            Margin = new Thickness(5);
            Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#ADD8E6");
            Click += Boton_Click;
        }

        private void Boton_Click(object sender, RoutedEventArgs e)
        {
            _ = MessageBox.Show($"Se presionó el botón {Content}");
        }
    }
}
