using FusionAxion.Repositories;
using FusionClass;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion
{
    public class ControllerFusion
    {
        //private Fusion fusion = null;
        private readonly object fusionLock = new object();
        private static ControllerFusion instance = null;
        private Fusion Fusion { get; set; }
        private ConnectorFusion ConnectorFusion { get; set; } = null;
        private ControllerFusion()
        {
            ConnectorFusion = new ConnectorFusion();
            Fusion = new Fusion();
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

        public void Connect(string IP)
        {
            lock (fusionLock)
            {
                Fusion.Connection(IP);
            }
        }

        public bool Disconect()
        {
            bool isClose = false;

            lock (fusionLock)
            {
                isClose = Fusion.Close();
            }

            return isClose;
        }

        public bool CheckConnection()
        {
            bool isConnected = false;

            lock (fusionLock)
            {
                isConnected = Fusion.ConnectionStatus();
            }

            return isConnected;
        }

        public void ConfigurarEstacion()
        {
            lock (fusionLock)
            {
                ConnectorFusion.ComandoConfiguracionDeLaEstacion(Fusion);
            }

            Station station = Station.Instance;

            try
            {
                foreach (Surtidor surtidor in station.Surtidores)
                {
                    string campos = "IdSurtidor,Manguera,Producto,Precio,DescProd";

                    foreach (Manguera manguera in surtidor.Mangueras)
                    {
                        string precioUnitario = manguera.Producto.PrecioUnitario.ToString("F2", CultureInfo.InvariantCulture);
                        string rows = string.Format("{0},{1},{2},{3},'{4}'",
                            surtidor.ID,
                            manguera.ID,
                            manguera.Producto.ID,
                            precioUnitario,
                            manguera.Producto.Descripcion);

                        DataTable tablaSurtidores = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT * " +
                                                                                                $"FROM Surtidores " +
                                                                                                $"WHERE IdSurtidor = {surtidor.ID} AND Manguera = {manguera.ID}");

                        _ = tablaSurtidores.Rows.Count == 0
                            ? ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Surtidores ({0}) VALUES ({1})", campos, rows))
                            : ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Surtidores " +
                                                                                   "SET Producto = ('{0}'), Precio = ('{1}'), DescProd = ('{2}') " +
                                                                                   "WHERE IdSurtidor = ({3}) AND Manguera = ('{4}')",
                                                                                    manguera.Producto.ID,
                                                                                    precioUnitario,
                                                                                    manguera.Producto.Descripcion,
                                                                                    surtidor.ID,
                                                                                    manguera.ID));

                        Log.Instance.WriteLog(string.Format("SURTIDOR: ({0}) MANGUERA: ({1}) PRODUCTO: ({2})",
                                                            surtidor.ID, manguera.ID, manguera.Producto.Descripcion), LogType.t_info);
                    }
                }
                Log.Instance.WriteLog("\n", LogType.t_info);

                foreach (Producto producto in station.Productos)
                {
                    string campos = "id_producto,id_siges,producto,precio";
                    string precioUnitario = producto.PrecioUnitario.ToString("F2", CultureInfo.InvariantCulture);

                    string rows = string.Format("{0},{1},'{2}',{3}",
                                                 producto.ID,
                                                 producto.ID_SIGES,
                                                 producto.Descripcion,
                                                 precioUnitario);

                    DataTable tablaProductos = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT * " +
                                                                                           "FROM Productos " +
                                                                                          $"WHERE id_producto = {producto.ID}");

                    _ = tablaProductos.Rows.Count == 0
                        ? ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Productos ({0}) VALUES ({1})", campos, rows))
                        : ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Productos " +
                                                                                 "SET producto = ('{0}'), precio = ({1}) " +
                                                                                 "WHERE id_producto = ({2})",
                                                                                  producto.Descripcion,
                                                                                  precioUnitario,
                                                                                  producto.ID));

                    Log.Instance.WriteLog(string.Format("PRODUCTO: ({0}) DESCRIPCION: ({1}) PRECIO: ({2})",
                                                            producto.ID, producto.Descripcion, precioUnitario), LogType.t_info);
                }
                Log.Instance.WriteLog("\n", LogType.t_info);

                foreach (Tanque tanque in station.Tanques)
                {
                    string campos = "id_tanque,volumen_actual,capacidad_maxima";
                    string volumenDeProducto = tanque.VolumenDeProducto.ToString("F2", CultureInfo.InvariantCulture);
                    string capacidadMaxima = tanque.CapacidadMaxima.ToString("F2", CultureInfo.InvariantCulture);

                    string rows = string.Format("{0},'{1}',{2}",
                                                 tanque.ID,
                                                 volumenDeProducto,
                                                 capacidadMaxima);

                    DataTable tablaTanques = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT * " +
                                                                                         "FROM Tanques " +
                                                                                        $"WHERE id_tanque = {tanque.ID}");

                    _ = tablaTanques.Rows.Count == 0
                        ? ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Tanques ({0}) VALUES ({1})", campos, rows))
                        : ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Tanques " +
                                                                                 "SET volumen_actual = ('{0}'), capacidad_maxima = ({1}) " +
                                                                                 "WHERE id_tanque = ({2})",
                                                                                  volumenDeProducto,
                                                                                  capacidadMaxima,
                                                                                  tanque.ID));

                    Log.Instance.WriteLog(string.Format("TANQUE: ({0}))", tanque.ID), LogType.t_info);
                }
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error en el metodo ConfigurarEstacion.\n\tExcepcion: {e.Message}.\n", LogType.t_error);
                throw new Exception($"Error en el metodo ConfigurarEstacion.\n\tExcepcion: {e.Message}");
            }
        }

        public void ActualizarTanques()
        {
            List<Tanque> tanques = Station.Instance.Tanques;

            foreach (Tanque tanque in tanques)
            {
                try
                {
                    if (tanque.ID > 0)
                    {
                        FusionTankInfo fusionTank = new FusionTankInfo();
                        lock (fusionLock)
                        {
                            _ = Fusion.GetTankInfo(tanque.ID, fusionTank);
                        }

                        tanque.VolumenDeProducto = Convert.ToDouble(fusionTank.GetFuelVolume());
                        tanque.VolumenDeAgua = Convert.ToDouble(fusionTank.GetWaterVolume());
                        tanque.CapacidadMaxima = Convert.ToDouble(fusionTank.TankVolumeCapacity());
                        tanque.VolumenVacio = tanque.CapacidadMaxima - (tanque.VolumenDeProducto + tanque.VolumenDeAgua);

                        Log.Instance.WriteLog($"Tanque {tanque.ID}, Volumen Total {tanque.CapacidadMaxima}, Volumen de Producto {tanque.VolumenDeProducto}", LogType.t_info);

                        string campos = "id_tanque,volumen_actual,capacidad_maxima,actualizado";

                        string rows = string.Format("{0},{1},{2},{3}",
                                                     tanque.ID,
                                                     tanque.VolumenDeProducto.ToString(CultureInfo.InvariantCulture),
                                                     tanque.CapacidadMaxima.ToString().ToString(CultureInfo.InvariantCulture),
                                                     DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));

                        DataTable tablaTanques = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT * " +
                                                                                             "FROM Tanques " +
                                                                                            $"WHERE id_tanque = {tanque.ID}");

                        _ = tablaTanques.Rows.Count == 0 ?
                            ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Tanques ({0}) VALUES ({1})", campos, rows)) :
                            ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Tanques SET volumen_actual = ({0}), capacidad_maxima = ({1}), actualizado = ('{2}') WHERE id_tanque = ({3})",
                                                                                    tanque.VolumenDeProducto.ToString(CultureInfo.InvariantCulture),
                                                                                    tanque.CapacidadMaxima.ToString(CultureInfo.InvariantCulture),
                                                                                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                                                                                    tanque.ID));
                    }
                    Log.Instance.WriteLog($"\n", LogType.t_info);
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($"Error al actualizar el tanque(id) : {tanque.ID}. Excepción: {e.Message}", LogType.t_error);
                }
            }
        }
    }
}
