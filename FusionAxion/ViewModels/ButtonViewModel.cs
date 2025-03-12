using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FusionAxion.ViewModels
{
    public class ButtonViewModel : ViewModelBase
    {
        // Fields
        public string buttonText;
        public int rowIndex;
        public int columnIndex;

        // Properties
        public string ButtonText
        {
            get => buttonText;
            set
            {
                buttonText = value;
                OnPropertyChanged(nameof(ButtonText));
            }
        }
        public int RowIndex
        {
            get => rowIndex;
            set
            {
                rowIndex = value;
                OnPropertyChanged(nameof(RowIndex));
            }
        }
        public int ColumnIndex
        {
            get => columnIndex;
            set
            {
                columnIndex = value;
                OnPropertyChanged(nameof(ColumnIndex));
            }
        }

        // Commands
        public ICommand ButtonCommand { get; set; }
    }
}
