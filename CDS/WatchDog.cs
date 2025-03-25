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

        public bool IsRunning { get => isRunning; set => isRunning = value; }
        public int StartTime { get; set; } = 10;        // 10 segundos
        public int ThresholdTime { get; set; } = 120;   // 120 segundos = 2 minutos

        public WatchDog(PumpController controller, IControllerProcess process)
        {
            pumpController = controller;
            this.process = process;
        }

        public void Start()
        {
            IsRunning = true;
            watchdogThread = new Thread(MonitorWorker)
            {
                IsBackground = true
            };
            watchdogThread.Start();
        }

        // WD tarda 1 minuto en comenzar controla el tiempo desde que 
        private void MonitorWorker()
        {
            while (IsRunning)
            {
                Thread.Sleep(1000 * StartTime);

                if ((DateTime.Now - process.LastExecutionTime).TotalSeconds > ThresholdTime)
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
            if (IsRunning)
            {
                IsRunning = false;
                watchdogThread?.Join();
            }
        }
    }
}
