using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CDS
{
    public interface IControllerProcess
    {
        CancellationTokenSource CancellationToken { get; set; }
        Data Data { get; set; }
        void RunProcess(Task mainProcess);
        void StopProcess();
    }

    public class CemProcess : IControllerProcess
    {
        public CancellationTokenSource CancellationToken { get; set; }
        public ControllerCem ControllerCem { get; set; }
        public Data Data { get; set; }

        private bool IsRunning { get; set; }

        public CemProcess()
        {
            CancellationToken = new CancellationTokenSource();
            IsRunning = false;
        }

        public void RunProcess(Task mainProcess)
        {
            ControllerCem = new ControllerCem(Data.IP, Data.Protocol);

            Log.Instance.WriteLog($"Nuevo proceso principal iniciado. ID: {mainProcess.Id}, Estado: {mainProcess.Status}.\n", LogType.t_info);

            while (!CancellationToken.Token.IsCancellationRequested)
            {
                IsRunning = true;

                try
                {
                    Log.Instance.WriteLog($" Estado del hilo {mainProcess.Id}: {mainProcess.Status}. TimerProcess {Data.Timer}\n", LogType.t_info);

                    Thread.Sleep(Convert.ToInt32(1000 * Convert.ToInt32(Data.Timer)));
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($" Estado del hilo {mainProcess.Id}: {mainProcess.Status} - Error en el loop del controlador.\n\t  Excepción: {e.Message}\n", LogType.t_error);
                }
            }

            Log.Instance.WriteLog($" Estado del hilo: {mainProcess.Id} - Finalizando.", LogType.t_info);

            IsRunning = false;
        }

        public void StopProcess()
        {
            CancellationToken?.Cancel();

            while (IsRunning) { }

            Log.Instance.WriteLog($" Proceso Finalizado.\n", LogType.t_info);
        }
    }

    public class FusionProcess : IControllerProcess
    {
        public CancellationTokenSource CancellationToken { get; set; }
        public ControllerFusion ControllerFusion { get; set; }
        public Data Data { get; set; }
        private bool IsRunning { get; set; }

        public FusionProcess()
        {
            CancellationToken = new CancellationTokenSource();
            IsRunning = false;
        }

        public void RunProcess(Task mainProcess)
        {
            ControllerFusion = new ControllerFusion(Data.IP, Data.StationFlag);

            Log.Instance.WriteLog($"Nuevo proceso principal iniciado. ID: {mainProcess.Id}, Estado: {mainProcess.Status}.\n", LogType.t_info);

            while (!CancellationToken.Token.IsCancellationRequested)
            {
                IsRunning = true;

                try
                {
                    Log.Instance.WriteLog($" Estado del hilo {mainProcess.Id}: {mainProcess.Status}. TimerProcess {Data.Timer}\n", LogType.t_info);

                    Thread.Sleep(Convert.ToInt32(1000 * Convert.ToInt32(Data.Timer)));
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($" Estado del hilo {mainProcess.Id}: {mainProcess.Status} - Error en el loop del controlador.\n\t  Excepción: {e.Message}\n", LogType.t_error);
                }
            }

            Log.Instance.WriteLog($" Estado del hilo: {mainProcess.Id} - Finalizando.", LogType.t_info);

            IsRunning = false;
        }

        public void StopProcess()
        {
            CancellationToken?.Cancel();

            while (IsRunning == true)
            {

            }

            Log.Instance.WriteLog($" Proceso Finalizado.\n", LogType.t_info);
        }
    }
}
