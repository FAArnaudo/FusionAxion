using FusionAxion.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion
{
    public class ConfigurationModel
    {
        private static readonly string configFile = Environment.CurrentDirectory + "/Config.ini";
        public ConfigurationModel() { }

        public static DataModel GetConfiguration()
        {
            DataModel data = null;

            try
            {
                StreamReader streamReader = new StreamReader(configFile);
                data = new DataModel
                {
                    RazonSocial = streamReader.ReadLine().Trim(),
                    RutaProyNuevo = streamReader.ReadLine().Trim(),
                    IP = streamReader.ReadLine().Trim(),
                    Timer = streamReader.ReadLine().Trim(),
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

        public static bool SaveConfiguration(DataModel data)
        {
            try
            {
                //Crea el archivo config.ini
                using (StreamWriter outputFile = new StreamWriter(configFile, false))
                {
                    outputFile.WriteLine(data.RazonSocial.Trim());                      //1°   Razon Social
                    outputFile.WriteLine(data.RutaProyNuevo.Trim());                    //2°   Path
                    outputFile.WriteLine(data.IP.Trim());                               //3°   IP
                    outputFile.WriteLine(data.Timer.Trim());                            //4°   Timer Process
                    outputFile.WriteLine(data.Logger);                                  //5°   Logger
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
}
