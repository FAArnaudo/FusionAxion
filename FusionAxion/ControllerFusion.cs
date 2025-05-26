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
        private static readonly CultureInfo culture = CultureInfo.InvariantCulture;
        public bool IsCanceled { get; set; } = false;
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

        public void GrabarDespachos()
        {
            string debugMessage;
            foreach (Surtidor surtidor in Station.Instance.Surtidores)
            {
                debugMessage = "";
                FusionSale fusionSale = new FusionSale();

                try
                {
                    if (Fusion.GetLastSale(surtidor.ID, fusionSale) == 1)
                    {
                        if (!fusionSale.GetAmount().Equals("0.00"))
                        {

                            string fechaHora = fusionSale.GetDateOfTransaction().Trim() + " " + fusionSale.GetInitTimeOfTransaction().Trim();
                            debugMessage += $"fechaHora: {fechaHora} - ";

                            bool exito = DateTime.TryParseExact(fechaHora, "yyyyMMdd HHmmss", null, DateTimeStyles.None, out DateTime fechaFormateada);

                            if (!exito)
                            {
                                fechaFormateada = DateTime.Now;
                                Log.Instance.WriteLog($"Error de formato: {fechaFormateada}.", LogType.t_debug);
                            }

                            debugMessage += $"dechaFormateada: {fechaFormateada} - ";

                            Despacho despacho = new Despacho()
                            {
                                IdDespacho = fusionSale.GetSaleID(),
                                IdSurtidor = fusionSale.GetPumpNr(),
                                IdManguera = fusionSale.GetHoseNr(),
                                IdProducto = fusionSale.GetGradeNr(),
                                Monto = ConvertDouble(fusionSale.GetAmount()),
                                Volumen = ConvertDouble(fusionSale.GetVolume()),
                                PPU = ConvertDouble(fusionSale.GetPPU()),
                                Producto = Fusion.GetConfig().GetGradeByID(fusionSale.GetGradeNr()),
                            };
                            string fecha = fechaFormateada.ToString("dd-MM-yyyy HH:mm:ss");
                            debugMessage += $"fecha: {fecha}.";

                            Log.Instance.WriteLog($"Despacho obtenido\n" +
                                                  $"ID: {despacho.IdDespacho}\n" +
                                                  $"Monto: {despacho.Monto}\n" +
                                                  $"Volumen: {despacho.Volumen}", LogType.t_debug);

                            DataTable tablaDespachos = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT * " +
                                                                                                   $"FROM Despachos " +
                                                                                                   $"WHERE id = {despacho.IdDespacho} AND surtidor = {surtidor.ID}");

                            /// Procesamiento de la ultima venta
                            if (tablaDespachos.Rows.Count == 0)
                            {
                                /// Agregar a Base de Datos
                                bool despacho_pedido = false;
                                string campos = "id,surtidor,manguera,producto,PPU,volumen,monto,descripcion,despacho_pedido,fecha";
                                string row = string.Format("{0},{1},{2},'{3}',{4},{5},{6},'{7}',{8},'{9}'",
                                    despacho.IdDespacho,
                                    despacho.IdSurtidor,
                                    despacho.IdManguera,
                                    despacho.Producto,
                                    despacho.PPU.ToString(CultureInfo.InvariantCulture),
                                    despacho.Volumen.ToString(CultureInfo.InvariantCulture),
                                    despacho.Monto.ToString(CultureInfo.InvariantCulture),
                                    despacho.Producto,
                                    despacho_pedido,
                                    fecha);

                                _ = ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Despachos ({0}) VALUES ({1})", campos, row));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($"Error al obtener la ultima venta.\n" +
                                          $"Surtidor: {surtidor.ID}\n" +
                                          $"Excepción: {e.Message}, Debug: {debugMessage}", LogType.t_error);
                }
            }

            if (IsCanceled)
            {
                throw new TaskCanceledException();
            }
        }

        public void GrabarDespachos(Surtidor surtidor)
        {
            string debugMessage;

            debugMessage = "";
            FusionSale fusionSale = new FusionSale();

            try
            {
                if (Fusion.GetLastSale(surtidor.ID, fusionSale) == 1)
                {
                    if (!fusionSale.GetAmount().Equals("0.00"))
                    {

                        string fechaHora = fusionSale.GetDateOfTransaction().Trim() + " " + fusionSale.GetInitTimeOfTransaction().Trim();
                        debugMessage += $"fechaHora: {fechaHora} - ";

                        bool exito = DateTime.TryParseExact(fechaHora, "yyyyMMdd HHmmss", null, DateTimeStyles.None, out DateTime fechaFormateada);

                        if (!exito)
                        {
                            fechaFormateada = DateTime.Now;
                            Log.Instance.WriteLog($"Error de formato: {fechaFormateada}.", LogType.t_debug);
                        }

                        debugMessage += $"dechaFormateada: {fechaFormateada} - ";

                        Despacho despacho = new Despacho()
                        {
                            IdDespacho = fusionSale.GetSaleID(),
                            IdSurtidor = fusionSale.GetPumpNr(),
                            IdManguera = fusionSale.GetHoseNr(),
                            IdProducto = fusionSale.GetGradeNr(),
                            Monto = ConvertDouble(fusionSale.GetAmount()),
                            Volumen = ConvertDouble(fusionSale.GetVolume()),
                            PPU = ConvertDouble(fusionSale.GetPPU()),
                            Producto = Fusion.GetConfig().GetGradeByID(fusionSale.GetGradeNr()),
                        };
                        string fecha = fechaFormateada.ToString("dd-MM-yyyy HH:mm:ss");
                        debugMessage += $"fecha: {fecha}.";

                        Log.Instance.WriteLog($"Despacho obtenido\n" +
                                              $"ID: {despacho.IdDespacho}\n" +
                                              $"Monto: {despacho.Monto}\n" +
                                              $"Volumen: {despacho.Volumen}", LogType.t_debug);

                        DataTable tablaDespachos = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT * " +
                                                                                               $"FROM Despachos " +
                                                                                               $"WHERE id = {despacho.IdDespacho} AND surtidor = {surtidor.ID}");

                        /// Procesamiento de la ultima venta
                        if (tablaDespachos.Rows.Count == 0)
                        {
                            /// Agregar a Base de Datos
                            bool despacho_pedido = false;
                            string campos = "id,surtidor,manguera,producto,PPU,volumen,monto,descripcion,despacho_pedido,fecha";
                            string row = string.Format("{0},{1},{2},'{3}',{4},{5},{6},'{7}',{8},'{9}'",
                                                        despacho.IdDespacho,
                                                        despacho.IdSurtidor,
                                                        despacho.IdManguera,
                                                        despacho.Producto,
                                                        despacho.PPU.ToString(CultureInfo.InvariantCulture),
                                                        despacho.Volumen.ToString(CultureInfo.InvariantCulture),
                                                        despacho.Monto.ToString(CultureInfo.InvariantCulture),
                                                        despacho.Producto,
                                                        despacho_pedido,
                                                        fecha);

                            _ = ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Despachos ({0}) VALUES ({1})", campos, row));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error al obtener la ultima venta.\n" +
                                      $"Surtidor: {surtidor.ID}\n" +
                                      $"Excepción: {e.Message}, Debug: {debugMessage}", LogType.t_error);
            }
        }

        public void CheckDiscount()
        {
            DataTable tableDespachos = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT * " +
                                                                                   $"FROM Despachos " +
                                                                                   $"ORDER BY id ASC LIMIT 16");
            string debugMessage = "";
            foreach (DataRow row in tableDespachos.Rows)
            {
                try
                {
                    if (Convert.ToString(row["AUC"]) == "0")
                    {
                        // Accedes al valor de la columna 'id' por su nombre
                        int id = Convert.ToInt32(row["id"]);
                        string descuento = "";
                        debugMessage = $"ID: {id} - ";

                        Log.Instance.WriteLog($"Verificando decuento del despacho ID: {id}\n", LogType.t_debug);

                        if (ConnectorFusion.AxionDiscount(Fusion, id, ref descuento))
                        {
                            string AUC = "";
                            double DCA = 0;
                            double DCP = 0;
                            string DPN = "";
                            string TEXTD = "";

                            // Usamos el método Split para dividir el string por el carácter "~"
                            string[] partes = descuento.Split('~');

                            // Ahora podemos recorrer el arreglo 'partes' para acceder a cada subcadena
                            foreach (string parte in partes)
                            {
                                // Dividimos cada parte por el carácter "=" para separar el nombre del valor
                                string[] claveValor = parte.Split('=');

                                string clave = claveValor[0].Trim(); // La clave (nombre)
                                string valor = claveValor[1].Trim(); // El valor

                                switch (clave)
                                {
                                    case "AUC":
                                        AUC = valor;
                                        break;
                                    case "DCA":
                                        DCA = ConvertDouble(valor);
                                        break;
                                    case "DCP":
                                        DCP = ConvertDouble(valor.Substring(0, 5));
                                        break;
                                    case "DPN":
                                        DPN = valor;
                                        break;
                                    case "TEXTD":
                                        TEXTD = valor;
                                        break;
                                    case "TICKET":
                                        break;
                                    default:
                                        break;
                                }
                            }
                            _ = ConnectorSQLite.Instance.ExecuteNonQuery($"UPDATE Despachos " +
                                                                         $"SET AUC = '{AUC}', " + 
                                                                             $"DCA = {DCA}, " +
                                                                             $"DCP = '{Convert.ToString(DCP)}', " +
                                                                             $"DPN = '{DPN}', TXTD = '{TEXTD}' " +
                                                                         $"WHERE id = {id}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($"Error al obtener Descuentos. Excepción: {e.Message}, Mensaje: {debugMessage}\n", LogType.t_error);
                }
            }
        }

        public void GrabarCierre()
        {
            _ = ConnectorSQLite.Instance.ExecuteNonQuery("UPDATE cierreBandera SET hacerCierre = 0");

            CierreDeTurno cierre = ConnectorFusion.ComandoCierresDeTurno(Fusion);

            int bufferLimit = 999999;
            string campos = "";
            string rows = "";
            int lastID = 0;

            try
            {
                // Obtengo el ultimo id de la tabla Cierres
                DataTable tablaCierres = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT max(id) FROM Cierres");

                if (tablaCierres != null)
                {
                    lastID = Convert.ToInt32(tablaCierres.Rows[0][0]);
                }

                if (cierre.Estado.Equals("OK"))
                {
                    // Esta consulta SQL verifica si hay datos en la tabla y devuelve 1 si hay al menos un registro o 0 si está vacía.
                    bool hasData = Convert.ToBoolean(Convert.ToInt32(ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS HasData FROM Cierres").Rows[0][0]));

                    //  Comprobamos si hay datos guardados
                    if (hasData)
                    {
                        DataTable ultimoCierrePorManguera = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT * FROM CierresPorManguera WHERE id = {lastID}");

                        List<TotalPorManguera> totalesPorManguera = new List<TotalPorManguera>();

                        for (int manguera = 0; manguera < cierre.TotalesPorManguera.Count; manguera++)
                        {
                            TotalPorManguera totalPorManguera = new TotalPorManguera
                            {
                                NumeroDeManguera = cierre.TotalesPorManguera[manguera].NumeroDeManguera,
                                NumeroDeSurtidor = cierre.TotalesPorManguera[manguera].NumeroDeSurtidor,
                            };

                            double volumenAnteriorAcumulado = 0;
                            double montoAnteriorAcumulado = 0;

                            foreach (DataRow dataRow in ultimoCierrePorManguera.Rows)
                            {
                                if (Convert.ToInt32(dataRow["surtidor"]) == cierre.TotalesPorManguera[manguera].NumeroDeSurtidor && Convert.ToInt32(dataRow["manguera"]) == cierre.TotalesPorManguera[manguera].NumeroDeManguera)
                                {
                                    volumenAnteriorAcumulado = Convert.ToDouble(dataRow["volumen_acumulado"]);
                                    montoAnteriorAcumulado = Convert.ToDouble(dataRow["monto_acumulado"]);
                                    break;
                                }
                            }

                            if (cierre.TotalesPorManguera[manguera].TotalVntasVolumen - volumenAnteriorAcumulado >= 0)
                            {
                                cierre.TotalesPorManguera[manguera].TotalVntasVolumen -= volumenAnteriorAcumulado;
                            }
                            else
                            {
                                cierre.TotalesPorManguera[manguera].TotalVntasVolumen = cierre.TotalesPorManguera[manguera].TotalVntasVolumen + bufferLimit - volumenAnteriorAcumulado;
                            }

                            cierre.TotalesPorManguera[manguera].TotalVntasMonto -= montoAnteriorAcumulado;
                        }
                    }

                    InsertShift(cierre);
                }
                else
                {
                    string message = "";

                    switch (cierre.ErrorCode)
                    {
                        case "SHI0001":
                            message = "El periodo a cerrar no tiene datos.";
                            break;
                        case "SHI0002":
                            message = "Error al aplicar el cierre de periodo a la base de datos.";
                            break;
                        default:
                            message = $"Error no identificado: {cierre.Message}";
                            break;
                    }

                    cierre.Message = message;

                    //campos = "state,message";

                    //rows = string.Format("'{0}','{1}'", cierre.Estado, message);

                    //_ = ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Cierres ({0}) VALUES ({1})", campos, rows));

                    InsertShift(cierre);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error en el Cierre de turno. Excepción: {e.Message}");
            }
        }

        private void InsertShift(CierreDeTurno cierre)
        {
            string fields = "id_cierre,fecha,state,message";
            string values = string.Format("{0},'{1}','{2}','{3}'",
                                           cierre.ID,
                                           DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                                           cierre.Estado,
                                           cierre.Message);

            _ = ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Cierres ({0}) VALUES ({1})", fields, values));

            // Traer ID del cierre para poder referenciar los detalles
            DataTable tablaCierres = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT max(id) FROM Cierres");

            int id = Convert.ToInt32(tablaCierres.Rows[0][0]);

            // Grabar CierresPorManguera
            fields = "id,surtidor,manguera,monto,volumen,monto_acumulado,volumen_acumulado";

            for (int manguera = 0; manguera < cierre.TotalesPorManguera.Count; manguera++)
            {
                values = string.Format("{0},{1},{2},{3},{4},{5},{6}",
                                        id,
                                        cierre.TotalesPorManguera[manguera].NumeroDeSurtidor,
                                        cierre.TotalesPorManguera[manguera].NumeroDeManguera,
                                        cierre.TotalesPorManguera[manguera].TotalVntasMonto.ToString("F2", CultureInfo.InvariantCulture),
                                        cierre.TotalesPorManguera[manguera].TotalVntasVolumen.ToString("F2", CultureInfo.InvariantCulture),
                                        cierre.TotalesPorManguera[manguera].TotalVntasSinControlMonto.ToString("F2", CultureInfo.InvariantCulture),
                                        cierre.TotalesPorManguera[manguera].TotalVntasSinControlVolumen.ToString("F2", CultureInfo.InvariantCulture));

                Log.Instance.WriteLog($"Consulta: INSERT INTO CierresPorManguera ({fields}) VALUES ({values})", LogType.t_debug);

                _ = ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO CierresPorManguera ({0}) VALUES ({1})", fields, values));
            }
        }
        private double ConvertDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Any, culture, out double result) ? result : result;
        }
    }
}
