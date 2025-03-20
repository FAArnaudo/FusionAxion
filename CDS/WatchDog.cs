using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CDS
{
    public class WatchDog
    {
        private readonly IControllerProcess process;
        private readonly PumpController pumpController;
        private Thread watchdogThread;
        private bool isRunning;

        public WatchDog(PumpController controller, IControllerProcess process)
        {
            pumpController = controller;
            this.process = process;
            this.process.WorkerFailed += RestartWorker; // Se suscribe al evento de fallo
        }

        public void Start()
        {
            isRunning = true;
            watchdogThread = new Thread(MonitorWorker)
            {
                IsBackground = true
            };
            watchdogThread.Start();
        }

        private void MonitorWorker()
        {
            while (isRunning)
            {
                Thread.Sleep(10000); // Verifica cada 10 segundos

                if ((DateTime.Now - process.LastExecutionTime).TotalSeconds > 60)
                {
                    Log.Instance.WriteLog("Watchdog detectó que el proceso no responde. Reiniciando...\n", LogType.t_error);
                    RestartWorker();
                }
            }
        }

        private void RestartWorker()
        {
            Log.Instance.WriteLog("Reiniciando proceso principal...\n", LogType.t_info);
            pumpController.RestartProcess();
        }

        public void Stop()
        {
            isRunning = false;
            watchdogThread?.Join();
        }
    }
}
