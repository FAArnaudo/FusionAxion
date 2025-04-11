using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
            connectionString = $"Data Source='{ConfigurationModel.GetConfiguration().RutaProyNuevo + "\\CDS\\" + "cds.db"}';Version=3;";
        }

        /// <summary>
        /// Método para abrir la conexión a la base de datos
        /// </summary>
        /// <returns></returns>
        protected SQLiteConnection OpenConnection()
        {
            if (connection == null)
            {
                connection = new SQLiteConnection(connectionString);
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
            if (connection != null && connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
        }
    }
}
