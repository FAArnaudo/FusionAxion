using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDS
{
    public class ControllerCem : Controller
    {
        private IProtocolCommand protocolCommand;
        private ConnectorCem ConnectorCem;
        public ControllerCem(string ip, string protocol)
        {
            ProtocolCommand = protocol.Equals("16") ? new Protocol16() : (IProtocolCommand)new Protocol32();

            ConnectorCem = new ConnectorCem(new CemCommunication());
        }

        public IProtocolCommand ProtocolCommand { get => protocolCommand; set => protocolCommand = value; }

        public override bool VerificarConexión()
        {
            return ConnectorCem.PoleoEnLinea(ProtocolCommand.PoleoEnLineaComand);
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

        public void TrtaerCierreAnterior()
        {
            throw new NotImplementedException();
        }

        public void TrtaerTurnoActual()
        {
            throw new NotImplementedException();
        }
    }

    public interface IProtocolCommand
    {
        int Getprotocol();

        byte[] PoleoEnLineaComand { get; }
        byte[] ConfigureStationComand { get; }
        byte[] TanksStockComand { get; }
        byte[] DespachoCommand { get; }
        byte[] CierreDeTurnoComand { get; }
        byte[] CierreAnteriorComand { get; }
        byte[] TurnoActualComand { get; }

    }

    public class Protocol16 : IProtocolCommand
    {
        public byte[] ConfigureStationComand => new byte[] { 0x65 };

        public byte[] TanksStockComand => new byte[] { 0x68 };

        public byte[] DespachoCommand => new byte[] { 0x70 };

        public byte[] CierreDeTurnoComand => new byte[] { 0x07 };

        public byte[] CierreAnteriorComand => new byte[] { 0x0B };

        public byte[] TurnoActualComand => new byte[] { 0x08 };

        public byte[] PoleoEnLineaComand => throw new NotImplementedException();

        public int Getprotocol()
        {
            return 16;
        }
    }

    public class Protocol32 : IProtocolCommand
    {
        public byte[] ConfigureStationComand => new byte[] { 0xB5 };

        public byte[] TanksStockComand => new byte[] { 0xB8 };

        public byte[] DespachoCommand => new byte[] { 0xC0 };

        public byte[] CierreDeTurnoComand => new byte[] { 0x07 };

        public byte[] CierreAnteriorComand => new byte[] { 0x0B };

        public byte[] TurnoActualComand => new byte[] { 0x08 };

        public byte[] PoleoEnLineaComand => throw new NotImplementedException();

        public int Getprotocol()
        {
            return 32;
        }
    }
}
