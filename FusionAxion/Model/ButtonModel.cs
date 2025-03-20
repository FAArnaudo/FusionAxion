using FusionAxion.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FusionAxion.Model
{
    public class ButtonModel : ViewModelBase
    {
        private string backbround = "#123456";
        // Properties
        public string Label { get; set; } = "";
        public int Height { get; set; } = 100;
        public int Width { get; set; } = 70;
        public string Background
        {
            get => backbround;
            set
            {
                backbround = value;
                OnPropertyChanged(nameof(Background));
            }
        }
        public ICommand Command { get; set; }

        public ButtonModel() { }
    }
}
