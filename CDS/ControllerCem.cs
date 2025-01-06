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
        private ConnectorCem ConnectorCem;
        public ControllerCem(string ip, string protocolo)
        {
            IP = id;
            Protocolo = protocolo;
            ConnectorCem = new ConnectorCem(new CemCommunication());
        }

        public string IP
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

        public override bool VerificarConexión()
        {
            if (ConnectorCem.PoleoEnLinea(new byte[] { 0x00 }))
            {
                return true;
            }

            return false;
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
