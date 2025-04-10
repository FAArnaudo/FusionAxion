using FusionAxion.Model;
using FusionAxion.Views;
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
        private LabelModel labelConnection;
        private ConfigurationView configurationView;
        private bool isViewVisible = true;
        private bool isClosed = false;

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
        public bool IsViewVisible
        {
            get => isViewVisible;
            set
            {
                isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }

        // Commands
        public ICommand CloseCommand { get; }
        public ICommand CambiarConfigCommand { get; }
        public ICommand VerDespachosCommand { get; }
        public ICommand VerSurtidoresCommand { get; }
        public ICommand VerTanquesCommand { get; }

        // Constructor
        public PanelFusionViewModel()
        {
            CloseCommand = new ViewModelCommand(ExecuteCloseCommand);
            CambiarConfigCommand = new ViewModelCommand(ExecuteCambiarConfigCommand);
            VerDespachosCommand = new ViewModelCommand(ExecuteVerDespachosCommand);
            VerSurtidoresCommand = new ViewModelCommand(ExecuteVerSurtidoresCommand);
            VerTanquesCommand = new ViewModelCommand(ExecuteVerTanquesCommand);

            Log.Instance.WriteLog($"Comprobando existencia de configuracion.\n", LogType.t_info);

            if (ConfigurationModel.ExistConfiguracion())
            {
                Log.Instance.WriteLog($"Realizando conexión\n", LogType.t_info);

                ControllerFusion.Instance.Connect(ConfigurationModel.GetConfiguration().IP);
                LoadConfiguration();
            }
        }

        private void ExecuteVerTanquesCommand(object obj)
        {
            throw new NotImplementedException();
        }

        private void ExecuteVerSurtidoresCommand(object obj)
        {
            throw new NotImplementedException();
        }

        private void ExecuteVerDespachosCommand(object obj)
        {
            DespachosView despachosView = new DespachosView();
            despachosView.Show();
        }

        private void ExecuteCambiarConfigCommand(object obj)
        {
            Log.Instance.WriteLog($"Abriendo ventana de configuración\n", LogType.t_info);
            configurationView = new ConfigurationView();
            configurationView.Show();
            configurationView.IsVisibleChanged += ConfigurationView_IsVisibleChanged;
            configurationView.Closing += ConfigurationView_Closing;
            IsViewVisible = false;
        }

        private void ConfigurationView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosed = true;
        }

        private void ConfigurationView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            IsViewVisible = true;
            if (!isClosed)
            {
                configurationView.Close();
                Log.Instance.WriteLog($"Cargando configuración inicial.\n", LogType.t_info);
                LoadConfiguration();
            }
            isClosed = false;
        }

        private void LoadConfiguration()
        {
            //Obtiene los parametros de configuracion.
            CurrentData = ConfigurationModel.GetConfiguration();

            //Verifica la conexion y actualiza el label
            Log.Instance.WriteLog($"Actualizando Label Status.", LogType.t_info);
            UpdateLabelConnection();
            //Obtiene la configuracion de la estacion
            Log.Instance.WriteLog($"Obteniendo configuracion de la estación.\n", LogType.t_info);
            ControllerFusion.Instance.ConfigurarEstacion();
            //Genera los surtidores
            Log.Instance.WriteLog($"Generando botones para la vista.\n", LogType.t_info);
            GenerateSurtidoresButton(Station.Instance.NumeroDeSurtidores);
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

        private void UpdateLabelConnection()
        {
            if (ControllerFusion.Instance.CheckConnection())
            {
                Log.Instance.WriteLog($"Conexión: True.\n", LogType.t_info);
                LabelConnection.Label = "Controlador\nOnLine";
                LabelConnection.Background = "#007816";
            }
            else
            {
                Log.Instance.WriteLog($"Coexión: False.\n", LogType.t_info);
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

        private void ExecuteCloseCommand(object obj)
        {
            while (!ControllerFusion.Instance.Disconect()) { }

            Application.Current.Shutdown();
        }
    }
}
