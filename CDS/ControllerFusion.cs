using FusionClass;
using Newtonsoft.Json.Linq;
using Polly;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace CDS
{
    public class ControllerFusion : Controller
    {
        private readonly ConnectorFusion ConnectorFusion;
        private readonly ICommunication communication;
        private Fusion cFusion;
        private static readonly CultureInfo culture = CultureInfo.InvariantCulture;

        public ControllerFusion(ICommunication communication)
        {
            ConnectorFusion = new ConnectorFusion();
            this.communication = communication;
        }

        private ICommunication GetDiscount()
        {
            return communication;
        }

        public override bool VerificarConexión()
        {
            bool connection = false;
            int retries = 1;

            try
            {
                // Si la instancia ya existe y la conexión es válida, no hacemos nada
                if (cFusion != null && cFusion.ConnectionStatus())
                {
                    Log.Instance.WriteLog("Conexión existente válida\n", LogType.t_debug);
                    return true;
                }

                // Si la conexión no es válida, limpiamos la instancia
                cFusion?.Close();
                cFusion = null;

                PolicyResult policyResult = Policy.Handle<Exception>()
                    .WaitAndRetry(retryCount: 4,
                                  sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)),
                                  onRetry: (exception, TimeSpan, context) =>
                                  {
                                      Log.Instance.WriteLog($"Excepción: {exception.Message.Trim()} Intento: {retries}\n", LogType.t_error);
                                      retries++;
                                  }).ExecuteAndCapture(() =>
                                  {
                                      // Crear nueva instancia solo si es necesario
                                      cFusion = new Fusion();
                                      Log.Instance.WriteLog("Instancia fusion creada\n", LogType.t_debug);

                                      // Intentar conectar con timeout de 5 segundos
                                      Task connectionTask = Task.Run(() => cFusion.Connection(communication.GetConfiguration().IP));
                                      Task timeoutTask = Task.Delay(5000);

                                      Task.WhenAny(connectionTask, timeoutTask).Wait();

                                      if (connectionTask.IsCompleted)
                                      {
                                          connection = cFusion.ConnectionStatus();
                                          Log.Instance.WriteLog($"Conexión establecida: {connection}\n", LogType.t_debug);
                                      }
                                      else
                                      {
                                          Log.Instance.WriteLog("Tiempo de espera agotado para la conexión\n", LogType.t_error);
                                      }
                                  });

                // Registrar el estado en la base de datos
                communication.ExecuteNonQuery($"UPDATE CheckConnection " +
                                              $"SET isConnected = {(connection ? 1 : 0)}, fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}' " +
                                              $"WHERE idConnection = 1");
            }
            catch (Exception ex)
            {
                Log.Instance.WriteLog($"Error en VerificarConexión: {ex.Message.Trim()}\n", LogType.t_error);
            }

            return connection;
        }

        public override void ConfigurarEstacion()
        {
            if (VerificarConexión())
            {
                ConnectorFusion.ComandoConfiguracionDeLaEstacion(cFusion);

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

                            DataTable tablaSurtidores = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT * " +
                                                                                                    $"FROM Surtidores " +
                                                                                                    $"WHERE IdSurtidor = {surtidor.ID} AND Manguera = {manguera.ID}");

                            _ = tablaSurtidores.Rows.Count == 0
                                ? ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Surtidores ({0}) VALUES ({1})", campos, rows))
                                : ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Surtidores " +
                                                                                       "SET Producto = ('{0}'), Precio = ('{1}'), DescProd = ('{2}') " +
                                                                                       "WHERE IdSurtidor = ({3}) AND Manguera = ('{4}')",
                                                                                        manguera.Producto.ID,
                                                                                        manguera.Producto.PrecioUnitario.ToString(CultureInfo.InvariantCulture),
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

                        string rows = string.Format("{0},{1},'{2}',{3}",
                                                     producto.ID,
                                                     producto.ID_SIGES,
                                                     producto.Descripcion,
                                                     producto.PrecioUnitario.ToString(CultureInfo.InvariantCulture));

                        DataTable tablaProductos = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT * " +
                                                                                               "FROM Productos " +
                                                                                              $"WHERE id_producto = {producto.ID}");

                        _ = tablaProductos.Rows.Count == 0
                            ? ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Productos ({0}) VALUES ({1})", campos, rows))
                            : ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Productos " +
                                                                                     "SET producto = ('{0}'), precio = ({1}) " +
                                                                                     "WHERE id_producto = ({2})",
                                                                                      producto.Descripcion,
                                                                                      producto.PrecioUnitario.ToString(CultureInfo.InvariantCulture),
                                                                                      producto.ID));

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

                        DataTable tablaTanques = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT * " +
                                                                                             "FROM Tanques " +
                                                                                            $"WHERE id_tanque = {tanque.ID}");

                        _ = tablaTanques.Rows.Count == 0
                            ? ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Tanques ({0}) VALUES ({1})", campos, rows))
                            : ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Tanques " +
                                                                                     "SET volumen_actual = ('{0}'), capacidad_maxima = ({1}) " +
                                                                                     "WHERE id_tanque = ({2})",
                                                                                      tanque.VolumenDeProducto,
                                                                                      tanque.CapacidadMaxima,
                                                                                      tanque.ID));

                        Log.Instance.WriteLog(string.Format("TANQUE: ({0}))", tanque.ID), LogType.t_info);
                    }
                    Log.Instance.WriteLog("\n", LogType.t_info);
                }
                catch (Exception e)
                {
                    throw new Exception($"Error en el metodo ConfigurarEstacion.\n\tExcepcion: {e.Message}");
                }
            }
        }

        public override void ActualizarProductos()
        {
            throw new NotImplementedException();
        }

        public override void ActualizarTanques()
        {
            List<Tanque> tanques = Station.Instance.Tanques;

            if (VerificarConexión())
            {
                foreach (Tanque tanque in tanques)
                {
                    try
                    {
                        if (tanque.ID > 0)
                        {
                            FusionTankInfo fusionTank = new FusionTankInfo();
                            _ = cFusion.GetTankInfo(tanque.ID, fusionTank);

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
                    }
                    catch (Exception e)
                    {
                        Log.Instance.WriteLog($"Error al actualizar el tanque(id) : {tanque.ID}. Excepción: {e.Message}", LogType.t_error);
                    }
                }
            }
            else
            {
                throw new Exception("Conexión fallida.");
            }
        }

        public override void GrabarDespachos()
        {
            string debugMessage;
            foreach (Surtidor surtidor in Station.Instance.Surtidores)
            {
                debugMessage = "";
                if (VerificarConexión())
                {
                    FusionSale fusionSale = new FusionSale();

                    try
                    {
                        if (cFusion.GetLastSale(surtidor.ID, fusionSale) == 1)
                        {
                            if (fusionSale != null && !fusionSale.GetAmount().Equals("0.00"))
                            {

                                string fechaHora = fusionSale.GetDateOfTransaction().Trim() + " " + fusionSale.GetInitTimeOfTransaction().Trim();
                                debugMessage += $"fechaHora: {fechaHora} - ";

                                bool exito = DateTime.TryParseExact(fechaHora, "yyyyMMdd HHmmss", null, DateTimeStyles.None, out DateTime fechaFormateada);

                                if (!exito)
                                {
                                    fechaFormateada = DateTime.Now;
                                    Log.Instance.WriteLog($"Error de formato: {fechaFormateada}.", LogType.t_debug);
                                }

                                debugMessage += $"fechaFormateada: {fechaFormateada} - ";

                                Despacho despacho = new Despacho()
                                {
                                    IdDespacho = fusionSale.GetSaleID(),
                                    IdSurtidor = fusionSale.GetPumpNr(),
                                    IdManguera = fusionSale.GetHoseNr(),
                                    IdProducto = fusionSale.GetGradeNr(),
                                    Monto = ConvertDouble(fusionSale.GetAmount()),
                                    Volumen = ConvertDouble(fusionSale.GetVolume()),
                                    PPU = ConvertDouble(fusionSale.GetPPU()),
                                    Producto = cFusion.GetConfig().GetGradeByID(fusionSale.GetGradeNr()),
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
                                              $"Surtidor: {surtidor.ID},\n" +
                                              $"Excepción: {e.Message},\n," +
                                              $"Debug: {debugMessage}", LogType.t_error);
                    }
                }

                FusionProcess.CheckFlags();

                if (FusionProcess.HacerCierre || FusionProcess.BreakProces)
                {
                    break;
                }
            }
        }

        public void CheckDiscount()
        {
            if (VerificarConexión())
            {
                GetDiscount().CheckDiscount(ConnectorFusion, cFusion);
            }
        }

        public override void GrabarCierre()
        {
            int bufferLimit = 999999;
            int tries = 0;
            int stopTries = 3;

            while (tries < stopTries)
            {
                if (VerificarConexión())
                {
                    break;
                }
                tries++;
            }

            if (tries >= stopTries)
            {
                throw new Exception("Conexión fallida. No se realizará el Cierre de Turno.");
            }

            CierreFusion cierre = ConnectorFusion.ComandoCierresDeTurno(cFusion);

            string campos;
            string rows;
            int lastID;

            try
            {
                _ = ConnectorSQLite.Instance.ExecuteNonQuery("UPDATE cierreBandera SET hacerCierre = 0");

                if (cierre.Estado.Equals("OK"))
                {
                    // Esta consulta SQL verifica si hay datos en la tabla y devuelve 1 si hay al menos un registro o 0 si está vacía.
                    bool hasData = Convert.ToBoolean(Convert.ToInt32(ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS HasData FROM Cierres").Rows[0][0]));

                    //  Comprobamos si hay datos guardados
                    if (hasData)
                    {
                        // Obtengo el ultimo id de la tabla Cierres
                        DataTable tablaCierres = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT max(id) FROM Cierres");
                        lastID = Convert.ToInt32(tablaCierres.Rows[0][0]);

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
                    string message;

                    switch (cierre.ErrorCode)
                    {
                        case "SHI0001":
                            message = "El periodo a cerrar no tiene datos";
                            break;
                        case "SHI0002":
                            message = "Error al aplicar el cierre de periodo a la base de datos";
                            break;
                        default:
                            message = "Error no identificado";
                            break;
                    }

                    campos = "state,message";

                    rows = string.Format("'{0}','{1}'", cierre.Estado, message);

                    _ = communication.ExecuteNonQuery(string.Format("INSERT INTO Cierres ({0}) VALUES ({1})", campos, rows));

                    // Grabar CierresPorManguera
                    string fields = "id,surtidor,manguera,monto,volumen,monto_acumulado,volumen_acumulado";

                    // Traer ID del cierre para poder referenciar los detalles
                    DataTable tablaCierres = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT max(id) FROM Cierres");

                    int id = Convert.ToInt32(tablaCierres.Rows[0][0]);

                    for (int manguera = 0; manguera < cierre.TotalesPorManguera.Count; manguera++)
                    {
                        string values = string.Format("{0},{1},{2},{3},{4},{5},{6}",
                                                id,
                                                cierre.TotalesPorManguera[manguera].NumeroDeSurtidor,
                                                cierre.TotalesPorManguera[manguera].NumeroDeManguera,
                                                0.ToString("F2", CultureInfo.InvariantCulture),
                                                0.ToString("F2", CultureInfo.InvariantCulture),
                                                0.ToString("F2", CultureInfo.InvariantCulture),
                                                0.ToString("F2", CultureInfo.InvariantCulture));

                        Log.Instance.WriteLog($"Consulta: INSERT INTO CierresPorManguera ({fields}) VALUES ({values})", LogType.t_debug);

                        _ = ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO CierresPorManguera ({0}) VALUES ({1})", fields, values));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error en el Cierre de turno. Excepción: {e.Message}");
            }
        }

        private void InsertShift(CierreFusion cierre)
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

        public void CloseConnection()
        {
            _ = cFusion.Close();
        }

        private double ConvertDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Any, culture, out double result) ? result : result;
        }

        public override void GrabarDespachos(Surtidor surtidor)
        {
            throw new NotImplementedException();
        }
    }

    public interface ICommunication
    {

        /// <summary>
        /// Verifica y aplica descuentos a los despachos en la base de datos.
        /// </summary>
        /// <param name="connectorFusion">Instancia de ConnectorFusion para manejar la conexión.</param>
        /// <param name="cFusion">Objeto Fusion utilizado en la conexión.</param>
        void CheckDiscount(ConnectorFusion connectorFusion, Fusion cFusion);

        /// <summary>
        /// Obtiene la configuración actual de la aplicación.
        /// </summary>
        /// <returns>Objeto Data con la configuración.</returns>
        Data GetConfiguration();

        /// <summary>
        /// Ejecuta una consulta SQL que no devuelve resultados.
        /// </summary>
        /// <param name="query">Consulta SQL a ejecutar.</param>
        /// <returns>Número de filas afectadas.</returns>
        int ExecuteNonQuery(string query);

        /// <summary>
        /// Ejecuta una consulta SQL y devuelve los resultados en un DataTable.
        /// </summary>
        /// <param name="query">Consulta SQL a ejecutar.</param>
        /// <returns>DataTable con los resultados.</returns>
        DataTable ExecuteSelectQuery(string query);
    }

    public class PumaConnector : ICommunication
    {
        public PumaConnector() { }

        public void CheckDiscount(ConnectorFusion connectorFusion, Fusion cFusion)
        {
            DataTable tableDespachos = ExecuteSelectQuery($"SELECT * " +
                                                          $"FROM Despachos " +
                                                          $"ORDER BY id DESC LIMIT 20");
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

                        if (connectorFusion.PumaDiscount(cFusion, id, ref descuento))
                        {
                            // Parsear la cadena JSON
                            JObject json = JObject.Parse(descuento);

                            SaveAnswer(json, "descuento_puma.json");

                            debugMessage += "Datos Principales - ";
                            // Datos principales
                            string authCode = json["AuthCode"].ToString();
                            int collectorId = json["CollectorId"].ToObject<int>();
                            string currencyId = json["CurrencyId"].ToString();
                            string dateCreated = json["DateCreated"].ToString();
                            string description = json["Description"].ToString();
                            string externalReference = json["external_reference"].ToString();
                            string paymentMethodId = json["payment_method_id"].ToString();
                            string paymentTypeId = json["PaymentTypeId"].ToString();

                            debugMessage += "PuntoVenta - ";
                            // PuntoVenta
                            _ = json["PuntoVenta"]["PosId"].ToString();
                            JArray autoliquidables = (JArray)json["PuntoVenta"]["Autoliquidables"];
                            string cod_auto = "";
                            string glosa_auto = "";
                            decimal valor_auto = 0;
                            foreach (JToken item in autoliquidables)
                            {
                                cod_auto = item["cod"].ToString();
                                glosa_auto = item["glosa"].ToString();
                                valor_auto = item["value"].ToObject<decimal>();
                            }

                            debugMessage += "Descuentos - ";
                            // Descuentos
                            JArray discounts = (JArray)json["PuntoVenta"]["Discounts"];
                            string totalGlosa = "";
                            decimal totalDiscount = 0;
                            foreach (JToken item in discounts)
                            {
                                string glosaDiscount = item["glosa"].ToString();
                                decimal valueDiscount = item["value"].ToObject<decimal>();
                                totalGlosa += glosaDiscount + " - ";
                                totalDiscount += valueDiscount;
                            }

                            debugMessage += "Otros datos - ";
                            // Otros datos
                            _ = json["PuntoVenta"]["Fecha"].ToString();
                            _ = json["PuntoVenta"]["TotalPagoUsuario"].ToObject<decimal>();
                            _ = json["PuntoVenta"]["TotalTransaccion"].ToObject<decimal>();
                            _ = json["PuntoVenta"]["TotalTransaccionSinDescuentos"].ToObject<decimal>();

                            string statementDescriptor = json["StatementDescriptor"].ToString();
                            string status = json["status"].ToString();
                            decimal transactionAmount = json["TransactionAmount"].ToObject<decimal>();

                            string campos = "external_reference,AuthCode,CollectorId,CurrencyId,DateCreated,Description,PaymentTypeId," +
                                            "StatementDescriptor,TransactionAmount,payment_method_id,status,Glosa,Descuento";

                            string rows = string.Format("{0},{1},{2},'{3}','{4}','{5}','{6}','{7}',{8},'{9}','{10}','{11}',{12}",
                                          externalReference, authCode, collectorId,
                                          currencyId, dateCreated, description,
                                          paymentTypeId, statementDescriptor,
                                          transactionAmount.ToString(CultureInfo.InvariantCulture), paymentMethodId,
                                          status, totalGlosa, totalDiscount.ToString(CultureInfo.InvariantCulture));

                            if (ExecuteSelectQuery($"SELECT * FROM Descuentos WHERE external_reference = {externalReference}") != null)
                            {
                                _ = ExecuteNonQuery(string.Format("INSERT INTO Descuentos ({0}) VALUES ({1})", campos, rows));
                            }

                            _ = ExecuteNonQuery($"UPDATE Despachos " +
                                                     $"SET AUC = '{authCode}', " +
                                                         $"DCA = {totalDiscount}, DCI = '{statementDescriptor}' , DCP = '{paymentMethodId}', " +
                                                         $"DPN = '{paymentTypeId}', TXTD = '{totalGlosa}', cod_auto = '{cod_auto}', " +
                                                         $"glosa_auto = '{glosa_auto}', valor_auto = {valor_auto.ToString(CultureInfo.InvariantCulture)} " +
                                                         $"WHERE id = {id}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($"\nError al obtener Descuentos. Excepción: {e.Message}, Mensaje: {debugMessage}", LogType.t_error);
                }

                FusionProcess.CheckFlags();

                if (FusionProcess.HacerCierre || FusionProcess.BreakProces)
                {
                    break;
                }
            }
        }
        public Data GetConfiguration()
        {
            return Configuration.GetConfiguration();
        }
        public int ExecuteNonQuery(string query)
        {
            return ConnectorSQLite.Instance.ExecuteNonQuery(query);
        }
        public DataTable ExecuteSelectQuery(string query)
        {
            return ConnectorSQLite.Instance.ExecuteSelectQuery(query);
        }
        public static void SaveAnswer(JObject jsonObject, string nombreArchivo)
        {
            // Crear el directorio si no existe
            string directorio = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Responses");
            if (!Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            // Definir la ruta completa del archivo
            string rutaArchivo = Path.Combine(directorio, nombreArchivo);

            // Guardar el JObject en el archivo
            File.WriteAllText(rutaArchivo, jsonObject.ToString());
        }
    }

    public class AxionConnector : ICommunication
    {
        public AxionConnector() { }
        public void CheckDiscount(ConnectorFusion connectorFusion, Fusion cFusion)
        {
            DataTable tableDespachos = ExecuteSelectQuery($"SELECT * " +
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

                        Log.Instance.WriteLog($"Verificando descuento del despacho ID: {id}\n", LogType.t_debug);

                        if (connectorFusion.AxionDiscount(cFusion, id, ref descuento))
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
                                        DCA = connectorFusion.ConvertDouble(valor);
                                        break;
                                    case "DCP":
                                        DCP = connectorFusion.ConvertDouble(valor.Substring(0, 5));
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
                            _ = ExecuteNonQuery($"UPDATE Despachos " +
                                                     $"SET AUC = '{AUC}', " +
                                                         $"DCA = {DCA}, DCP = '{Convert.ToString(DCP)}', " +
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
        public Data GetConfiguration()
        {
            return Configuration.GetConfiguration();
        }
        public int ExecuteNonQuery(string query)
        {
            return ConnectorSQLite.Instance.ExecuteNonQuery(query);
        }
        public DataTable ExecuteSelectQuery(string query)
        {
            return ConnectorSQLite.Instance.ExecuteSelectQuery(query);
        }
    }
}
