using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
            if (ConnectorCem.PoleoEnLinea(ProtocolCommand.PoleoEnLineaCommand))
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
            Station station = ConnectorCem.ComandoConfiguracionDeLaEstacion(ProtocolCommand.ConfigureStationCommand);

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
                Log.Instance.WriteLog("\n", LogType.t_info);

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

                    Log.Instance.WriteLog(string.Format("PRODUCTO: ({0}) DESCRIPCION: ({1}) PRECIO: ({2})",
                                                            producto.ID, producto.Descripcion, producto.PrecioUnitario), LogType.t_info);
                }
                Log.Instance.WriteLog("\n", LogType.t_info);

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
                    Log.Instance.WriteLog(string.Format("TANQUE: ({0}) CAPACIDAD: ({1}))",
                                                            tanque.ID, tanque.CapacidadMaxima), LogType.t_info);
                }
                Log.Instance.WriteLog("\n", LogType.t_info);
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
            List<Tanque> tanques = ConnectorCem.ComandoStockDeTanques(ProtocolCommand.TanksStockCommand);

            try
            {
                foreach (Tanque tanque in tanques)
                {
                    string campos = "id_tanque,volumen_actual,capacidad_maxima";

                    string rows = string.Format("{0},{1},{2}",
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
            ConnectorSQLite.Instance.ExecuteNonQuery("UPDATE cierreBandera SET actualizar_tanques = 0");
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

                        Thread.Sleep(1000);
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
            CierreDeTurnoCem cierreDeTurno = ConnectorCem.ComandoCierresDeTurno(ProtocolCommand.CierreDeTurnoCommand);

            try
            {
                ConnectorSQLite.Instance.ExecuteNonQuery("UPDATE cierreBandera SET hacerCierre = 0");

                string fields = "fecha,monto_contado,volumen_contado,monto_YPFruta,volumen_YPFruta,state";
                string values = string.Format("'{0}',{1},{2},{3},{4},'{5}'",
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                    cierreDeTurno.TotalesMedioDePago[0].TotalMonto,
                    cierreDeTurno.TotalesMedioDePago[0].TotalVolumen,
                    cierreDeTurno.TotalesMedioDePago[3].TotalMonto,
                    cierreDeTurno.TotalesMedioDePago[3].TotalVolumen,
                    cierreDeTurno.Estado);

                ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Cierres ({0}) VALUES ({1})", fields, values));

                // Traer ID del cierre para poder referenciar los detalles
                DataTable tablaCierres = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT max(id) FROM Cierres");

                cierreDeTurno.ID = Convert.ToInt32(tablaCierres.Rows[0][0]);

                ConnectorSQLite.Instance.ExecuteNonQuery($"UPDATE Cierres SET id_cierre = ({cierreDeTurno.ID}) WHERE id = {cierreDeTurno.ID}");

                // Grabar CierresPorProducto
                fields = "id,producto,monto,volumen";
                for (int periodo = 0; periodo < cierreDeTurno.TotalesPorPeriodoPorNivelPorProducto.Count; periodo++)
                {
                    for (int nivel = 0; nivel < cierreDeTurno.TotalesPorPeriodoPorNivelPorProducto[periodo].Count; nivel++)
                    {
                        for (int producto = 0; producto < cierreDeTurno.TotalesPorPeriodoPorNivelPorProducto[periodo][nivel].Count; producto++)
                        {
                            values = string.Format("{0},{1},{2},{3}",
                                cierreDeTurno.ID,
                                cierreDeTurno.TotalesPorPeriodoPorNivelPorProducto[periodo][nivel][producto].NumeroDeProducto,
                                cierreDeTurno.TotalesPorPeriodoPorNivelPorProducto[periodo][nivel][producto].TotalMonto,
                                cierreDeTurno.TotalesPorPeriodoPorNivelPorProducto[periodo][nivel][producto].TotalVolumen);

                            ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO CierresPorProducto ({0}) VALUES ({1})", fields, values));
                        }
                    }
                }

                // Grabar CierresPorManguera
                fields = "id,surtidor,manguera,monto,volumen";

                for (int manguera = 0; manguera < cierreDeTurno.TotalesPorManguera.Count; manguera++)
                {
                    values = string.Format("{0},{1},{2},{3},{4}",
                        cierreDeTurno.ID,
                        cierreDeTurno.TotalesPorManguera[manguera].NumeroDeSurtidor,
                        cierreDeTurno.TotalesPorManguera[manguera].NumeroDeManguera,
                        cierreDeTurno.TotalesPorManguera[manguera].TotalVntasMonto,
                        cierreDeTurno.TotalesPorManguera[manguera].TotalVntasVolumen);

                    ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO CierresPorManguera ({0}) VALUES ({1})", fields, values));
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Excepción: {e.Message}");
            }

            CheckTableSize();
        }

        public void TrtaerCierreAnterior()
        {
            Log.Instance.WriteLog("Iniciando: Traer la Informacion del ultimo cierre de turno cortado.\n", LogType.t_info);

            CierreDeTurnoCem turnoAnterior = ConnectorCem.ComandoCierresDeTurno(ProtocolCommand.CierreAnteriorCommand);
            turnoAnterior.Estado = "Turno Rectificado - OK";

            try
            {
                ConnectorSQLite.Instance.ExecuteNonQuery("UPDATE cierreBandera SET cierre_anterior = 0");

                // Traer ID del cierre para poder referenciar los detalles
                DataTable tablaCierres = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT max(id) FROM Cierres");

                turnoAnterior.ID = Convert.ToInt32(tablaCierres.Rows[0][0]);

                int modified = ConnectorSQLite.Instance.ExecuteNonQuery("UPDATE Cierres " +
                    $"SET fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}', " +
                        $"monto_contado = {turnoAnterior.TotalesMedioDePago[0].TotalMonto}, " +
                        $"volumen_contado = {turnoAnterior.TotalesMedioDePago[0].TotalVolumen}, " +
                        $"monto_YPFruta = {turnoAnterior.TotalesMedioDePago[3].TotalMonto}, " +
                        $"volumen_YPFruta = {turnoAnterior.TotalesMedioDePago[3].TotalVolumen}, " +
                        $"state = '{turnoAnterior.Estado}' " +
                    $"WHERE id = {turnoAnterior.ID}");

                if (modified != 1)
                {
                    string fields = "fecha,monto_contado,volumen_contado,monto_YPFruta,volumen_YPFruta,state";
                    
                    string values = string.Format("'{0}',{1},{2},{3},{4},'{5}'",
                        DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                        turnoAnterior.TotalesMedioDePago[0].TotalMonto,
                        turnoAnterior.TotalesMedioDePago[0].TotalVolumen,
                        turnoAnterior.TotalesMedioDePago[3].TotalMonto,
                        turnoAnterior.TotalesMedioDePago[3].TotalVolumen,
                        turnoAnterior.Estado);

                    ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Cierres ({0}) VALUES ({1})", fields, values));
                }

                // Grabar CierresPorProducto
                for (int periodo = 0; periodo < turnoAnterior.TotalesPorPeriodoPorNivelPorProducto.Count; periodo++)
                {
                    for (int nivel = 0; nivel < turnoAnterior.TotalesPorPeriodoPorNivelPorProducto[periodo].Count; nivel++)
                    {
                        for (int producto = 0; producto < turnoAnterior.TotalesPorPeriodoPorNivelPorProducto[periodo][nivel].Count; producto++)
                        {
                            modified = ConnectorSQLite.Instance.ExecuteNonQuery("UPDATE CierresPorProducto " +
                                                                    $"SET monto = {turnoAnterior.TotalesPorPeriodoPorNivelPorProducto[periodo][nivel][producto].TotalMonto}, " +
                                                                        $"volumen = {turnoAnterior.TotalesPorPeriodoPorNivelPorProducto[periodo][nivel][producto].TotalVolumen} " +
                                                                    $"WHERE id = {turnoAnterior.ID} AND producto = {turnoAnterior.TotalesPorPeriodoPorNivelPorProducto[periodo][nivel][producto].NumeroDeProducto}");

                            if (modified != 1)
                            {
                                string fields = "id,producto,monto,volumen";

                                string values = string.Format("{0},{1},{2},{3}",
                                turnoAnterior.ID,
                                turnoAnterior.TotalesPorPeriodoPorNivelPorProducto[periodo][nivel][producto].NumeroDeProducto,
                                turnoAnterior.TotalesPorPeriodoPorNivelPorProducto[periodo][nivel][producto].TotalMonto,
                                turnoAnterior.TotalesPorPeriodoPorNivelPorProducto[periodo][nivel][producto].TotalVolumen);

                                ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO CierresPorProducto ({0}) VALUES ({1})", fields, values));
                            }
                        }
                    }
                }

                // Actualizar CierresPorManguera
                for (int manguera = 0; manguera < turnoAnterior.TotalesPorManguera.Count; manguera++)
                {
                    modified = ConnectorSQLite.Instance.ExecuteNonQuery("UPDATE CierresPorManguera " +
                    $"SET monto = {turnoAnterior.TotalesPorManguera[manguera].TotalVntasMonto}, " +
                        $"volumen = {turnoAnterior.TotalesPorManguera[manguera].TotalVntasVolumen} " +
                    $"WHERE id = {turnoAnterior.ID} AND surtidor = {turnoAnterior.TotalesPorManguera[manguera].NumeroDeSurtidor} AND manguera = {turnoAnterior.TotalesPorManguera[manguera].NumeroDeManguera}");

                    if (modified != 1)
                    {
                        string fields = "id,surtidor,manguera,monto,volumen";
                        
                        string values = string.Format("{0},{1},{2},{3},{4}",
                        turnoAnterior.ID,
                        turnoAnterior.TotalesPorManguera[manguera].NumeroDeSurtidor,
                        turnoAnterior.TotalesPorManguera[manguera].NumeroDeManguera,
                        turnoAnterior.TotalesPorManguera[manguera].TotalVntasMonto,
                        turnoAnterior.TotalesPorManguera[manguera].TotalVntasVolumen);

                        ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO CierresPorManguera ({0}) VALUES ({1})", fields, values));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error en el metodo TrtaerCierreAnterior. Excepción: {e.Message}");
            }
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

        private void CheckTableSize()
        {
            ConnectorSQLite.Instance.ExecuteNonQuery("DELETE FROM despachos");

            int sizeTable = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT * FROM Cierres").Rows.Count;
            int maxLimit = 60;
            int limit = maxLimit / 2;

            if (sizeTable >= maxLimit)
            {
                string deleteQuery = $"DELETE FROM Cierres WHERE id IN (SELECT id FROM Cierres ORDER BY id ASC LIMIT {limit})";
                ConnectorSQLite.Instance.ExecuteNonQuery(deleteQuery);

                deleteQuery = $"DELETE FROM CierresPorManguera " +
                              $"WHERE id " +
                              $"IN (SELECT id " +
                                  $"FROM CierresPorManguera " +
                                  $"ORDER BY id ASC " +
                                  $"LIMIT {limit * Station.Instance.NumeroDeSurtidores * Station.Instance.NumeroDeProductos})";
                ConnectorSQLite.Instance.ExecuteNonQuery(deleteQuery);

                deleteQuery = $"DELETE FROM CierresPorProducto " +
                              $"WHERE id IN (SELECT id FROM CierresPorProducto ORDER BY id ASC LIMIT {limit * Station.Instance.NumeroDeProductos})";
                ConnectorSQLite.Instance.ExecuteNonQuery(deleteQuery);
            }
        }
    }

    public interface IProtocolCommand
    {
        int GetProtocol();

        byte[] PoleoEnLineaCommand { get; }
        byte[] ConfigureStationCommand { get; }
        byte[] TanksStockCommand { get; }
        byte[] DespachoCommand { get; }
        byte[] CierreDeTurnoCommand { get; }
        byte[] CierreAnteriorCommand { get; }
        byte[] TurnoActualCommand { get; }
    }

    public class Protocol16 : IProtocolCommand
    {
        public byte[] ConfigureStationCommand => new byte[] { 0x65 };

        public byte[] TanksStockCommand => new byte[] { 0x68 };

        public byte[] DespachoCommand => new byte[] { 0x70 };

        public byte[] CierreDeTurnoCommand => new byte[] { 0x07 };

        public byte[] CierreAnteriorCommand => new byte[] { 0x0B };

        public byte[] TurnoActualCommand => new byte[] { 0x08 };

        public byte[] PoleoEnLineaCommand => new byte[] { 0x00 };

        public int GetProtocol()
        {
            return 16;
        }
    }

    public class Protocol32 : IProtocolCommand
    {
        public byte[] ConfigureStationCommand => new byte[] { 0xB5 };

        public byte[] TanksStockCommand => new byte[] { 0xB8 };

        public byte[] DespachoCommand => new byte[] { 0xC0 };

        public byte[] CierreDeTurnoCommand => new byte[] { 0x07 };

        public byte[] CierreAnteriorCommand => new byte[] { 0x0B };

        public byte[] TurnoActualCommand => new byte[] { 0x08 };

        public byte[] PoleoEnLineaCommand => new byte[] { 0x00 };

        public int GetProtocol()
        {
            return 32;
        }
    }
}
