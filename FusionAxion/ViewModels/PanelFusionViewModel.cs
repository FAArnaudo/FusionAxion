using FusionAxion.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion.ViewModels
{
    public class PanelFusionViewModel : ViewModelBase
    {
        // Fields
        private DataModel currentData;
        private ObservableCollection<ButtonViewModel> buttons;

        // Properties
        public DataModel CurrentData
        {
            get => currentData;
            set
            {
                currentData = value;
                OnPropertyChanged(nameof(CurrentData));
            }
        }

        public ObservableCollection<ButtonViewModel> Buttons
        {
            get => buttons;
            set
            {
                buttons = value;
                OnPropertyChanged(nameof(Buttons));
            }
        }

        // Constructor
        public PanelFusionViewModel()
        {
            LoadCurrentData();
            ConfigureButtons();
        }

        private void LoadCurrentData()
        {
            if (ConfigurationModel.ExistConfiguracion())
            {
                CurrentData = ConfigurationModel.GetConfiguration();
            }
        }

        private void ConfigureButtons()
        {
            Buttons = new ObservableCollection<ButtonViewModel>();

            int MAX_ROW = 4;
            int MAX_COLUMNS = 8;
            int buttonNumber = 1;

            for (int row = 0; row < MAX_ROW; row++)
            {
                for (int column = 0; column < MAX_COLUMNS; column++)
                {
                    Buttons.Add(new ButtonViewModel
                    {
                        ButtonText = $"Botón {buttonNumber}",
                        ButtonCommand = new ViewModelCommand(ExecuteButtonCommand),
                        RowIndex = row,
                        ColumnIndex = column
                    });
                    buttonNumber++;
                }
            }
        }

        private void ExecuteButtonCommand(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
