using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion.Repositories
{
    public class DataBase : RepositoryBase
    {
        public DataBase() { }
        public void CreateDataBase()
        {
            string folderPath = ConfigurationModel.GetConfiguration().RutaProyNuevo + "\\CDS\\";
            string databasePath = folderPath + "cds.db";

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
        }

        private void CreateTables()
        {
            try
            {
                using (SQLiteConnection connection = OpenConnection())
                {
                    string createTableQuery = "CREATE TABLE IF NOT EXISTS Surtidores " +
                                              "(IdSurtidor INTEGER, Manguera  INTEGER, Producto  INTEGER, " +
                                              "Precio REAL, DescProd TEXT)";

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

                    createTableQuery = "CREATE TABLE IF NOT EXISTS Productos " +
                                       "(id_producto INTEGER PRIMARY KEY, id_siges INTEGER, " +
                                       "numero_despacho INTEGER, producto TEXT NOT NULL, precio REAL NOT NULL)";

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
            finally
            {
                // Cerrar la conexión al final de la operación
                CloseConnection();
            }
        }
    }
}
