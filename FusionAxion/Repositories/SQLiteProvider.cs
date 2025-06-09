using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion.Repositories
{
    public class SQLiteProvider : IDatabaseProvider
    {
        private readonly string connectionString;
        private SQLiteConnection connection;
        // Un objeto que se utilizará para la sincronización
        private static readonly object lockObjectDB = new object();

        public SQLiteProvider()
        {
            string dbPath = Path.Combine(ConfigurationModel.GetConfiguration().RutaProyNuevo, "CDS", "cds.db");
            connectionString = $"Data Source={dbPath};Version=3;Pooling=False;Journal Mode=WAL;Mutex=Full;";
        }

        public IDbConnection OpenConnection()
        {
            // Intentamos crear la conexión solo si no existe aún
            if (connection == null)
            {
                connection = new SQLiteConnection(connectionString);
            }

            // Verificamos el estado de la conexión
            if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
            {
                try
                {
                    // Intentamos abrir la conexión
                    connection.Open();
                }
                catch (SQLiteException sqlEx)
                {
                    // Manejo específico de excepciones de SQLite
                    throw new Exception("Error al abrir la conexión a la base de datos.", sqlEx);
                }
                catch (UnauthorizedAccessException uaeEx)
                {
                    // Manejo de error de permisos
                    throw new Exception("No tiene permisos para acceder a la base de datos.", uaeEx);
                }
                catch (FileNotFoundException fnfEx)
                {
                    // Manejo de error si el archivo no se encuentra
                    throw new Exception("Archivo de base de datos no encontrado.", fnfEx);
                }
                catch (Exception ex)
                {
                    // Manejo general de otras excepciones
                    throw new Exception("No se pudo abrir la conexión a la base de datos.", ex);
                }
            }

            return connection;
        }

        public DataTable ExecuteSelect(string query)
        {
            lock (lockObjectDB)
            {
                try
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(query, (SQLiteConnection)OpenConnection()))
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
                    Log.Instance.WriteLog($"Error al ejecutar ExecuteSelectQuery {query}.\nExcepción: {e.Message}", LogType.t_error);
                    return null;
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLog($"[SQLite] Error en ExecuteSelect: {e.Message}", LogType.t_error);
                    return null;
                }
                finally
                {
                    // Cerrar la conexión al final de la operación
                    CloseConnection();
                }
            }

        }

        public int ExecuteNonQuery(string query)
        {
            lock (lockObjectDB)
            {
                try
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(query, (SQLiteConnection)OpenConnection()))
                    {
                        return cmd.ExecuteNonQuery(); // Ejecuta el comando y devuelve el número de filas afectadas
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteLog($"[SQLite] Error en ExecuteNonQuery: {ex.Message}", LogType.t_error);
                    return -1;
                }
                finally
                {
                    // Cerrar la conexión al final de la operación
                    CloseConnection();
                }
            }
        }

        public bool ExecuteScalarAsBool(string query)
        {
            lock (lockObjectDB)
            {
                try
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(query, (SQLiteConnection)OpenConnection()))
                    {
                        int result = Convert.ToInt32(cmd.ExecuteScalar());
                        return result == 1;
                    }

                }
                catch (Exception ex)
                {
                    Log.Instance.WriteLog($"[SQLite] Error en ExecuteScalarAsBool: {ex.Message}", LogType.t_error);
                    return false;
                }
                finally
                {
                    // Cerrar la conexión al final de la operación
                    CloseConnection();
                }
            }
        }

        public void CloseConnection()
        {
            if (connection != null && connection.State != ConnectionState.Closed)
                connection.Close();
        }

        public void InitializeDatabase()
        {
            CreateDatabase();
            CreateTables();
        }

        private void CreateDatabase()
        {
            string folderPath = Path.Combine(ConfigurationModel.GetConfiguration().RutaProyNuevo, "CDS");
            string databasePath = Path.Combine(folderPath, "cds.db"); ;

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
        }

        private void CreateTables()
        {
            try
            {
                using (SQLiteConnection connection = (SQLiteConnection)OpenConnection())
                {

                    string createTableQuery = "CREATE TABLE IF NOT EXISTS Productos " +
                                              "(id_producto INTEGER PRIMARY KEY, id_siges INTEGER, " +
                                              "producto TEXT NOT NULL, precio REAL NOT NULL)";

                    using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.Connection = connection;
                        _ = command.ExecuteNonQuery();
                    }

                    createTableQuery = "CREATE TABLE IF NOT EXISTS Surtidores " +
                                       "(id_surtidor INTEGER NOT NULL, id_manguera  INTEGER NOT NULL, " +
                                       "PRIMARY KEY (id_surtidor, id_manguera))";

                    using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.Connection = connection;
                        _ = command.ExecuteNonQuery();
                    }

                    createTableQuery = "CREATE TABLE IF NOT EXISTS Tanques (id_tanque INTEGER PRIMARY KEY, " +
                                       "volumen_actual REAL NOT NULL, capacidad_maxima REAL NOT NULL, " +
                                       "actualizado TEXT DEFAULT((strftime('%d-%m-%Y %H:%M:%S', 'now', 'localtime'))));";

                    using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.Connection = connection;
                        _ = command.ExecuteNonQuery();
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

                    using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.Connection = connection;
                        _ = command.ExecuteNonQuery();
                    }

                    createTableQuery = "CREATE TABLE IF NOT EXISTS cierreBandera " +
                                       "(hacerCierre INTEGER NOT NULL, actualizar_tanques INTEGER NOT NULL DEFAULT 0, " +
                                       "cierre_anterior INTEGER NOT NULL DEFAULT 0);\n" +
                                       "INSERT INTO cierreBandera (hacerCierre) " +
                                       "SELECT 0 WHERE NOT EXISTS (SELECT 1 FROM cierreBandera)";

                    using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.Connection = connection;
                        _ = command.ExecuteNonQuery();
                    }

                    createTableQuery = "CREATE TABLE IF NOT EXISTS Cierres " +
                                       "(id INTEGER, id_cierre INTEGER, fecha TEXT DEFAULT((strftime('%d-%m-%Y %H:%M:%S', 'now', 'localtime'))), " +
                                       "monto_contado TEXT, volumen_contado TEXT, " +
                                       "monto_YPFruta TEXT, volumen_YPFruta TEXT, state TEXT, message TEXT, PRIMARY KEY(id AUTOINCREMENT))";

                    using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.Connection = connection;
                        _ = command.ExecuteNonQuery();
                    }

                    createTableQuery = "CREATE TABLE IF NOT EXISTS CierresPorManguera " +
                                       "(id INTEGER NOT NULL, surtidor INTEGER NOT NULL, " +
                                       "manguera INTEGER NOT NULL, monto REAL, " +
                                       "volumen REAL, monto_acumulado REAL, volumen_acumulado REAL)";

                    using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.Connection = connection;
                        _ = command.ExecuteNonQuery();
                    }

                    createTableQuery = "CREATE TABLE IF NOT EXISTS CierresPorProducto " +
                                       "(id INTEGER NOT NULL, producto INTEGER NOT NULL, " +
                                       "monto REAL NOT NULL, volumen REAL NOT NULL)";

                    using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.Connection = connection;
                        _ = command.ExecuteNonQuery();
                    }

                    createTableQuery = "CREATE TABLE IF NOT EXISTS CheckConnection " +
                                      "(idConnection INTEGER PRIMARY KEY, isConnected INTEGER, fecha date DEFAULT((strftime('%d-%m-%Y %H:%M:%S', 'now', 'localtime'))));\n" +
                                      "INSERT INTO CheckConnection (idConnection, isConnected)\n" +
                                      "SELECT 1, 0 \n" +
                                      "WHERE NOT EXISTS (SELECT 1 FROM CheckConnection WHERE idConnection = 1)";

                    using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.Connection = connection;
                        _ = command.ExecuteNonQuery();
                    }

                    createTableQuery = "CREATE TABLE IF NOT EXISTS Descuentos " +
                                       "(external_reference INTEGER, AuthCode INTEGER, CollectorId INTEGER, " +
                                       "CurrencyId TEXT, DateCreated TEXT, Description TEXT, PaymentTypeId TEXT, " +
                                       "StatementDescriptor TEXT, TransactionAmount REAL, payment_method_id TEXT, status TEXT, " +
                                       "Glosa TEXT, Descuento REAL, fecha date DEFAULT((strftime('%d-%m-%Y %H:%M:%S', 'now', 'localtime'))));";

                    using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.Connection = connection;
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteLog($"Error al crear tabla: {ex.Message}", LogType.t_error);
            }
        }
    }
}
