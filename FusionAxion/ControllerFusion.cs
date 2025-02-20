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
        private ConnectorFusion ConnectorFusion { get; set; }
        private static ControllerFusion instance = null;
        private ControllerFusion()
        {
            ConnectorFusion = new ConnectorFusion();
        }

        public static ControllerFusion Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ControllerFusion();
                }

                return instance;
            }
        }

        public void CloseConnection()
        {
            _ = ConnectorFusion.Fusion.Close();

        }

        public bool CheckConnection()
        {
            bool connection = false;

            if (ConnectorFusion.Fusion.ConnectionStatus())
            {
                connection = true;
            }

            return connection;
        }
    }
}
