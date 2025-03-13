using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading;
using System.Security.Principal;
using FusionAxion.Model;

namespace FusionAxion.ViewModels
{
    public class ConfigurationViewModel : ViewModelBase
    {
        // Fields
        private string razonSocial;
        private string rutaProyNuevo;
        private string ip;
        private string timer;
        private readonly List<string> timerOptions;
        private int currentIndex = 0;
        private LogType logger;
        private string statusMessage = "";
        private ObservableCollection<string> items;
        private string selectedItem = LogType.t_info.ToString();
        private bool isViewVisible = true;

        // Properties
        public string RazonSocial
        {
            get => razonSocial;
            set
            {
                razonSocial = value;
                OnPropertyChanged(nameof(RazonSocial));
            }
        }
        public string RutaProyNuevo
        {
            get => rutaProyNuevo;
            set
            {
                rutaProyNuevo = value;
                OnPropertyChanged(nameof(RutaProyNuevo));
            }
        }
        public string IP
        {
            get => ip;
            set
            {
                ip = value;
                OnPropertyChanged(nameof(IP));
            }
        }
        public string Timer
        {
            get => timer;
            set
            {
                timer = value;
                OnPropertyChanged(nameof(Timer));
            }
        }
        public string Logger
        {
            get => logger.ToString();
            set
            {
                switch (value.ToString())
                {
                    case "t_debug":
                        logger = LogType.t_debug;
                        break;
                    case "t_info":
                        logger = LogType.t_info;
                        break;
                    case "t_error":
                        logger = LogType.t_error;
                        break;
                    default:
                        logger = LogType.t_info;
                        break;
                }
                OnPropertyChanged(nameof(Logger));
            }
        }
        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
        public ObservableCollection<string> Items
        {
            get => items;
            set
            {
                items = value;
                OnPropertyChanged(nameof(Items));
            }
        }
        public string SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
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

        //-> Commands
        public ICommand BuscarCommand { get; }
        public ICommand UpCommand { get; }
        public ICommand DownCommand { get; }
        public ICommand SaveConfigurationCommand { get; }
        public ICommand StatusMessageCommand { get; }

        // Constructor
        public ConfigurationViewModel()
        {
            timerOptions = new List<string> { "2", "4", "6", "8", "10", "12" };
            Items = new ObservableCollection<string>
            {
                "t_debug",
                "t_info",
                "t_error"
            };

            BuscarCommand = new ViewModelCommand(ExecuteBuscarCommand);
            UpCommand = new ViewModelCommand(ExecuteUpCommand);
            DownCommand = new ViewModelCommand(ExecuteDownCommand);
            SaveConfigurationCommand = new ViewModelCommand(ExecuteSaveConfigurationCommand, CanExecuteSaveConfigurationCommand);

            ShowData();
        }

        private void ExecuteBuscarCommand(object obj)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.MyComputer, // Carpeta raíz (opcional)
                Description = "Selecciona una carpeta" // Descripción del diálogo (opcional)
            };

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string folderPath = folderBrowserDialog.SelectedPath;
                RutaProyNuevo = folderPath;
            }
        }

        private void ExecuteUpCommand(object obj)
        {
            if (currentIndex < timerOptions.Count - 1)
            {
                currentIndex++;
                UpdateTextBox();
            }
        }

        private void ExecuteDownCommand(object obj)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                UpdateTextBox();
            }
        }

        private void UpdateTextBox()
        {
            Timer = timerOptions[currentIndex];
        }

        private bool CanExecuteSaveConfigurationCommand(object obj)
        {
            bool validData = false;
            if (!string.IsNullOrWhiteSpace(RazonSocial))
            {
                if (!string.IsNullOrWhiteSpace(RutaProyNuevo) && RutaProyNuevo.Trim().ToLower().EndsWith(@"sistema\proy_nuevo"))
                {
                    if (!string.IsNullOrWhiteSpace(IP))
                    {
                        validData = true;
                    }
                }
            }

            return validData;
        }

        private void ExecuteSaveConfigurationCommand(object obj)
        {
            DataModel data = new DataModel
            {
                RazonSocial = RazonSocial,
                RutaProyNuevo = RutaProyNuevo,
                IP = IP,
                Timer = Timer,
                Logger = Logger
            };

            Log.Instance.SetLogType(Logger.ToString());

            if (ControllerFusion.Instance.CheckConnection(data.IP))
            {
                if (ConfigurationModel.SaveConfiguration(data))
                {
                    StatusMessage = "Configuración guardada correctamente.";
                    IsViewVisible = false;
                }
                else
                {
                    StatusMessage = "La configuracion no pudo ser guardada.\nIntente reiniciando.";
                }
            }
            else
            {
                StatusMessage = "*Conexión no establecida con el controlador Fusion\nVerifique la dirección IP.";
            }
        }

        private void ShowData()
        {
            if (!ConfigurationModel.ExistConfiguracion())
            {
                UpdateTextBox();
            }
            else
            {
                DataModel data = ConfigurationModel.GetConfiguration();

                RazonSocial = data.RazonSocial;
                RutaProyNuevo = data.RutaProyNuevo;
                IP = data.IP;
                Timer = data.Timer;
                Logger = data.Logger;
            }
            StatusMessage = "";
        }
    }

}
