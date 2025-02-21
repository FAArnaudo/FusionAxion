using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion
{
    public class Configuration
    {
        private static readonly string configFile = Environment.CurrentDirectory + "/Config.ini";
        public Configuration() { }

        public static Data GetConfiguration()
        {
            Data data = null;

            try
            {
                StreamReader streamReader = new StreamReader(configFile);
                data = new Data
                {
                    RazonSocial = streamReader.ReadLine().Trim(),
                    RutaProyNuevo = streamReader.ReadLine().Trim(),
                    IP = streamReader.ReadLine().Trim(),
                    Timer = streamReader.ReadLine().Trim(),
                    Modo = streamReader.ReadLine().Trim(),
                    Logger = streamReader.ReadLine().Trim()
                };
                streamReader.Close();
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog("Error en GetConfiguration. Excepción: " + e.Message, LogType.t_error);
                return data;
            }
            return data;
        }

        public static bool SaveConfiguration(Data data)
        {
            try
            {
                //Crea el archivo config.ini
                using (StreamWriter outputFile = new StreamWriter(configFile, false))
                {
                    outputFile.WriteLine(data.RazonSocial.Trim());                      //1°   Razon Social
                    outputFile.WriteLine(data.RutaProyNuevo.Trim());                    //2°   Path
                    outputFile.WriteLine(data.IP.Trim());                               //3°   IP
                    outputFile.WriteLine(data.Timer.Trim());                            //5°   Timer Process
                    outputFile.WriteLine(data.Modo);                                    //6°   Mood
                    outputFile.WriteLine(data.Logger);                                  //7°   Logger
                }
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error al guardar la configuración. Excepción: {e.Message}", LogType.t_error);
                return false;
            }
            return true;
        }

        public static bool ExistConfiguracion()
        {
            return File.Exists(configFile);
        }
    }
    public class Data
    {
        private string razonSocial = "";
        private string rutaProyNuevo = "";
        private string ip = "";
        private string timer = "";
        private MODO modo = MODO.NORMAL;
        private LogType logger = LogType.t_info;

        public Data() { }

        public string RazonSocial
        {
            get => razonSocial;
            set
            {
                if (value != razonSocial)
                {
                    razonSocial = value;
                }
            }
        }

        public string RutaProyNuevo
        {
            get => rutaProyNuevo;
            set
            {
                if (rutaProyNuevo != value)
                {
                    rutaProyNuevo = value;
                }
            }
        }

        public string IP
        {
            get => ip;
            set
            {
                if (ip != value)
                {
                    ip = value;
                }
            }
        }

        public string Timer
        {
            get => timer;
            set
            {
                if (value != timer)
                {
                    timer = value;
                }
            }
        }

        public string Modo
        {
            get => modo.ToString();
            set
            {
                switch (value)
                {
                    case "NORMAL":
                        modo = MODO.NORMAL;
                        break;
                    case "TEST":
                        modo = MODO.TEST;
                        break;
                    default:
                        modo = MODO.NORMAL;
                        break;
                }
            }
        }

        public string Logger
        {
            get => logger.ToString();
            set
            {
                switch (value.ToString())
                {
                    case "t_debug":
                        logger = LogType.t_debug;
                        break;
                    case "t_info":
                        logger = LogType.t_info;
                        break;
                    case "t_error":
                        logger = LogType.t_error;
                        break;
                    default:
                        logger = LogType.t_info;
                        break;
                }
            }
        }
    }

    public enum MODO
    {
        NORMAL,
        TEST
    }

    public interface IGetConfiguration
    {
        Data GetConfiguration();
    }

    public class ConnectorConfiguration : IGetConfiguration
    {
        public ConnectorConfiguration() { }

        public Data GetConfiguration()
        {
            return Configuration.GetConfiguration();
        }
    }
}
