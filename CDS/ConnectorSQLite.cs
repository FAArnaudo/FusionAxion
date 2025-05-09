using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace CDS
{
    public class ConnectorSQLite
    {
        // Instancia estática privada
        private static ConnectorSQLite instance = null;
        private IGetConfiguration configuration;

        // Nombre de la base de datos y cadena de conexión
        private readonly string databaseName = "cds.db";
        private readonly string connectionString = "Data Source='{0}';Version=3;";

        // Conexión a la base de datos SQLite
        private SQLiteConnection connection;

        // Un objeto que se utilizará para la sincronización
        private static readonly object lockObjectDB = new object();

        // Constructor privado
        private ConnectorSQLite() { }

        /// <summary>
        /// Propiedad pública estática para obtener la instancia del Singleton
        /// </summary>
        public static ConnectorSQLite Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConnectorSQLite();
                }

                return instance;
            }
        }

        /// <summary>
        /// Método para crear la base de datos si no existe
        /// </summary>
        /// <returns>retorna true si la creación fue exitosa o false en caso contrario</returns>
        public bool CreateDatabase(IGetConfiguration getConfiguration)
        {
            configuration = getConfiguration;

            string folderPath = configuration.GetConfiguration().RutaProyNuevo + "\\CDS\\";
            string databasePath = Path.Combine(folderPath, databaseName);

            lock (lockObjectDB)
            {
                try
                {
                    // Crear la carpeta si no existe
                    if (!Directory.Exists(folderPath))
                    {
                        _ = Directory.CreateDirectory(folderPath);
                    }

                    // Crear la base de datos si no existe
                    if (!File.Exists(databasePath))
                    {
                        SQLiteConnection.CreateFile(databasePath);
                    }

                    CreateTables();

                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al crear la base de datos: {ex.Message}.\n");
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        /// <summary>
        /// Método para ejecutar una consulta de tipo SELECT y retornar un DataTable
        /// </summary>
        /// <param name="query"></param>
        /// <returns>Retorna el DataTable de la consulta de selección o null en caso de error</returns>
        public DataTable ExecuteSelectQuery(string query)
        {
            lock (lockObjectDB)
            {
                try
                {
                    // Abrir la conexión
                    _ = OpenConnection();

                    using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(cmd))
                        {
                            DataTable dataTable = new DataTable();
                            _ = dataAdapter.Fill(dataTable); // Llenar el DataTable con los resultados
                            return dataTable;
                        }
                    }
                }
                catch (SQLiteException e)
                {
                    connection = null;
                    Log.Instance.WriteLog($"Error al ejecutar ExecuteSelectQuery {query}. Excepción: {e.Message}\n", LogType.t_error);
                    return null;
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($"Error al ejecutar SELECT: {e.Message}.\n", LogType.t_error);
                    return null;
                }
                finally
                {
                    // Cerrar la conexión al final de la operación
                    CloseConnection();
                }
            }
        }

        /// <summary>
        /// Método para ejecutar una consulta de tipo INSERT, UPDATE, DELETE y retornar el número de filas afectadas
        /// </summary>
        /// <param name="query"></param>
        /// <returns>retorna el numero de campos modificados o 0 si no haya modificacion. En caso de error retorna -1</returns>
        public int ExecuteNonQuery(string query)
        {
            lock (lockObjectDB)
            {
                try
                {
                    // Abrir la conexión
                    _ = OpenConnection();

                    using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                    {
                        int rowsAffected = cmd.ExecuteNonQuery(); // Ejecuta el comando y devuelve el número de filas afectadas
                        return rowsAffected;
                    }
                }
                catch (SQLiteException e)
                {
                    connection = null;
                    Log.Instance.WriteLog($"Error al ejecutar ExecuteNonQuery {query}. Excepción: {e.Message}.\n", LogType.t_error);
                    return -1;
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteLog($"Error al ejecutar INSERT/UPDATE/DELETE: {ex.Message}.\n", LogType.t_error);
                    return -1; // En caso de error, retornamos -1 (ninguna fila afectada)
                }
                finally
                {
                    // Cerrar la conexión al final de la operación
                    CloseConnection();
                }
            }
        }

        /// <summary>
        /// Método para ejecutar una consulta que retornen un estado
        /// </summary>
        /// <param name="query"></param>
        /// <returns>true si hubo inserción o modificación, y false en caso contrario</returns>
        public bool ExecuteStateQuery(string query)
        {
            lock (lockObjectDB)
            {
                try
                {
                    _ = OpenConnection();
                    using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                    {
                        int result = Convert.ToInt32(cmd.ExecuteScalar());
                        return result == 1;
                    }

                }
                catch (Exception ex)
                {
                    Log.Instance.WriteLog($"Error al ejecutar una consulta de estados: {ex.Message}.\n", LogType.t_error);
                    return false;
                }
                finally
                {
                    // Cerrar la conexión al final de la operación
                    CloseConnection();
                }
            }

        }

        /// <summary>
        /// Crea todas las tablas necesarias para que el sistema pueda almacenar la informacón
        /// </summary>
        private void CreateTables()
        {
            try
            {
                _ = OpenConnection();
                string createTableQuery = "CREATE TABLE IF NOT EXISTS Surtidores " +
                                          "(IdSurtidor INTEGER, Manguera  INTEGER, Producto  INTEGER, " +
                                          "Precio REAL, DescProd TEXT)";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Tanques (id_tanque INTEGER PRIMARY KEY, " +
                                   "volumen_actual REAL NOT NULL, capacidad_maxima REAL NOT NULL, " +
                                   "actualizado TEXT DEFAULT((strftime('%d-%m-%Y %H:%M:%S', 'now', 'localtime'))));";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Despachos " +
                                   "(id INTEGER NOT NULL, surtidor INTEGER NOT NULL, " +
                                   "manguera INTEGER, producto INTEGER NOT NULL, " +
                                   "PPU REAL NOT NULL, volumen REAL NOT NULL, " +
                                   "monto REAL NOT NULL, descripcion TEXT, facturado INTEGER, YPFRuta INTEGER, " +
                                   "despacho_pedido INTEGER, fecha TEXT DEFAULT(datetime('now', 'localtime')), " +
                                   "AUC TEXT DEFAULT '0', DCA REAL, DCI TEXT , DCP TEXT, " +
                                   "DPN TEXT, TXTD TEXT, cod_auto TEXT, glosa_auto TEXT, " +
                                   "valor_auto REAL, PRIMARY KEY(id,surtidor))";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS cierreBandera " +
                                   "(hacerCierre INTEGER NOT NULL, actualizar_tanques INTEGER NOT NULL DEFAULT 0, " +
                                   "cierre_anterior INTEGER NOT NULL DEFAULT 0);" +
                                   "\nINSERT INTO cierreBandera (hacerCierre) " +
                                   "SELECT 0 WHERE NOT EXISTS (SELECT 1 FROM cierreBandera)";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Cierres " +
                                   "(id INTEGER, id_cierre INTEGER, fecha TEXT DEFAULT((strftime('%d-%m-%Y %H:%M:%S', 'now', 'localtime'))), " +
                                   "monto_contado TEXT, volumen_contado TEXT, " +
                                   "monto_YPFruta TEXT, volumen_YPFruta TEXT, state TEXT, message TEXT, PRIMARY KEY(id AUTOINCREMENT))";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS CierresPorManguera " +
                                   "(id INTEGER NOT NULL, surtidor INTEGER NOT NULL, " +
                                   "manguera INTEGER NOT NULL, monto REAL, " +
                                   "volumen REAL, monto_acumulado REAL, volumen_acumulado REAL)";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS CierresPorProducto " +
                                   "(id INTEGER NOT NULL, producto INTEGER NOT NULL, " +
                                   "monto REAL NOT NULL, volumen REAL NOT NULL)";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Productos " +
                                   "(id_producto INTEGER PRIMARY KEY, id_siges INTEGER, " +
                                   "numero_despacho INTEGER, producto TEXT NOT NULL, precio REAL NOT NULL)";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS CheckConnection " +
                                  "(idConnection INTEGER PRIMARY KEY, isConnected INTEGER, fecha date DEFAULT((strftime('%d-%m-%Y %H:%M:%S', 'now', 'localtime'))));" +
                                  "\nINSERT INTO CheckConnection (idConnection, isConnected)" +
                                  "\nSELECT 1, 0 " +
                                  "\nWHERE NOT EXISTS (SELECT 1 FROM CheckConnection WHERE idConnection = 1)";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Datos_CIO " +
                                   "(id_cio INTEGER PRIMARY KEY, ip_vox TEXT NOT NULL, ip_bridge TEXT NOT NULL, " +
                                   "ip_server TEXT NOT NULL, ip_libre TEXT NOT NULL, " +
                                   "ruteo_estatico TEXT, fecha date DEFAULT((strftime('%d-%m-%Y %H:%M:%S', 'now', 'localtime'))));" +
                                   "\nINSERT INTO Datos_CIO (id_cio, ip_vox, ip_bridge, ip_server, ip_libre, ruteo_estatico)" +
                                   "\nSELECT 1, '', '', '', '', '' " +
                                   "\nWHERE NOT EXISTS (SELECT 1 FROM Datos_CIO WHERE id_cio = 1);";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Descuentos " +
                                   "(external_reference INTEGER, AuthCode INTEGER, CollectorId INTEGER, " +
                                   "CurrencyId TEXT, DateCreated TEXT, Description TEXT, PaymentTypeId TEXT, " +
                                   "StatementDescriptor TEXT, TransactionAmount REAL, payment_method_id TEXT, status TEXT, " +
                                   "Glosa TEXT, Descuento REAL, fecha date DEFAULT((strftime('%d-%m-%Y %H:%M:%S', 'now', 'localtime'))));";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Autorizaciones (" +
                                   "id_autorizacion INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                   "surtidor INTEGER NOT NULL, " +
                                   "manguera INTEGER, " +
                                   "monto_reset REAL, " +
                                   "estado TEXT NOT NULL DEFAULT 'Pendiente', " +
                                   "fecha_solicitud DATETIME DEFAULT CURRENT_TIMESTAMP, " +
                                   "autorizado INTEGER, " +
                                   "cancelada_por_POS INTEGER NOT NULL DEFAULT 0, " +
                                   "cancelada_por_sistema INTEGER NOT NULL DEFAULT 0, " +
                                   "fecha_procesamiento DATETIME, " +
                                   "fecha_cancelacion_POS DATETIME, " +
                                   "fecha_cancelacion DATETIME, " +
                                   "observaciones TEXT); " +

                                   "CREATE INDEX IF NOT EXISTS idx_autorizaciones_estado " +
                                   "ON Autorizaciones (Estado); " +

                                   "CREATE INDEX IF NOT EXISTS idx_autorizaciones_cancelaciones " +
                                   "ON Autorizaciones (CanceladaPorPOS, CanceladaPorSistema); " +

                                   "CREATE INDEX IF NOT EXISTS idx_autorizaciones_surtidor " +
                                   "ON Autorizaciones (Surtidor);";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteLog($"Error al crear tabla: {ex.Message}.\n", LogType.t_error);
            }
            finally
            {
                // Cerrar la conexión al final de la operación
                CloseConnection();
            }
        }

        /// <summary>
        /// Método para abrir la conexión a la base de datos
        /// </summary>
        /// <returns></returns>
        private SQLiteConnection OpenConnection()
        {
            if (connection == null)
            {
                connection = new SQLiteConnection(string.Format(connectionString, configuration.GetConfiguration().RutaProyNuevo + "\\CDS\\" + databaseName));
            }

            if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteLog($"Error al abrir la conexión: {ex.Message}", LogType.t_error);
                    throw new Exception($"No se pudo abrir la conexión a la base de datos. Excepcion: {ex.Message}.\n");
                }
            }
            return connection;
        }

        /// <summary>
        /// Método para cerrar la conexión a la base de datos
        /// </summary>
        private void CloseConnection()
        {
            if (connection != null && connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
        }
    }
}
