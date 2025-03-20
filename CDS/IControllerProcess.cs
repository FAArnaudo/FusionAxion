using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CDS
{
    public interface IControllerProcess
    {
        CancellationTokenSource CancellationToken { get; set; }
        Data Data { get; set; }
        DateTime LastExecutionTime { get; set; }
        void RunProcess(Task mainProcess);
        void StopProcess();

        event Action WorkerFailed; // Evento para notificar fallos al Watchdog
    }

    public class FusionProcess : IControllerProcess
    {
        public CancellationTokenSource CancellationToken { get; set; }
        public ControllerFusion ControllerFusion { get; set; }
        public Data Data { get; set; }
        private bool IsRunning { get; set; }
        public static bool HacerCierre { get; set; }
        public static bool BreakProces { get; set; }
        public DateTime LastExecutionTime { get; set; }

        public FusionProcess()
        {
            CancellationToken = new CancellationTokenSource();
            IsRunning = false;
            BreakProces = false;
        }

        public event Action WorkerFailed;

        public void RunProcess(Task mainProcess)
        {
            CreateController();

            Log.Instance.WriteLog($"Nuevo proceso principal iniciado. ID: {mainProcess.Id}, Estado: {mainProcess.Status}, TimerProcess {Data.Timer}.\n", LogType.t_info);

            while (!CancellationToken.Token.IsCancellationRequested)
            {
                IsRunning = true;

                if (ControllerFusion.VerificarConexión())
                {
                    ControllerFusion.ConfigurarEstacion();
                    ControllerFusion.ActualizarTanques();

                    HacerCierre = false;
                    Log.Instance.WriteLog($"Iniciando Lecturas...\n", LogType.t_info);

                    try
                    {
                        while (!HacerCierre && !CancellationToken.Token.IsCancellationRequested)
                        {
                            ControllerFusion.GrabarDespachos();

                            if (CancellationToken.Token.IsCancellationRequested || HacerCierre)
                            {
                                continue;
                            }

                            ControllerFusion.CheckDiscount();

                            CheckFlags();

                            Thread.Sleep(Convert.ToInt32(1000 * Convert.ToInt32(Data.Timer)));
                        }

                        // Hacer el cierre
                        if (HacerCierre)
                        {
                            Log.Instance.WriteLog("Iniciando: Realizando corte de turno.\n", LogType.t_info);
                            ControllerFusion.GrabarCierre();
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Instance.WriteLog($" Estado del hilo {mainProcess.Id}: {mainProcess.Status} - Error en el loop del controlador.\n\t  Excepción: {e.Message}\n", LogType.t_error);
                    }
                }
                else
                {
                    Log.Instance.WriteLog($"Conexión inicial fallida. No se pudo establecer comunicación con el controlador", LogType.t_error);
                }
            }

            ControllerFusion.CloseConnection();

            Log.Instance.WriteLog($" Estado del hilo: {mainProcess.Id} - Finalizando.", LogType.t_info);

            _ = ConnectorSQLite.Instance.ExecuteNonQuery($"UPDATE CheckConnection SET isConnected = 0, fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}' WHERE idConnection = 1");

            IsRunning = false;
        }

        public void StopProcess()
        {
            CancellationToken?.Cancel();
            BreakProces = true;

            while (IsRunning)
            {
                Thread.Sleep(100);
            }

            Log.Instance.WriteLog($" Proceso Finalizado.\n", LogType.t_info);
        }

        public void CreateController()
        {
            ICommunication communication;
            switch (Data.StationFlag)
            {
                case "AXION":
                    communication = new AxionConnector();
                    break;
                case "PUMA":
                    communication = new PumaConnector();
                    break;
                default:
                    communication = new AxionConnector();
                    break;
            }

            ControllerFusion = new ControllerFusion(communication);
        }

        public static void CheckFlags()
        {
            DataTable flags = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT * FROM cierreBandera");

            HacerCierre = Convert.ToBoolean(flags.Rows[0][0]);
        }
    }
}
