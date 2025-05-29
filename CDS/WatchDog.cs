using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CDS
{
    public class WatchDog
    {
        private readonly IControllerProcess process;
        private Thread watchdogThread;

        public bool IsRunning { get; set; }
        public int StartTime { get; set; } = 10;                // 10 segundos
        public int ThresholdTime { get; set; } = 60 * 30;       // Minutos
        public int ThresholdReSend { get; set; } = 60 * 60 * 2; // 2 horas

        public WatchDog(IControllerProcess process)
        {
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

        // WD tarda {@StartTime} en comenzar a controlar el tiempo desde que inicia el sistema
        private void MonitorWorker()
        {
            while (IsRunning)
            {
                Thread.Sleep(1000 * StartTime);

                if ((DateTime.Now - process.LastExecutionTime).TotalSeconds > ThresholdTime)
                {
                    Log.Instance.WriteLog("Watchdog detectó que el programa no responde.\n", LogType.t_error);
                    RestartWorker();
                }
            }
        }

        private void RestartWorker()
        {
            string message = $"Watchdog detectó que el proceso no responde.\n" +
                             $"Estacion: {Configuration.GetConfiguration().RazonSocial}.\n" +
                             $"<p>Importante:</p>" +
                             $"<p>- Revisar la conexion con Posservice</p>" +
                             $"<p>- Verificar que respondan los botones del CDS</p>" +
                             $"<p>- {Station.Instance.GeneralMessage}</p>";

            string destinatario = ObtenerDestinatario();
            if (destinatario == null)
            {
                destinatario = "federico.arnaudo@sistemasiges.com.ar";
            }

            EnviarCorreo(destinatario, "Falla en CDS", message);
            Thread.Sleep(1000 * ThresholdReSend);
        }

        public void EnviarCorreo(string destinatario, string asunto, string cuerpo)
        {
            // Configuración del servidor SMTP
            string smtpServidor = "smtp.gmail.com"; // Cambia esto por el servidor SMTP que uses
            int puerto = 587; // El puerto típico es 587 (para TLS), 465 (para SSL) o 25 (para conexión sin cifrado)
            string usuario = "federico.arnaudo@sistemasiges.com.ar"; // Tu dirección de correo electrónico
            string contrasena = "ipmp txqk lomv jokg"; // Tu contraseña de correo electrónico

            try
            {
                // Crear el objeto SmtpClient con la configuración del servidor SMTP
                SmtpClient smtp = new SmtpClient(smtpServidor)
                {
                    Port = puerto,
                    Credentials = new NetworkCredential(usuario, contrasena),
                    EnableSsl = true // Habilita el uso de SSL para seguridad
                };

                // Crear el objeto MailMessage con la información del mensaje
                MailMessage mensaje = new MailMessage
                {
                    From = new MailAddress(usuario), // El remitente
                    Subject = asunto, // El asunto del correo
                    Body = cuerpo, // El cuerpo del correo
                    IsBodyHtml = true // Si el cuerpo es en formato HTML
                };

                // Agregar el destinatario
                mensaje.To.Add(destinatario);

                // Enviar el mensaje
                smtp.Send(mensaje);

                Log.Instance.WriteLog("Correo enviado con éxito.\n", LogType.t_info);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteLog($"Error al enviar correo. Excepcion: {ex.Message}.\n", LogType.t_error);
            }
        }

        public string ObtenerDestinatario()
        {
            try
            {
                string rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "Folders\\Destinatario.txt");

                if (File.Exists(rutaArchivo))
                {
                    using (StreamReader lector = new StreamReader(rutaArchivo))
                    {
                        string primeraLinea = lector.ReadLine();
                        return string.IsNullOrWhiteSpace(primeraLinea) ? null : primeraLinea;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
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
