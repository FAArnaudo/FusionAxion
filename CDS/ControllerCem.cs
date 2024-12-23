using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDS
{
    public class ControllerCem : Controller
    {
        private string id;
        private string protocolo;
        public ControllerCem(string ip, string protocolo) : base(ip)
        {
            ID = id;
            Protocolo = protocolo;
        }

        public string ID
        {
            get => id;
            set
            {
                if (id == null || !id.Equals(value))
                {
                    id = value;
                }
            }
        }

        public string Protocolo
        {
            get => protocolo;
            set
            {
                if (protocolo == null || !protocolo.Equals(value))
                {
                    protocolo = value;
                }
            }
        }

        public override void ConfigurarEstacion()
        {
            throw new NotImplementedException();
        }

        public override void ActualizarProductos()
        {
            throw new NotImplementedException();
        }

        public override void ActualizarTanques()
        {
            throw new NotImplementedException();
        }

        public override void GrabarDespachos()
        {
            throw new NotImplementedException();
        }

        public override void GrabarCierre()
        {
            throw new NotImplementedException();
        }
    }
}
