using Polly;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDS
{
    public class ConnectorCem
    {
        private readonly byte separador = 0x7E;
        private readonly string pipeName = "CEM44POSPIPE";
        private string ipController;
        private string protocol;

        public string IpController { get => ipController; set => ipController = value; }
        public string Protocol { get => protocol; set => protocol = value; }

        public ConnectorCem() { }

        public bool PoleoEnLinea()
        {
            return false;
        }

        public Station ComandoConfiguracionDeLaEstacion()
        {
            return null;
        }

        public Tank ComandoStockDeTanques()
        {
            return null;
        }

        public Despacho ComandoInformacionDeDespacho()
        {
            return null;
        }

        public CierreDeTurno ComandoCierresDeTurno()
        {
            return null;
        }

        private byte[] EnviarComando(byte[] comando)
        {
            byte[] buffer = null;
            NamedPipeClientStream pipeClient = null;

            try
            {
                int retries = 1;

                // Política de reintentos
                PolicyResult policyResult = Policy.Handle<Exception>()
                    .WaitAndRetry(retryCount: 4,
                                  sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)),
                                  onRetry: (exception, TimeSpan, conttext) =>
                                  {
                                      // Cerrar el pipe en caso de fallo
                                      if (pipeClient != null)
                                      {
                                          pipeClient.Dispose();
                                          pipeClient = null; // Limpiar el pipe para la nueva conexión
                                      }
                                      Log.Instance.WriteLog($"\n\t  Excepción: {exception.Message.Trim()} Intento: {retries}", LogType.t_error);
                                      retries++;
                                  }).ExecuteAndCapture(() =>
                                  {
                                      // Crear el pipeClient si está cerrado
                                      if (pipeClient == null)
                                      {
                                          pipeClient = new NamedPipeClientStream(ipController, pipeName);
                                      }

                                      // Conectar con tiempo de espera
                                      pipeClient.Connect(5000);

                                      // Enviar el comando
                                      pipeClient.Write(comando, 0, comando.Length);

                                      // Leer respuesta
                                      buffer = new byte[pipeClient.OutBufferSize];
                                      _ = pipeClient.Read(buffer, 0, buffer.Length);
                                  });
                // Verificación de resultado de conexión
                if (policyResult.Outcome != 0)
                {
                    Log.Instance.WriteLog($"  Fin de intentos...\n", LogType.t_error);
                    ReloadData();
                }
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error al enviar comando. Excepcón: {e.Message}", LogType.t_error);
            }
            finally
            {
                // Asegurarse de cerrar el pipe al final
                if (pipeClient != null)
                {
                    pipeClient.Dispose();
                }
            }

            return buffer;
        }

        public void ReloadData()
        {
            ipController = Configuration.GetConfiguration().IP;
            protocol = Configuration.GetConfiguration().Protocol;
        }
    }

}
