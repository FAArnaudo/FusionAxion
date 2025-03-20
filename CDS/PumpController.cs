using System;
using System.Threading.Tasks;

namespace CDS
{
    public class PumpController
    {
        private WatchDog watchdog;
        public PumpController()
        {
            ControllerProcess = null;
            Task = null;
            Data = null;
        }

        /// <summary>
        /// StartProcess inicia el proceso que se conecta con el controlador indicado y
        /// le provee la información necesaria de la configuración.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool StartProcess(Data data)
        {
            try
            {
                Data = data;

                CheckController();

                watchdog = new WatchDog(this, ControllerProcess);
                watchdog.Start();

                return ControllerProcess != null;
            }
            catch (NullReferenceException e)
            {
                Log.Instance.WriteLog($"Error al Iniciar un nuevo proceso. Excepción: {e.Message}", LogType.t_error);
                return false;
            }
            catch (ArgumentNullException e)
            {
                Log.Instance.WriteLog($"Error al Iniciar un nuevo proceso. Excepción: {e.Message}", LogType.t_error);
                return false;
            }
        }

        /// <summary>
        /// UpdateProcess actualiza el procesador corriendo actualmente. Le pone fin al proceso anterior
        /// e inicia uno nuevo actual y diferente.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool UpdateProcess(Data data)
        {
            try
            {
                Data = data;

                EndProcess();

                CheckController();

                return ControllerProcess != null;
            }
            catch (NullReferenceException e)
            {
                Log.Instance.WriteLog($"Error al actualizar el proceso. Excepción: {e.Message}", LogType.t_error);
                return false;
            }
            catch (ArgumentNullException e)
            {
                Log.Instance.WriteLog($"Error al actualizar el proceso. Excepción: {e.Message}", LogType.t_error);
                return false;
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error al actualizar el proceso. Excepción: {e.Message}", LogType.t_error);
                return false;
            }
        }

        public void RestartProcess()
        {
            UpdateProcess(Data);
        }

        private void CheckController()
        {
            switch (Data.Controller)
            {
                case "CEM-44":
                    ControllerProcess = new CemProcess
                    {
                        Data = Data,
                    };

                    Task = Task.Run(() => ControllerProcess.RunProcess(Task));

                    break;
                case "FUSION":
                    ControllerProcess = new FusionProcess
                    {
                        Data = Data,
                    };

                    Task = Task.Run(() => ControllerProcess.RunProcess(Task));

                    break;
                default:
                    Log.Instance.WriteLog($"Error al iniciar el proceso: Controlador no reconocido.", LogType.t_error);
                    ControllerProcess = null;
                    break;
            }
        }

        /// <summary>
        /// SetNewData mantiene el controlador actual pero actualiza los datos de configuración.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SetNewData(Data data)
        {
            try
            {
                Data = data;
                ControllerProcess.Data = data;

                return true;
            }
            catch (NullReferenceException e)
            {
                Log.Instance.WriteLog($"Error al actualizar el proceso. Excepción: {e.Message}", LogType.t_error);
                return false;
            }
            catch (ArgumentNullException e)
            {
                Log.Instance.WriteLog($"Error al actualizar el proceso. Excepción: {e.Message}", LogType.t_error);
                return false;
            }
        }

        public void EndProcess()
        {
            if (ControllerProcess != null)
            {
                ControllerProcess.StopProcess();
            }

            ControllerProcess = null;
        }
        public void Stop()
        {
            watchdog.Stop();
            ControllerProcess.StopProcess();
        }

        public Data Data { get; set; }

        private IControllerProcess ControllerProcess { get; set; }

        public Task Task { get; set; }
    }
}