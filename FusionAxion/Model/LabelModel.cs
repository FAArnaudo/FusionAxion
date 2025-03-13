using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FusionAxion.Model
{
    public class LabelModel
    {
        // Properties
        public string Label { get; set; } = "Iniciando...";
        public string Background { get; set; } = "#91a6ab";
        public int Height { get; set; } = 50;
        public int Width { get; set; } = 120;

        public LabelModel() { }
    }
}
