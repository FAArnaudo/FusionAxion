using Polly;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDS
{
    public class ConnectorCem
    {
        private readonly byte separador = 0x7E;
        private readonly string pipeName = "CEM44POSPIPE";
        private string ipController;
        private string protocol;

        public string IpController { get => ipController; set => ipController = value; }
        public string Protocol { get => protocol; set => protocol = value; }

        public ConnectorCem() { }

        public bool PoleoEnLinea()
        {
            return false;
        }

        public Station ComandoConfiguracionDeLaEstacion()
        {
            return null;
        }

        public Tank ComandoStockDeTanques()
        {
            return null;
        }

        public Despacho ComandoInformacionDeDespacho()
        {
            return null;
        }

        public CierreDeTurno ComandoCierresDeTurno()
        {
            return null;
        }

        private byte[] EnviarComando(byte[] mensaje)
        {
            throw new NotImplementedException();
        }
    }

}
