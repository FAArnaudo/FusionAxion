using FusionAxion.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FusionAxion.Model
{
    public class LabelModel : ViewModelBase
    {
        private string label = "Esperando\nConfiguración";
        private string background = "#91A6AB";
        // Properties
        public string Label
        {
            get => label;
            set
            {
                label = value;
                OnPropertyChanged(nameof(Label));
            }
        }
        public string Background
        {
            get => background;
            set
            {
                background = value;
                OnPropertyChanged(nameof(Background));
            }
        }

        public LabelModel() { }
    }
}
