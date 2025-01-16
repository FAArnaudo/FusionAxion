using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDS
{
    public class ControllerCem : Controller
    {
        private IProtocolCommand protocolCommand;
        private readonly ConnectorCem ConnectorCem;
        public ControllerCem(string protocol)
        {
            ProtocolCommand = protocol.Equals("16") ? new Protocol16() : (IProtocolCommand)new Protocol32();

            ConnectorCem = new ConnectorCem(new CemCommunication());
        }

        public IProtocolCommand ProtocolCommand { get => protocolCommand; set => protocolCommand = value; }

        public override bool VerificarConexión()
        {
            if (ConnectorCem.PoleoEnLinea(ProtocolCommand.PoleoEnLineaComand))
            {
                _ = ConnectorSQLite.Instance.ExecuteNonQuery($"UPDATE CheckConnection SET isConnected = 1, fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}' WHERE idConnection = 1");
                return true;
            }
            else
            {
                _ = ConnectorSQLite.Instance.ExecuteNonQuery($"UPDATE CheckConnection SET isConnected = 0, fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}' WHERE idConnection = 1");
                return false;
            }
        }

        public override void ConfigurarEstacion()
        {
            Station station = ConnectorCem.ComandoConfiguracionDeLaEstacion(ProtocolCommand.ConfigureStationComand);

            try
            {
                foreach (Surtidor surtidor in station.Surtidores)
                {
                    string campos = "IdSurtidor,Manguera,Producto,Precio,DescProd";

                    foreach (Manguera manguera in surtidor.Mangueras)
                    {
                        string rows = string.Format("{0},{1},{2},{3},'{4}'",
                            surtidor.ID,
                            manguera.ID,
                            manguera.Producto.ID,
                            manguera.Producto.PrecioUnitario,
                            manguera.Producto.Descripcion);

                        DataTable tablaSurtidores = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT * " +
                                                                                                $"FROM Surtidores " +
                                                                                                $"WHERE IdSurtidor = {surtidor.ID} AND Manguera = {manguera.ID}");

                        if (tablaSurtidores.Rows.Count == 0)
                        {
                            _ = ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Surtidores ({0}) VALUES ({1})", campos, rows));
                        }
                        else
                        {
                            ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Surtidores " +
                                                                                   "SET Producto = ('{0}'), Precio = ('{1}'), DescProd = ('{2}') " +
                                                                                   "WHERE IdSurtidor = ({3}) AND Manguera = ('{4}')",
                                                                                   manguera.Producto.ID,
                                                                                   manguera.Producto.PrecioUnitario,
                                                                                   manguera.Producto.Descripcion,
                                                                                   surtidor.ID,
                                                                                   manguera.ID));
                        }

                        Log.Instance.WriteLog(string.Format("SURTIDOR: ({0}) MANGUERA: ({1}) PRODUCTO: ({2})",
                                                            surtidor.ID, manguera.ID, manguera.Producto.Descripcion), LogType.t_info);
                    }
                }

                foreach (Producto producto in station.Productos)
                {
                    string campos = "id_producto,producto,precio";

                    string rows = string.Format("{0},'{1}',{2}",
                                                 producto.ID,
                                                 producto.Descripcion,
                                                 producto.PrecioUnitario);

                    DataTable tablaProductos = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT * " +
                                                                                           "FROM Productos " +
                                                                                          $"WHERE id_producto = {producto.ID}");

                    if (tablaProductos.Rows.Count == 0)
                    {
                        ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Productos ({0}) VALUES ({1})", campos, rows));
                    }
                    else
                    {
                        ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Productos " +
                                                                               "SET producto = ('{0}'), precio = ({1}) " +
                                                                               "WHERE id_producto = ({2})",
                                                                               producto.Descripcion,
                                                                               producto.PrecioUnitario,
                                                                               producto.ID));
                    }
                }

                foreach (Tanque tanque in station.Tanques)
                {
                    string campos = "id_tanque,volumen_actual,capacidad_maxima";

                    string rows = string.Format("{0},'{1}',{2}",
                                                 tanque.ID,
                                                 tanque.VolumenDeProducto,
                                                 tanque.CapacidadMaxima);

                    DataTable tablaTanques = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT * " +
                                                                                           "FROM Tanques " +
                                                                                          $"WHERE id_tanque = {tanque.ID}");

                    if (tablaTanques.Rows.Count == 0)
                    {
                        ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Tanques ({0}) VALUES ({1})", campos, rows));
                    }
                    else
                    {
                        ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Tanques " +
                                                                               "SET volumen_actual = ('{0}'), capacidad_maxima = ({1}) " +
                                                                               "WHERE id_tanque = ({2})",
                                                                                tanque.VolumenDeProducto,
                                                                                tanque.CapacidadMaxima,
                                                                                tanque.ID));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error en el metodo ConfigurarEstacion.\n\tExcepcion: {e.Message}");
            }
        }

        public override void ActualizarProductos()
        {
            throw new NotImplementedException();
        }

        public override void ActualizarTanques()
        {
            List<Tanque> tanques = ConnectorCem.ComandoStockDeTanques(ProtocolCommand.TanksStockComand);

            try
            {
                foreach (Tanque tanque in tanques)
                {
                    string campos = "id_tanque,volumen_actual,capacidad_maxima";

                    string rows = string.Format("{0},'{1}',{2}",
                                                 tanque.ID,
                                                 tanque.VolumenDeProducto,
                                                 tanque.CapacidadMaxima);

                    DataTable tablaTanques = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT * " +
                                                                                           "FROM Tanques " +
                                                                                          $"WHERE id_tanque = {tanque.ID}");

                    if (tablaTanques.Rows.Count == 0)
                    {
                        ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Tanques ({0}) VALUES ({1})", campos, rows));
                    }
                    else
                    {
                        ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Tanques " +
                                                                               "SET volumen_actual = ('{0}'), capacidad_maxima = ({1}) " +
                                                                               "WHERE id_tanque = ({2})",
                                                                                tanque.VolumenDeProducto,
                                                                                tanque.CapacidadMaxima,
                                                                                tanque.ID));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error en el metodo ActualizarTanques.\n\tExcepcion: {e.Message}", LogType.t_error);
            }
        }

        public override void GrabarDespachos()
        {
            foreach (Surtidor surtidor in Station.Instance.Surtidores)
            {
                byte[] command = ProtocolCommand.DespachoCommand;

                DespachoCem despacho;

                if (surtidor.ID != ProtocolCommand.GetProtocol())
                {
                    command[0] = (byte)(command[0] + Convert.ToByte(surtidor.ID));
                }

                despacho = ConnectorCem.ComandoInformacionDeDespacho(command);

                if (despacho == null)
                {
                    continue;
                }

                try
                {
                    DataTable tablaDespachos = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT * " +
                                                                                           $"FROM Despachos " +
                                                                                           $"WHERE id = {despacho.IdDespacho} AND surtidor = {surtidor.ID}");

                    if (tablaDespachos.Rows.Count == 0)
                    {
                        bool YPFRutaContado = false;
                        despacho.IdSurtidor = surtidor.ID;

                        UpdateProductos(despacho);

                        foreach (Manguera manguera in surtidor.Mangueras)
                        {
                            if (manguera.Producto.Descripcion.Equals(despacho.Producto))
                            {
                                despacho.IdManguera = manguera.ID;
                                break;
                            }
                        }

                        if (despacho.VentaFacturada)
                        {
                            DataTable tablaProductos = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT * " +
                                                                                                   $"FROM Productos " +
                                                                                                   $"WHERE id_producto = {despacho.IdProducto} OR numero_despacho = {despacho.IdProducto}");

                            double precioUnitario = Convert.ToDouble(tablaProductos.Rows[0]["precio"]);

                            if (despacho.PPU < precioUnitario)
                            {
                                YPFRutaContado = true;
                            }

                            if (despacho.Producto == null)
                            {
                                despacho.Producto = Convert.ToString(tablaProductos.Rows[0]["producto"]);
                            }
                        }

                        string campos = "id,surtidor,manguera,producto,PPU,volumen,monto,descripcion,facturado,YPFruta,despacho_pedido,fecha";
                        string row = string.Format("{0},{1},{2},{3},{4},{5},{6},'{7}',{8},{9},{10},'{11}'",
                                despacho.IdDespacho,
                                despacho.IdSurtidor,
                                despacho.IdManguera,
                                despacho.IdProducto,
                                despacho.PPU.ToString(CultureInfo.InvariantCulture),
                                despacho.Volumen.ToString(CultureInfo.InvariantCulture),
                                despacho.Monto.ToString(CultureInfo.InvariantCulture),
                                despacho.Producto,
                                despacho.VentaFacturada,
                                YPFRutaContado,
                                0,
                                DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));

                        ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Despachos ({0}) VALUES ({1})", campos, row));

                        Log.Instance.WriteLog(string.Format("INSERT INTO Despachos ({0}) VALUES ({1})", campos, row), LogType.t_debug);
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($"Error en el metodo GrabarDespachos.\n\tExcepcion: {e.Message}", LogType.t_error);
                }
            }
        }

        public override void GrabarCierre()
        {
            try
            {

            }
            catch (Exception e)
            {
                throw new Exception($"Excepción: {e.Message}");
            }
        }

        public void TrtaerCierreAnterior()
        {
            throw new NotImplementedException();
        }

        public void TrtaerTurnoActual()
        {
            throw new NotImplementedException();
        }

        private void UpdateProductos(DespachoCem despacho)
        {
            foreach (ProductoCem producto in Station.Instance.Productos)
            {
                if (producto.PrecioUnitario == despacho.PPU)
                {
                    ConnectorSQLite.Instance.ExecuteNonQuery($"UPDATE Productos " +
                                                             $"SET numero_despacho = {despacho.IdProducto} " +
                                                             $"WHERE id_producto = {producto.ID}");

                    producto.IdProductoDespacho = despacho.IdProducto;

                    despacho.IdProducto = producto.ID;
                    despacho.Producto = producto.Descripcion;
                    break;
                }
            }
        }
    }

    public interface IProtocolCommand
    {
        int GetProtocol();

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

        public byte[] PoleoEnLineaComand => new byte[] { 0x00 };

        public int GetProtocol()
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

        public byte[] PoleoEnLineaComand => new byte[] { 0x00 };

        public int GetProtocol()
        {
            return 32;
        }
    }
}
