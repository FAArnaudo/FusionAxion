using FusionAxion.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FusionAxion.ViewModels
{
    public class PanelFusionViewModel : ViewModelBase
    {
        // Fields
        private DataModel currentData;
        private ObservableCollection<ButtonModel> surtidoresButton;
        private ObservableCollection<ButtonModel> tanquesButtons;
        private LabelModel labelConnection;

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
        public ObservableCollection<ButtonModel> SurtidoresButton
        {
            get => surtidoresButton;
            set
            {
                surtidoresButton = value;
                OnPropertyChanged(nameof(SurtidoresButton));
            }
        }
        public ObservableCollection<ButtonModel> TanquesButton
        {
            get => tanquesButtons;
            set
            {
                tanquesButtons = value;
                OnPropertyChanged(nameof(TanquesButton));
            }
        }
        public LabelModel LabelConnection
        {
            get
            {
                if (labelConnection == null)
                {
                    labelConnection = new LabelModel();
                }
                return labelConnection;
            }
            set
            {
                labelConnection = value;
                OnPropertyChanged(nameof(LabelConnection));
            }
        }

        // Commands
        public ICommand CloseCommand { get; }

        // Constructor
        public PanelFusionViewModel()
        {
            LoadCurrentData();
            CloseCommand = new ViewModelCommand(ExecuteCloseCommand);
        }

        private void ExecuteCloseCommand(object obj)
        {
            while (!ControllerFusion.Instance.Disconect()) { }

            Application.Current.Shutdown();
        }

        private void LoadCurrentData()
        {
            //Obtiene los parametros de configuracion.
            CurrentData = ConfigurationModel.GetConfiguration();
            //Verifica la conexion y actualiza el label
            UpdateLabelConnection();
            //Obtiene la configuracion de la estacion
            ControllerFusion.Instance.ConfigurarEstacion();
            //Genera los surtidores
            GenerateSurtidoresButton(Station.Instance.NumeroDeSurtidores);
            //Genera los tanques
            GenerateTanquesButton(Station.Instance.NumeroDeTanques);
        }

        private void GenerateSurtidoresButton(int count)
        {
            SurtidoresButton = new ObservableCollection<ButtonModel>();

            for (int i = 1; i <= count; i++)
            {
                SurtidoresButton.Add(new ButtonModel
                {
                    Label = $"Surtidor {i}",
                    Command = new ViewModelCommand(ExecuteButtonCommand),
                });
            }
        }

        private void GenerateTanquesButton(int count)
        {
            TanquesButton = new ObservableCollection<ButtonModel>();

            for (int i = 1; i <= count; i++)
            {
                TanquesButton.Add(new ButtonModel
                {
                    Label = $"Tanque {i}",
                    Command = new ViewModelCommand(ExecuteButtonCommand),
                });
            }
        }

        private void UpdateLabelConnection()
        {
            if (ControllerFusion.Instance.CheckConnection(CurrentData.IP))
            {
                LabelConnection.Label = "Controlador\nOnLine";
                LabelConnection.Background = "#007816";
            }
            else
            {
                LabelConnection.Label = "Controlador\nOffLine";
                LabelConnection.Background = "#b00000";
            }
        }

        private void ExecuteButtonCommand(object obj)
        {
            if (obj is ButtonModel button)
            {
                _ = MessageBox.Show($"Botón {button.Label} presionado");
            }
        }
    }
}
