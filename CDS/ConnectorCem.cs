using Polly;
using System;
using System.Collections.Generic;
using System.IO;
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
        private IConnections connections;

        public string IpController { get => ipController; set => ipController = value; }
        public string Protocol { get => protocol; set => protocol = value; }
        public IConnections Connections { get => connections; set => connections = value; }

        public ConnectorCem(IConnections connections)
        {
            Connections = connections;
        }

        public bool PoleoEnLinea(byte[] command)
        {
            int confirmation = 0;
            byte[] reply;

            try
            {
                if (Configuration.GetConfiguration().Modo.Equals(MODO.NORMAL.ToString()))
                {
                    reply = Connections.EnviarComando(command);

                    if (!File.Exists(Environment.CurrentDirectory + "Reply\\poleo.txt"))
                    {
                        SaveAnswer(reply, "poleo");
                    }
                }
                else
                {
                    reply = ReadAnswer("poleo");
                }

                if (reply[confirmation] == 0x0)
                {
                    return true;
                }

                return false;
            }
            catch (FileNotFoundException e)
            {
                throw new Exception($"Error al obtener una respuesta guardada: Excepción {e.Message}");
            }
            catch (Exception e)
            {
                throw new Exception($"Error al obtener la conexión con el controlador CEM: Excepción {e.Message}");
            }
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

        public void SaveAnswer(byte[] respuesta, string nombreArchivo)
        {
            nombreArchivo = string.Concat(nombreArchivo.Split(Path.GetInvalidFileNameChars())) + ".txt";

            string directorio = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reply");
            if (!Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            string rutaCompleta = Path.Combine(directorio, nombreArchivo);

            if (!File.Exists(rutaCompleta))
            {
                using (StreamWriter sw = File.AppendText(rutaCompleta))
                {
                    int cont = 0;
                    for (int iteraciones = 0; iteraciones < respuesta.Length; iteraciones++)
                    {
                        sw.WriteLine(respuesta[iteraciones].ToString("X2")); // Escribe en formato hexadecimal

                        if (iteraciones > 0)
                        {
                            if (respuesta[iteraciones] == 0 && respuesta[iteraciones - 1] == 0 && cont < 6)
                            {
                                cont++;
                            }
                            else if (respuesta[iteraciones] == 0 && cont >= 6)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        /*
         * Se utiliza para testear las respuestas reales del Cem-44
         * se lee un .txt que contiene las respuestas y las guarda en un byte,
         * para simular la respuesta.
         */

        public byte[] ReadAnswer(string nombreArchivo)
        {
            // Obtener la ruta del directorio donde se ejecuta el programa
            string directorioEjecucion = AppDomain.CurrentDomain.BaseDirectory;

            // Combinar la ruta del directorio con el nombre del archivo
            string rutaArchivo = Path.Combine(directorioEjecucion, "Reply", nombreArchivo + ".txt");

            // Verificar si el archivo existe
            if (!File.Exists(rutaArchivo))
            {
                throw new FileNotFoundException($"El archivo '{rutaArchivo}' no existe.");
            }

            // Leer todas las líneas del archivo
            string[] lines = File.ReadAllLines(rutaArchivo);

            // Lista para almacenar los bytes leídos
            List<byte> byteList = new List<byte>();

            // Procesar cada línea del archivo
            foreach (string line in lines)
            {
                // Dividir la línea en valores numéricos individuales
                string[] numericValues = line.Split(',');

                // Convertir cada valor numérico en un byte y agregarlo a la lista
                foreach (string value in numericValues)
                {
                    if (byte.TryParse(value.Trim(), out byte parsedValue))
                    {
                        byteList.Add(parsedValue);
                    }
                    else
                    {
                        throw new FormatException($"El valor '{value}' no es un byte válido.");
                    }
                }
            }

            // Convertir la lista a un arreglo de bytes y retornarlo
            return byteList.ToArray();
        }
    }

    public interface IConnections
    {
        byte[] EnviarComando(byte[] comando);
    }

    public class CemCommunication : IConnections
    {
        public CemCommunication() { }

        public byte[] EnviarComando(byte[] comando)
        {
            throw new NotImplementedException();
        }
    }
}
