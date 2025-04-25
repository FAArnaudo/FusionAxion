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
        void RunProcess(Task mainProcess, WatchDog watchDog);
        void StopProcess();
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

        public void RunProcess(Task mainProcess, WatchDog watchDog)
        {
            CreateController();

            Log.Instance.WriteLog($"Nuevo proceso principal iniciado. ID: {mainProcess.Id}, Estado: {mainProcess.Status}, TimerProcess {Data.Timer}.\n", LogType.t_info);

            while (!CancellationToken.Token.IsCancellationRequested)
            {
                IsRunning = true;

                Log.Instance.WriteLog($"Verificando conexion.\n", LogType.t_debug);

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
                            LastExecutionTime = DateTime.Now; // Actualiza el tiempo de ejecución

                            ControllerFusion.GrabarDespachos();

                            foreach (Surtidor surtidor in Station.Instance.Surtidores)
                            {
                                ControllerFusion.GrabarDespachos(surtidor);

                                ControllerFusion.CheckDiscount();

                                CheckFlags();

                                if (CancellationToken.Token.IsCancellationRequested || HacerCierre)
                                    break;

                                Thread.Sleep(1000 * Convert.ToInt32(Data.Timer));
                            }

                            Thread.Sleep(Convert.ToInt32(1000 * Convert.ToInt32(Data.Timer)));
                        }

                        // Hacer el cierre
                        if (HacerCierre)
                        {
                            Log.Instance.WriteLog("Iniciando: Realizando corte de turno.\n", LogType.t_info);
                            ControllerFusion.GrabarCierre();
                            HacerCierre = false;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Instance.WriteLog($" Estado del hilo {mainProcess.Id}: {mainProcess.Status} - Error en el loop del controlador.Excepción: {e.Message}\n", LogType.t_error);
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
