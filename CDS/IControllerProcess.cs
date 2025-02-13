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
        void RunProcess(Task mainProcess);
        void StopProcess();
    }

    public class CemProcess : IControllerProcess
    {
        public CancellationTokenSource CancellationToken { get; set; }
        public ControllerCem ControllerCem { get; set; }
        public Data Data { get; set; }
        private bool IsRunning { get; set; }
        private bool HacerCierre { get; set; }

        public CemProcess()
        {
            CancellationToken = new CancellationTokenSource();
            IsRunning = false;
        }

        public void RunProcess(Task mainProcess)
        {

            ControllerCem = new ControllerCem(Data.Protocol);

            Log.Instance.WriteLog($"Nuevo proceso principal iniciado. ID: {mainProcess.Id}, Estado: {mainProcess.Status}.\n", LogType.t_info);

            while (!CancellationToken.Token.IsCancellationRequested)
            {
                IsRunning = true;

                try
                {
                    while (!ControllerCem.VerificarConexión())
                    {
                        Thread.Sleep(1000 * Convert.ToInt32(Data.Timer));
                        Log.Instance.WriteLog("Intentando establecer conexión.", LogType.t_debug);
                    }

                    ControllerCem.ConfigurarEstacion();
                    ControllerCem.ActualizarTanques();

                    HacerCierre = false;
                    Log.Instance.WriteLog($"Iniciando Lecturas...\n", LogType.t_info);

                    while (!HacerCierre && !CancellationToken.Token.IsCancellationRequested)
                    {
                        Log.Instance.WriteLog($"Estado del hilo {mainProcess.Id}: {mainProcess.Status}. TimerProcess {Data.Timer}\n", LogType.t_debug);

                        ControllerCem.GrabarDespachos();

                        CheckFlags();

                        Thread.Sleep(1000 * Convert.ToInt32(Data.Timer));
                    }

                    // Hacer el cierre
                    if (HacerCierre)
                    {
                        Log.Instance.WriteLog("Iniciando: Realizando corte de turno.\n", LogType.t_info);
                        ControllerCem.GrabarCierre();
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($"Estado del hilo {mainProcess.Id}: {mainProcess.Status} - Error en el loop del controlador.\n\t  Excepción: {e.Message}\n", LogType.t_error);
                }
            }

            Log.Instance.WriteLog($"Finalizando hilo: {mainProcess.Id}.", LogType.t_info);

            IsRunning = false;
        }

        public void CheckFlags()
        {
            DataTable flags = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT * FROM cierreBandera");

            HacerCierre = Convert.ToBoolean(flags.Rows[0][0]);

            bool actualizarTanques = Convert.ToBoolean(flags.Rows[0][1]);
            bool traerCierreAnterior = Convert.ToBoolean(flags.Rows[0][2]);

            if (actualizarTanques)
            {
                ControllerCem.ActualizarTanques();
            }

            if (traerCierreAnterior)
            {
                ControllerCem.TrtaerCierreAnterior();
            }
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
        private bool HacerCierre { get; set; }

        public FusionProcess()
        {
            CancellationToken = new CancellationTokenSource();
            IsRunning = false;
        }

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

                            if (CancellationToken.Token.IsCancellationRequested)
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
                ControllerFusion.CloseConnection();
            }

            Log.Instance.WriteLog($" Estado del hilo: {mainProcess.Id} - Finalizando.", LogType.t_info);

            _ = ConnectorSQLite.Instance.ExecuteNonQuery($"UPDATE CheckConnection SET isConnected = 0, fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}' WHERE idConnection = 1");

            IsRunning = false;
        }

        public void StopProcess()
        {
            CancellationToken?.Cancel();

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

        public void CheckFlags()
        {
            DataTable flags = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT * FROM cierreBandera");

            HacerCierre = Convert.ToBoolean(flags.Rows[0][0]);
        }
    }
}
