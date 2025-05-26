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

        public Controller()
        {

        }

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
            while (CancellationToken.Token.IsCancellationRequested)
            {
                try
                {
                    foreach (Surtidor surtidor in Station.Instance.Surtidores)
                    {
                        ControllerFusion.Instance.GrabarDespachos(surtidor);
                    }
                }
                catch (TaskCanceledException ex)
                {
                    ControllerFusion.Instance.IsCanceled = false; ;
                    Log.Instance.WriteLog($"Proceso finalizado. Excepcion: {ex.Message}\n", LogType.t_error);
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteLog($"Error en el metodo Run del proceso principal. Excepcion: {ex.Message}\n", LogType.t_error);
                }
            }
        }

        public void EndProcess()
        {
            if (MainProcess != null)
            {
                CancellationToken?.Cancel();
                ControllerFusion.Instance.IsCanceled = true;

                MainProcess = null;
                CancellationToken = null;
            }
        }
    }
}
