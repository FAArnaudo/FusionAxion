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
    public abstract class RepositoryBase
    {
        private readonly string connectionString;
        private SQLiteConnection connection = null;

        public RepositoryBase()
        {
            // Aseguramos que la ruta sea válida
            string dbPath = Path.Combine(ConfigurationModel.GetConfiguration().RutaProyNuevo, "CDS", "cds.db");

            connectionString = $"Data Source='{dbPath}';Version=3;";
        }

        /// <summary>
        /// Método para abrir la conexión a la base de datos
        /// </summary>
        /// <returns></returns>
        protected SQLiteConnection OpenConnection()
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
                    Log.Instance.WriteLog($"Error de SQLite al abrir la conexión: {sqlEx.Message}", LogType.t_error);
                    throw new Exception("Error al abrir la conexión a la base de datos.", sqlEx);
                }
                catch (UnauthorizedAccessException uaeEx)
                {
                    // Manejo de error de permisos
                    Log.Instance.WriteLog($"Error de permisos al abrir la conexión: {uaeEx.Message}", LogType.t_error);
                    throw new Exception("No tiene permisos para acceder a la base de datos.", uaeEx);
                }
                catch (FileNotFoundException fnfEx)
                {
                    // Manejo de error si el archivo no se encuentra
                    Log.Instance.WriteLog($"Archivo de base de datos no encontrado: {fnfEx.Message}", LogType.t_error);
                    throw new Exception("Archivo de base de datos no encontrado.", fnfEx);
                }
                catch (Exception ex)
                {
                    // Manejo general de otras excepciones
                    Log.Instance.WriteLog($"Error al abrir la conexión: {ex.Message}", LogType.t_error);
                    throw new Exception("No se pudo abrir la conexión a la base de datos.", ex);
                }
            }

            return connection;
        }

        /// <summary>
        /// Método para cerrar la conexión a la base de datos
        /// </summary>
        protected void CloseConnection()
        {
            if (connection != null)
            {
                try
                {
                    // Solo cerramos la conexión si no está ya cerrada
                    if (connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                    }
                }
                catch (SQLiteException sqlEx)
                {
                    // Manejo específico de excepciones de SQLite al cerrar la conexión
                    Log.Instance.WriteLog($"Error de SQLite al cerrar la conexión: {sqlEx.Message}", LogType.t_error);
                    throw new Exception("Error al cerrar la conexión a la base de datos.", sqlEx);
                }
                catch (ObjectDisposedException objEx)
                {
                    // Manejo del caso si la conexión ya fue dispuesta
                    Log.Instance.WriteLog($"Error al intentar cerrar una conexión ya dispuesta: {objEx.Message}", LogType.t_error);
                    throw new Exception("La conexión ya fue dispuesta previamente.", objEx);
                }
                catch (Exception ex)
                {
                    // Manejo general de otras excepciones al cerrar la conexión
                    Log.Instance.WriteLog($"Error al cerrar la conexión: {ex.Message}", LogType.t_error);
                    throw new Exception("No se pudo cerrar la conexión a la base de datos.", ex);
                }
            }
        }
    }
}
