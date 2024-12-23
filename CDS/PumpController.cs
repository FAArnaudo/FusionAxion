using System;
using System.Threading;
using System.Threading.Tasks;

namespace CDS
{
    public class PumpController
    {
        private IControllerProcess controllerProcess;           // Se almacena la instancia del proceso segun el controlador
        private Task task;                                      // Almacena la instancia del proceso que realizara la tarea
        private Data data;                                      // Información obtenida de la vista de configuración

        public PumpController() { }

        public bool InitProcess(Data data)
        {
            if (Data != null)
            {
                EndProcess();
            }

            CheckController(data);

            if (ControllerProcess != null)
            {
                Data = data;

                Task = Task.Run(() => ControllerProcess.RunProcess(Task));

                return true;
            }

            return false;
        }

        private void CheckController(Data data)
        {
            switch (data.Controller)
            {
                case "CEM-44":
                    ControllerProcess = new CemProcess
                    {
                        Data = data,
                        TokenSource = new CancellationTokenSource()
                    };
                    break;
                case "FUSION":
                    ControllerProcess = new FusionProcess
                    {
                        Data = data,
                        TokenSource = new CancellationTokenSource()
                    };
                    break;
                default:
                    Log.Instance.WriteLog($"Error al iniciar el nuevo proceso: Controlador no reconocido.", LogType.t_error);
                    ControllerProcess = null;
                    break;
            }
        }

        public void EndProcess()
        {
            ControllerProcess.TokenSource?.Cancel();

            Thread.Sleep(2000 * Convert.ToInt32(data.Timer));
        }

        public Data Data
        {
            get => data;
            set => data = value;
        }

        public IControllerProcess ControllerProcess
        {
            get => controllerProcess;
            set => controllerProcess = value;
        }

        public Task Task
        {
            get => task;
            set => task = value;
        }
    }
}