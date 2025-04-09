using System;
using System.Collections.Generic;
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
        private readonly PumpController pumpController;
        private Thread watchdogThread;
        private bool isRunning;

        public bool IsRunning { get => isRunning; set => isRunning = value; }
        public int StartTime { get; set; } = 10;        // 10 segundos
        public int ThresholdTime { get; set; } = 60 * 10; // Minutos
        public int ThresholdReSend { get; set; } = 60 * 5; // Minutos

        public WatchDog(PumpController controller, IControllerProcess process)
        {
            pumpController = controller;
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

        // WD tarda 1 minuto en comenzar controla el tiempo desde que 
        private void MonitorWorker()
        {
            while (IsRunning)
            {
                Thread.Sleep(1000 * StartTime);

                if ((DateTime.Now - process.LastExecutionTime).TotalSeconds > ThresholdTime)
                {
                    Log.Instance.WriteLog("Watchdog detectó que el proceso no responde.\n", LogType.t_error);
                    RestartWorker();
                }
            }
        }

        private void RestartWorker()
        {
            //Log.Instance.WriteLog("Reiniciando proceso principal...\n", LogType.t_info);
            //pumpController.RestartProcess();
            string message = $"Watchdog detectó que el proceso no responde.\n" +
                             $"Estacion: {Configuration.GetConfiguration().RazonSocial}.\n" +
                             $"Revisar la conexion con Posservice antes de reiniciar el CDS.";
            EnviarCorreo("federico.arnaudo@sistemasiges.com.ar", "CDS", message);
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
