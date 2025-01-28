using FusionClass;
using Newtonsoft.Json.Linq;
using Polly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
            cFusion = null;
            bool connection = false;
            int retries = 1;

            // Política de reintentos
            PolicyResult policyResult = Policy.Handle<Exception>()
                .WaitAndRetry(retryCount: 4,
                              sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)),
                              onRetry: (exception, TimeSpan, conttext) =>
                              {
                                  // Cerrar el pipe en caso de fallo
                                  if (cFusion != null)
                                  {
                                      _ = cFusion.Close();
                                      cFusion = null; // Limpiar el pipe para la nueva conexión
                                  }
                                  Log.Instance.WriteLog($"\n\t  Excepción: {exception.Message.Trim()} Intento: {retries}", LogType.t_error);
                                  retries++;
                              }).ExecuteAndCapture(() =>
                              {
                                  // Crear el pipeClient si está cerrado
                                  if (cFusion == null)
                                  {
                                      cFusion = new Fusion();
                                  }

                                  cFusion.Connection(communication.GetConfiguration().IP);

                                  _ = cFusion.Echo();
                              });

            // Verificación de resultado de conexión
            if (policyResult.Outcome == 0)
            {
                _ = communication.ExecuteNonQuery($"UPDATE CheckConnection " +
                                                  $"SET isConnected = 1, fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}' " +
                                                  $"WHERE idConnection = 1");
                connection = cFusion.ConnectionStatus();
            }
            else
            {
                _ = communication.ExecuteNonQuery($"UPDATE CheckConnection " +
                                                  $"SET isConnected = 0, fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}' " +
                                                  $"WHERE idConnection = 1");
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
                                manguera.Producto.PrecioUnitario,
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
                                                                                       manguera.Producto.PrecioUnitario,
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
                        string campos = "id_producto,producto,precio";

                        string rows = string.Format("{0},'{1}',{2}",
                                                     producto.ID,
                                                     producto.Descripcion,
                                                     producto.PrecioUnitario);

                        DataTable tablaProductos = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT * " +
                                                                                               "FROM Productos " +
                                                                                              $"WHERE id_producto = {producto.ID}");

                        _ = tablaProductos.Rows.Count == 0
                            ? ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Productos ({0}) VALUES ({1})", campos, rows))
                            : ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Productos " +
                                                                                   "SET producto = ('{0}'), precio = ({1}) " +
                                                                                   "WHERE id_producto = ({2})",
                                                                                   producto.Descripcion,
                                                                                   producto.PrecioUnitario,
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
                                                     tanque.VolumenDeProducto,
                                                     tanque.CapacidadMaxima);

                        DataTable tablaTanques = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT * " +
                                                                                               "FROM Tanques " +
                                                                                              $"WHERE id_tanque = {tanque.ID}");

                        _ = tablaTanques.Rows.Count == 0 ? ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Tanques ({0}) VALUES ({1})", campos, rows)) : ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Tanques " + "SET volumen_actual = ('{0}'), capacidad_maxima = ({1}) " + "WHERE id_tanque = ({2})", tanque.VolumenDeProducto, tanque.CapacidadMaxima, tanque.ID));
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
                try
                {
                    foreach (Tanque tanque in tanques)
                    {
                        if (tanque.ID > 0)
                        {
                            FusionTankInfo fusionTank = new FusionTankInfo();
                            _ = cFusion.GetTankInfo(tanque.ID, fusionTank);

                            tanque.VolumenDeProducto = Convert.ToDouble(fusionTank.GetFuelVolume());
                            tanque.VolumenDeAgua = Convert.ToDouble(fusionTank.GetWaterVolume());
                            tanque.CapacidadMaxima = Convert.ToDouble(fusionTank.TankVolumeCapacity());
                            tanque.VolumenVacio = tanque.CapacidadMaxima - (tanque.VolumenDeProducto + tanque.VolumenDeAgua);

                            Log.Instance.WriteLog($"\nTanque {tanque.ID}, Volumen Total {tanque.CapacidadMaxima}, Volumen de Producto {tanque.VolumenDeProducto}", LogType.t_debug);

                            string campos = "id_tanque,volumen_actual,capacidad_maxima,actualizado";

                            string rows = string.Format("{0},{1},{2}",
                                                         tanque.ID,
                                                         tanque.VolumenDeProducto,
                                                         tanque.CapacidadMaxima);

                            DataTable tablaTanques = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT * " +
                                                                                                 "FROM Tanques " +
                                                                                                $"WHERE id_tanque = {tanque.ID}");

                            _ = tablaTanques.Rows.Count == 0 ?
                                ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("INSERT INTO Tanques ({0}) VALUES ({1})", campos, rows)) :
                                ConnectorSQLite.Instance.ExecuteNonQuery(string.Format("UPDATE Tanques SET volumen_actual = ({0}), capacidad_maxima = ({1}), actualizado = ('{2}') WHERE id_tanque = ({3})", tanque.VolumenDeProducto, tanque.CapacidadMaxima, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), tanque.ID));
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Error al obtener los tanques. Excepción: {e.Message}");
                }
            }
            else
            {
                throw new Exception("Conexión fallida.");
            }
        }

        public override void GrabarDespachos()
        {
            foreach (Surtidor surtidor in Station.Instance.Surtidores)
            {
                if (VerificarConexión())
                {
                    FusionSale fusionSale = new FusionSale();

                    try
                    {
                        if (cFusion.GetLastSale(surtidor.ID, fusionSale) == 1)
                        {
                            if (!fusionSale.GetAmount().Equals("0.00"))
                            {
                                string fechaHora = fusionSale.GetDateOfTransaction().Trim() + " " + fusionSale.GetInitTimeOfTransaction().Trim();
                                DateTime fechaFormateada = DateTime.ParseExact(fechaHora, "yyyyMMdd HHmmss", null);

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

                                Log.Instance.WriteLog($"Despacho obtenido: {despacho.IdDespacho}.\n", LogType.t_debug);

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
                        Log.Instance.WriteLog($"\nError al obtener la ultima venta. Excepción: {e.Message}", LogType.t_error);
                    }
                }
            }
        }

        public override void GrabarCierre()
        {
            throw new NotImplementedException();
        }

        public void CheckDiscount()
        {
            if (VerificarConexión())
            {
                GetDiscount().CheckDiscount(ConnectorFusion, cFusion);
            }
        }

        private double ConvertDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Any, culture, out double result) ? result : result;
        }
    }
    public interface ICommunication
    {
        void CheckDiscount(ConnectorFusion connectorFusion, Fusion cFusion);
        Data GetConfiguration();
        int ExecuteNonQuery(string query);
        DataTable ExecuteSelectQuery(string query);
    }

    public class PumaConnector : ICommunication
    {
        public PumaConnector() { }

        public void CheckDiscount(ConnectorFusion connectorFusion, Fusion cFusion)
        {
            DataTable tableDespachos = ExecuteSelectQuery($"SELECT * " +
                                                          $"FROM Despachos " +
                                                          $"ORDER BY id ASC LIMIT 16");

            foreach (DataRow row in tableDespachos.Rows)
            {
                if (Convert.ToString(row["AUC"]) == "0")
                {
                    // Accedes al valor de la columna 'id' por su nombre
                    int id = Convert.ToInt32(row["id"]);
                    string descuento = "";

                    if (connectorFusion.PumaDiscount(cFusion, id, ref descuento))
                    {
                        // Parsear la cadena JSON
                        var json = JObject.Parse(descuento);

                        // Datos principales
                        string authCode = json["AuthCode"].ToString();
                        int collectorId = json["CollectorId"].ToObject<int>();
                        string currencyId = json["CurrencyId"].ToString();
                        string dateCreated = json["DateCreated"].ToString();
                        string description = json["Description"].ToString();
                        string externalReference = json["external_reference"].ToString();
                        string paymentMethodId = json["payment_method_id"].ToString();
                        string paymentTypeId = json["PaymentTypeId"].ToString();

                        // PuntoVenta
                        _ = json["PuntoVenta"]["PosId"].ToString();
                        var autoliquidables = (JArray)json["PuntoVenta"]["Autoliquidables"];
                        foreach (var item in autoliquidables)
                        {
                            _ = item["cod"].ToString();
                            _ = item["glosa"].ToString();
                            _ = item["value"].ToObject<decimal>();
                        }

                        // Descuentos
                        var discounts = (JArray)json["PuntoVenta"]["Discounts"];
                        string totalGlosa = "";
                        decimal totalDiscount = 0;
                        foreach (var item in discounts)
                        {
                            string glosaDiscount = item["glosa"].ToString();
                            decimal valueDiscount = item["value"].ToObject<decimal>();
                            totalGlosa += glosaDiscount + " - ";
                            totalDiscount += valueDiscount;
                        }

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
                                      transactionAmount, paymentMethodId,
                                      status, totalGlosa, totalDiscount);

                        ExecuteNonQuery(string.Format("INSERT INTO Descuentos ({0}) VALUES ({1})", campos, rows));

                        _ = ExecuteNonQuery($"UPDATE Despachos " +
                                                 $"SET AUC = '{authCode}', " +
                                                     $"DCA = {0}, DCI = '{statementDescriptor}' , DCP = '{paymentMethodId}', " +
                                                     $"DPN = '{paymentTypeId}', TXTD = '{description}' " +
                                                     $"WHERE id = {id}");
                    }
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

    public class AxionConnector : ICommunication
    {
        public AxionConnector() { }

        public void CheckDiscount(ConnectorFusion connectorFusion, Fusion cFusion)
        {
            DataTable tableDespachos = ExecuteSelectQuery($"SELECT * " +
                               $"FROM Despachos " +
                               $"ORDER BY id ASC LIMIT 16");

            foreach (DataRow row in tableDespachos.Rows)
            {
                if (Convert.ToString(row["AUC"]) == "0")
                {
                    // Accedes al valor de la columna 'id' por su nombre
                    int id = Convert.ToInt32(row["id"]);
                    string descuento = "";

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
