using FusionClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion
{
    public class ControllerFusion
    {
        private Fusion cFusion;
        public ControllerFusion()
        {

        }

        public bool CheckConnection()
        {
            bool connection = false;
            int tries = 0;

            try
            {
                while (tries <= 5)
                {
                    tries++;

                    // Crear el pipeClient si está cerrado
                    if (cFusion == null)
                    {
                        cFusion = new Fusion();
                    }

                    cFusion.Connection("IP");

                    if (cFusion.ConnectionStatus())
                    {
                        // Mensaje de Conexion Exitosa
                        connection = true;
                        break;
                    }
                    else
                    {
                        // Mensaje de Conexion Fallida intento N° x
                        cFusion = null;
                    }
                }
            }
            catch (Exception)
            {
                cFusion = null;
                // Mensaje de Conexion Fallida
            }

            return connection;
        }
    }
}
