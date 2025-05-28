using FusionAxion.Model;
using FusionAxion.Repositories;
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
        private readonly Controller controller;

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
        public string LabelBackground
        {
            get => LabelConnection.Background;
            set
            {
                LabelConnection.Background = value;
                OnPropertyChanged(nameof(LabelBackground));
            }
        }
        public string LabelContent
        {
            get => LabelConnection.Label;
            set
            {
                LabelConnection.Label = value;
                OnPropertyChanged(nameof(LabelContent));
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
        public ICommand VerProductosCommand { get; }
        public ICommand VerCierresCommand { get; }
        public ICommand VerifyConnectionCommand { get; }

        // Constructor
        public PanelFusionViewModel()
        {
            CloseCommand = new ViewModelCommand(ExecuteCloseCommand);
            CambiarConfigCommand = new ViewModelCommand(ExecuteCambiarConfigCommand);
            VerDespachosCommand = new ViewModelCommand(ExecuteVerDespachosCommand);
            VerSurtidoresCommand = new ViewModelCommand(ExecuteVerSurtidoresCommand);
            VerTanquesCommand = new ViewModelCommand(ExecuteVerTanquesCommand);
            VerProductosCommand = new ViewModelCommand(ExecuteVerProductosCommand);
            VerCierresCommand = new ViewModelCommand(ExecuteVerCierresCommand);
            VerifyConnectionCommand = new ViewModelCommand(ExecuteVerifyConnectionCommand);

            DataBase dataBase = new DataBase();
            controller = new Controller();

            LoadConfiguration();
        }

        private void ExecuteCloseCommand(object obj)
        {
            controller.EndProcess();

            Application.Current.Shutdown();
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

        private void ExecuteVerDespachosCommand(object obj)
        {
            DespachosView despachosView = new DespachosView();
            despachosView.Show();
        }

        private void ExecuteVerSurtidoresCommand(object obj)
        {
            SurtidoresView surtidoresView = new SurtidoresView();
            surtidoresView.Show();
        }

        private void ExecuteVerTanquesCommand(object obj)
        {
            TanquesView tanquesView = new TanquesView();
            tanquesView.Show();
        }

        private void ExecuteVerProductosCommand(object obj)
        {
            ProductosView productosView = new ProductosView();
            productosView.Show();
        }

        private void ExecuteVerCierresCommand(object obj)
        {
            CierresView cierresView = new CierresView();
            cierresView.Show();
        }

        private void ExecuteVerifyConnectionCommand(object obj)
        {
            UpdateLabelConnection();
        }

        private void LoadConfiguration()
        {
            Log.Instance.SetLogType(ConfigurationModel.GetConfiguration().Logger);

            if (CurrentData == null)
            {

                //Obtiene la configuracion de la estacion
                Log.Instance.WriteLog($"Obteniendo configuracion de la estación.\n", LogType.t_info);
                ControllerFusion.Instance.ConfigurarEstacion();

                //Genera los surtidores
                Log.Instance.WriteLog($"Generando botones para la vista.\n", LogType.t_info);
                GenerateSurtidoresButton(Station.Instance.NumeroDeSurtidores);
            }

            //Obtiene los parametros de configuracion.
            CurrentData = ConfigurationModel.GetConfiguration();
            controller.Init();
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

        public void UpdateLabelConnection()
        {
            if (ControllerFusion.Instance.CheckConnection())
            {
                Log.Instance.WriteLog($"Conexión: True.\n", LogType.t_info);
                LabelContent = "OnLine";
                LabelBackground = "#007816";
            }
            else
            {
                Log.Instance.WriteLog($"Coexión: False.\n", LogType.t_info);
                LabelContent = "OffLine";
                LabelBackground = "#b00000";
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
