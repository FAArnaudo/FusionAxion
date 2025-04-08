using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CDS
{
    public class CemProcess : IControllerProcess
    {
        public CancellationTokenSource CancellationToken { get; set; }
        public ControllerCem ControllerCem { get; set; }
        public Data Data { get; set; }
        private bool IsRunning { get; set; }
        private bool HacerCierre { get; set; }
        public DateTime LastExecutionTime { get; set; }

        public CemProcess()
        {
            CancellationToken = new CancellationTokenSource();
            IsRunning = false;
            HacerCierre = false;
        }

        public void RunProcess(Task mainProcess, WatchDog watchDog)
        {
            ControllerCem = new ControllerCem(Data.Protocol);

            Log.Instance.WriteLog($"Nuevo proceso principal iniciado. ID: {mainProcess.Id}, Estado: {mainProcess.Status}.\n", LogType.t_info);

            if (!watchDog.IsRunning)
            {
                watchDog.Start();
            }

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

                    Log.Instance.WriteLog($"Iniciando Lecturas...\n", LogType.t_info);

                    while (!HacerCierre && !CancellationToken.Token.IsCancellationRequested)
                    {
                        LastExecutionTime = DateTime.Now; // Actualiza el tiempo de ejecución

                        ControllerCem.GrabarDespachos();

                        CheckFlags();

                        Thread.Sleep(1000 * Convert.ToInt32(Data.Timer));

                        Log.Instance.WriteLog($"Estado del hilo {mainProcess.Id}: {mainProcess.Status}. TimerProcess {Data.Timer}. Tiempo de bucle: {DateTime.Now - LastExecutionTime}\n", LogType.t_debug);
                    }

                    // Hacer el cierre
                    if (HacerCierre)
                    {
                        Log.Instance.WriteLog("Iniciando: Corte de turno.\n", LogType.t_info);
                        ControllerCem.GrabarCierre();
                        HacerCierre = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($"Estado del hilo {mainProcess.Id}: {mainProcess.Status} - Error en el loop del controlador.\n\t  Excepción: {e.Message}\n", LogType.t_error);
                    HacerCierre = false;
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
                Log.Instance.WriteLog("Iniciando: Actualización de tanques.\n", LogType.t_info);
                ControllerCem.ActualizarTanques();
            }

            if (traerCierreAnterior)
            {
                Log.Instance.WriteLog("Iniciando: Información del corte anterior.\n", LogType.t_info);
                ControllerCem.TrtaerCierreAnterior();
            }
        }

        public void StopProcess()
        {
            CancellationToken?.Cancel();
            int time = 0;

            while (IsRunning)
            {
                Thread.Sleep(1000);
                time++;
                Log.Instance.WriteLog($"Esperando finalizacion del proceso.\n", LogType.t_info);

                if (time == 10)
                {
                    IsRunning = false;

                }
            }

            Log.Instance.WriteLog($" Proceso Finalizado.\n", LogType.t_info);
        }
    }
}
