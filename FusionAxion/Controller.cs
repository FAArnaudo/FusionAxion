using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FusionAxion
{
    public class Controller
    {

        public Task MainProcess { get; set; } = null;
        public CancellationTokenSource CancellationToken { get; set; }
        public bool IsRunning { get; set; } = false;
        public bool HacerCierre { get; set; } = false;

        public Controller() { }

        public void Init()
        {
            if (MainProcess == null)
            {
                CancellationToken = new CancellationTokenSource();
                MainProcess = Task.Run(() => Run());
            }
        }

        private void Run()
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                IsRunning = true;
                try
                {
                    ControllerFusion.Instance.ActualizarTanques();
                    while (!HacerCierre)
                    {
                        foreach (Surtidor surtidor in Station.Instance.Surtidores)
                        {
                            ControllerFusion.Instance.GrabarDespachos(surtidor);

                            ControllerFusion.Instance.CheckDiscount();

                            if (HacerCierre)
                            {
                                break;
                            }

                            Thread.Sleep(Convert.ToInt32(ConfigurationModel.GetConfiguration().Timer) * 1000);
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    Log.Instance.WriteLog($"Proceso finalizado. Excepción: {ex.Message}\n", LogType.t_error);

                    _ = ControllerFusion.Instance.Disconect();
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteLog($"Error en el metodo Run del proceso principal. Excepcion: {ex.Message}\n", LogType.t_error);
                }
            }
            IsRunning = false;
        }

        public void EndProcess()
        {
            if (MainProcess != null)
            {
                ControllerFusion.Instance.IsCanceled = true;
                CancellationToken?.Cancel();

                while (IsRunning)
                {
                    Thread.Sleep(100);
                }

                MainProcess = null;
                CancellationToken = null;
            }
        }
    }
}
