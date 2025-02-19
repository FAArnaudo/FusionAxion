using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion
{
    public class Log
    {
        private static Log instance = null;
        private LogType logLevel = LogType.t_info;
        // Un objeto que se utilizará para la sincronización
        private static readonly object _lockObject = new object();
        private Log() { }
        public static Log Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Log();
                }
                return instance;
            }
        }

        public LogType GetLogLevel()
        {
            return logLevel;
        }

        public void SetLogType(string logType)
        {
            switch (logType)
            {
                case "t_debug":
                    logLevel = LogType.t_debug;
                    break;
                case "t_info":
                    logLevel = LogType.t_info;
                    break;
                case "t_error":
                    logLevel = LogType.t_error;
                    break;
                default:
                    logLevel = LogType.t_info;
                    break;
            }
        }

        public void WriteLog(string message, LogType type)
        {
            try
            {
                // Crea la carpeta si no existe, en la carpeta base del programa.
                string path = AppDomain.CurrentDomain.BaseDirectory + "/Log";
                string logFile = "log" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";

                if (!Directory.Exists(path))
                {
                    _ = Directory.CreateDirectory(path);
                }

                // Borra el log del dia anterior
                string deleteFile = "log" + DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0)).ToString("dd-MM-yyyy") + ".txt";
                if (File.Exists(Environment.CurrentDirectory + "/Log/" + deleteFile))
                {
                    File.Delete(Environment.CurrentDirectory + "/Log/" + deleteFile);
                }

                // Log message with correspondant log level
                switch (type)
                {
                    case LogType.t_info:
                        if (GetLogLevel() <= type)
                        {
                            lock (_lockObject)
                            {
                                using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, logFile), true))
                                {
                                    outputFile.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  INFO:    " + message);
                                }
                            }
                        }
                        break;
                    case LogType.t_debug:
                        if (GetLogLevel() <= type)
                        {
                            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, logFile), true))
                            {
                                outputFile.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  DEBUG:   " + message);
                            }
                        }
                        break;
                    case LogType.t_error:
                        if (GetLogLevel() <= type)
                        {
                            lock (_lockObject)
                            {
                                using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, logFile), true))
                                {
                                    outputFile.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  ERROR:   " + message);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error al escribir el reporte en el log. Excepcion: {e.Message}");
            }
        }
    }

    public enum LogType
    {
        t_debug = 0,
        t_info,
        t_error
    }
}
