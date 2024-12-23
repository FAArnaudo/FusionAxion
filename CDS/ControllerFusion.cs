using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDS
{
    public class ControllerFusion : Controller
    {
        private string ip;
        public ControllerFusion(string ip) : base(ip)
        {
            IP = ip;
        }

        public string IP
        {
            get => ip;
            set
            {
                if (ip == null || !ip.Equals(value))
                {
                    ip = value;
                }
            }
        }

        public override void ActualizarProductos()
        {
            throw new NotImplementedException();
        }

        public override void ActualizarTanques()
        {
            throw new NotImplementedException();
        }

        public override void ConfigurarEstacion()
        {
            throw new NotImplementedException();
        }

        public override void GrabarCierre()
        {
            throw new NotImplementedException();
        }

        public override void GrabarDespachos()
        {
            throw new NotImplementedException();
        }
    }
}
