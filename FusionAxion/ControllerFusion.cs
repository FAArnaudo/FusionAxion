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

        public bool CheckConnection(string IP)
        {
            bool isConnected = false;

            ConnectorFusion.Fusion.Connection(IP);

            if (ConnectorFusion.Fusion.ConnectionStatus())
            {
                isConnected = true;
            }
            else
            {
                _ = Disconect();
            }

            return isConnected;
        }

        public bool Disconect()
        {
            bool isClose = false;
            if (ConnectorFusion.Fusion.Close())
            {
                isClose = true;
            }

            ConnectorFusion.Fusion = null;

            return isClose;
        }
    }
}
