using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion.Model
{
    public class DataModel
    {
        private string razonSocial = "";
        private string rutaProyNuevo = "";
        private string ip = "192.168.0.0";
        private string timer = "";
        private LogType logger = LogType.t_info;

        public DataModel() { }

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
}
