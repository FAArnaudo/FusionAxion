using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion.Repositories
{
    public class ConnectorSQLite : RepositoryBase
    {
        // Un objeto que se utilizará para la sincronización
        private static readonly object lockObjectDB = new object();

        private static ConnectorSQLite instance = null;
        private ConnectorSQLite() { }
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
                    using (SQLiteCommand cmd = new SQLiteCommand(query, OpenConnection()))
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
                    Log.Instance.WriteLog($"Error al ejecutar SELECT: {e.Message}", LogType.t_error);
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
                    using (SQLiteCommand cmd = new SQLiteCommand(query, OpenConnection()))
                    {
                        int rowsAffected = cmd.ExecuteNonQuery(); // Ejecuta el comando y devuelve el número de filas afectadas
                        return rowsAffected;
                    }
                }
                catch (SQLiteException e)
                {
                    Log.Instance.WriteLog($"Error al ejecutar ExecuteNonQuery {query}.\nExcepción: {e.Message}", LogType.t_error);
                    return -1;
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteLog($"Error al ejecutar INSERT/UPDATE/DELETE: {ex.Message}", LogType.t_error);
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
                    using (SQLiteCommand cmd = new SQLiteCommand(query, OpenConnection()))
                    {
                        int result = Convert.ToInt32(cmd.ExecuteScalar());
                        return result == 1;
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteLog($"Error al ejecutar una consulta de estados: {ex.Message}", LogType.t_error);
                    return false;
                }
                finally
                {
                    // Cerrar la conexión al final de la operación
                    CloseConnection();
                }
            }
        }
    }
}
