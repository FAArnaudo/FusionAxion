using FusionClass;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public void Connect(string IP)
        {
            ConnectorFusion.Fusion.Connection(IP);
        }

        public bool Disconect()
        {
            bool isClose = false;

            if (ConnectorFusion.Fusion.Close())
            {
                isClose = true;

                ConnectorFusion.ResetFusionObject();
            }

            return isClose;
        }

        public bool CheckConnection()
        {
            bool isConnected = false;

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

        public void ConfigurarEstacion()
        {
            ConnectorFusion.ComandoConfiguracionDeLaEstacion();

            Station station = Station.Instance;

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
                            manguera.Producto.PrecioUnitario.ToString(CultureInfo.InvariantCulture),
                            manguera.Producto.Descripcion);

                        //DataTable tablaSurtidores = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT * " +
                        //                                                                        $"FROM Surtidores " +
                        //                                                                        $"WHERE IdSurtidor = {surtidor.ID} AND Manguera = {manguera.ID}");

                        //_ = tablaSurtidores.Rows.Count == 0
                        //    ? ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Surtidores ({0}) VALUES ({1})", campos, rows))
                        //    : ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Surtidores " +
                        //                                                           "SET Producto = ('{0}'), Precio = ('{1}'), DescProd = ('{2}') " +
                        //                                                           "WHERE IdSurtidor = ({3}) AND Manguera = ('{4}')",
                        //                                                            manguera.Producto.ID,
                        //                                                            manguera.Producto.PrecioUnitario.ToString(CultureInfo.InvariantCulture),
                        //                                                            manguera.Producto.Descripcion,
                        //                                                            surtidor.ID,
                        //                                                            manguera.ID));
                        Log.Instance.WriteLog(string.Format("SURTIDOR: ({0}) MANGUERA: ({1}) PRODUCTO: ({2})",
                                                            surtidor.ID, manguera.ID, manguera.Producto.Descripcion), LogType.t_info);
                    }
                }
                Log.Instance.WriteLog("\n", LogType.t_info);

                foreach (Producto producto in station.Productos)
                {
                    string campos = "id_producto,id_siges,producto,precio";

                    string rows = string.Format("{0},{1},'{2}',{3}",
                                                 producto.ID,
                                                 producto.ID_SIGES,
                                                 producto.Descripcion,
                                                 producto.PrecioUnitario.ToString(CultureInfo.InvariantCulture));

                    //DataTable tablaProductos = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT * " +
                    //                                                                       "FROM Productos " +
                    //                                                                      $"WHERE id_producto = {producto.ID}");

                    //_ = tablaProductos.Rows.Count == 0
                    //    ? ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Productos ({0}) VALUES ({1})", campos, rows))
                    //    : ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Productos " +
                    //                                                             "SET producto = ('{0}'), precio = ({1}) " +
                    //                                                             "WHERE id_producto = ({2})",
                    //                                                              producto.Descripcion,
                    //                                                              producto.PrecioUnitario.ToString(CultureInfo.InvariantCulture),
                    //                                                              producto.ID));

                    Log.Instance.WriteLog(string.Format("PRODUCTO: ({0}) DESCRIPCION: ({1}) PRECIO: ({2})",
                                                            producto.ID, producto.Descripcion, producto.PrecioUnitario), LogType.t_info);
                }
                Log.Instance.WriteLog("\n", LogType.t_info);

                foreach (Tanque tanque in station.Tanques)
                {
                    string campos = "id_tanque,volumen_actual,capacidad_maxima";

                    string rows = string.Format("{0},'{1}',{2}",
                                                 tanque.ID,
                                                 tanque.VolumenDeProducto.ToString(CultureInfo.InvariantCulture),
                                                 tanque.CapacidadMaxima.ToString(CultureInfo.InvariantCulture));

                    //DataTable tablaTanques = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT * " +
                    //                                                                     "FROM Tanques " +
                    //                                                                    $"WHERE id_tanque = {tanque.ID}");

                    //_ = tablaTanques.Rows.Count == 0
                    //    ? ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Tanques ({0}) VALUES ({1})", campos, rows))
                    //    : ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Tanques " +
                    //                                                             "SET volumen_actual = ('{0}'), capacidad_maxima = ({1}) " +
                    //                                                             "WHERE id_tanque = ({2})",
                    //                                                              tanque.VolumenDeProducto,
                    //                                                              tanque.CapacidadMaxima,
                    //                                                              tanque.ID));

                    Log.Instance.WriteLog(string.Format("TANQUE: ({0}))", tanque.ID), LogType.t_info);
                }
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error en el metodo ConfigurarEstacion.\n\tExcepcion: {e.Message}.\n", LogType.t_error);
                throw new Exception($"Error en el metodo ConfigurarEstacion.\n\tExcepcion: {e.Message}");
            }
        }
    }
}
