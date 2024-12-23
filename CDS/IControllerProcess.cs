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
        CancellationTokenSource TokenSource { get; set; }
        void RunProcess(Task mainProcess);
        Data Data { get; set; }
    }

    public class CemProcess : IControllerProcess
    {
        public Data Data { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        public ControllerCem ControllerCem { get; set; }

        public CemProcess() { }

        public void RunProcess(Task mainProcess)
        {
            int timerProcess = Convert.ToInt32(Data.Timer);

            ControllerCem = new ControllerCem(Data.IP, Data.Protocol);

            Log.Instance.WriteLog($"Nuevo proceso principal iniciado. ID: {mainProcess.Id}, Estado: {mainProcess.Status}.\n", LogType.t_info);

            while (!TokenSource.IsCancellationRequested)
            {
                try
                {
                    bool cierreDetectado = false;

                    while (!cierreDetectado && !TokenSource.IsCancellationRequested)
                    {
                        Thread.Sleep(1000 * timerProcess);              // Timer in seconds

                        cierreDetectado = ConnectorSQLite.Instance.ExecuteStateQuery($"SELECT hacerCierre FROM cierreBandera LIMIT 1");
                    }

                    if (cierreDetectado)
                    {
                        Log.Instance.WriteLog($"Pedido de Cierre detectado...\n", LogType.t_info);
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($" Estado del hilo: {mainProcess.Status} - Error en el loop del controlador.\n\t  Excepción: {e.Message}\n", LogType.t_error);
                }
            }

            Log.Instance.WriteLog($" Estado del hilo: {mainProcess.Status} - Finalizando.", LogType.t_info);
        }
    }

    public class FusionProcess : IControllerProcess
    {
        public Data Data { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        public ControllerFusion ControllerFusion { get; set; }

        public FusionProcess() { }

        public void RunProcess(Task mainProcess)
        {
            ControllerFusion = new ControllerFusion(Data.IP);

            int timerProcess = Convert.ToInt32(Data.Timer);

            Log.Instance.WriteLog($"Nuevo proceso principal iniciado. ID: {mainProcess.Id}, Estado: {mainProcess.Status}.\n", LogType.t_info);

            while (!TokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    bool cierreDetectado = false;

                    while (!cierreDetectado && !TokenSource.IsCancellationRequested)
                    {
                        Thread.Sleep(1000 * timerProcess);              // Timer in seconds

                        cierreDetectado = ConnectorSQLite.Instance.ExecuteStateQuery($"SELECT hacerCierre FROM cierreBandera LIMIT 1");
                    }

                    if (cierreDetectado)
                    {
                        Log.Instance.WriteLog($"Pedido de Cierre detectado...\n", LogType.t_info);
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($" Estado del hilo: {mainProcess.Status} - Error en el loop del controlador.\n\t  Excepción: {e.Message}\n", LogType.t_error);
                }
            }

            Log.Instance.WriteLog($" Estado del hilo: {mainProcess.Status} - Finalizando.", LogType.t_info);
        }
    }
}
