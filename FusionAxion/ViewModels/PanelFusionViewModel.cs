using FusionAxion.Model;
using FusionAxion.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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

        private CancellationTokenSource cts;
        private bool pausado;

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

        // Constructor
        public PanelFusionViewModel()
        {
            CloseCommand = new ViewModelCommand(ExecuteCloseCommand);
            CambiarConfigCommand = new ViewModelCommand(ExecuteCambiarConfigCommand);

            Log.Instance.WriteLog($"Comprobando existencia de configuracion.\n", LogType.t_debug);

            if (ConfigurationModel.ExistConfiguracion())
            {
                Log.Instance.WriteLog($"Configuracion encontrada. Realizando conexión\n", LogType.t_debug);

                ControllerFusion.Instance.Connect(ConfigurationModel.GetConfiguration().IP);
                Init();
            }
        }

        private void ExecuteCambiarConfigCommand(object obj)
        {
            Log.Instance.WriteLog($"Abriendo ventana de configuración\n", LogType.t_debug);
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
            }

            isClosed = false;
            Init();
        }

        private void LoadConfiguration()
        {
            //Obtiene los parametros de configuracion.
            CurrentData = ConfigurationModel.GetConfiguration();

            //Verifica la conexion y actualiza el label
            Log.Instance.WriteLog($"Actualizando Label Status.", LogType.t_debug);
            UpdateLabelConnection();

            //Obtiene la configuracion de la estacion
            Log.Instance.WriteLog($"Obteniendo configuracion de la estación.\n", LogType.t_debug);
            ControllerFusion.Instance.ConfigurarEstacion();

            //Genera los surtidores
            Log.Instance.WriteLog($"Generando botones para la vista.\n", LogType.t_debug);
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
            bool isConnected = ControllerFusion.Instance.CheckConnection();

            if (isConnected)
            {
                LabelConnection.Label = "Controlador\nOnLine";
                LabelConnection.Background = "#007816";
            }
            else
            {
                LabelConnection.Label = "Controlador\nOffLine";
                LabelConnection.Background = "#b00000";
            }

            Log.Instance.WriteLog($"Coexión: {isConnected}.\n", LogType.t_debug);
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

        private async Task Init()
        {
            try
            {
                Log.Instance.WriteLog($"Cargando configuración.\n", LogType.t_debug);
                LoadConfiguration();

                Task surtidoresControll = IniciarRecorridoAsync();

                // Espera ambas tareas (esto mantiene la aplicación en ejecución)
                await Task.WhenAll(surtidoresControll);
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"{e.Message}", LogType.t_error);
            }
        }

        public async Task IniciarRecorridoAsync()
        {
            cts = new CancellationTokenSource();
            pausado = false;

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (pausado)
                    {
                        await Task.Delay(500); // Espera antes de volver a comprobar
                        continue;
                    }

                    foreach (ButtonModel button in SurtidoresButton)
                    {
                        if (cts.Token.IsCancellationRequested || pausado) break;

                        button.Background = "#5ad9e1";  // Cambiar color
                        await RealizarProcesoAsync(button);
                        button.Background = "#123456";  // Restaurar color
                        await Task.Delay(500);          // Pequeña pausa
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Se canceló la tarea, salir sin error
            }
        }

        private async Task RealizarProcesoAsync(ButtonModel button)
        {
            await Task.Run(() =>
            {
                // Aquí iría la lógica de procesamiento del botón
                Thread.Sleep(1000); // Simulación de proceso pesado
            });
        }

        public void PausarRecorrido()
        {
            pausado = true;
        }

        public void ReanudarRecorrido()
        {
            pausado = false;
        }

        public void DetenerRecorrido()
        {
            cts?.Cancel();
        }
    }
}
