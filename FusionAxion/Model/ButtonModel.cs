using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FusionAxion.Model
{
    public class ButtonModel
    {

        // Properties
        public string Label { get; set; } = "";
        public int Height { get; set; } = 90;
        public int Width { get; set; } = 60;
        public string Background { get; set; } = "#123456";
        public ICommand Command { get; set; }

        public ButtonModel() { }
    }
}
