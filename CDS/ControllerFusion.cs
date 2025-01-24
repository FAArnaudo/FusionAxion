using FusionClass;
using Polly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CDS
{
    public class ControllerFusion : Controller
    {
        private ICommunication communication;
        private Fusion cFusion;
        private static readonly CultureInfo invariantCulture = CultureInfo.InvariantCulture;

        public ControllerFusion(ICommunication communication)
        {
            this.communication = communication;
        }
        private ICommunication GetDiscount()
        {
            return communication;
        }

        private void SetDiscount(ICommunication value)
        {
            communication = value;
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

        public override void ActualizarProductos()
        {
            throw new NotImplementedException();
        }

        public override void ActualizarTanques()
        {
            throw new NotImplementedException();
        }

        public override void ConfigurarEstacion()
        {
            throw new NotImplementedException();
        }

        public override void GrabarCierre()
        {
            throw new NotImplementedException();
        }

        public override void GrabarDespachos()
        {
            throw new NotImplementedException();
        }

        public void CheckDiscount()
        {
            GetDiscount().CheckDiscount();
        }
    }
    public interface ICommunication
    {
        void CheckDiscount();
        Data GetConfiguration();
        int ExecuteNonQuery(string query);
    }

    public class DiscountPuma : ICommunication
    {
        public DiscountPuma() { }

        public void CheckDiscount()
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery(string query)
        {
            return ConnectorSQLite.Instance.ExecuteNonQuery(query);
        }

        public Data GetConfiguration()
        {
            return Configuration.GetConfiguration();
        }
    }

    public class DiscountAxion : ICommunication
    {
        public DiscountAxion() { }

        public void CheckDiscount()
        {
            throw new NotImplementedException();
        }
        public int ExecuteNonQuery(string query)
        {
            return ConnectorSQLite.Instance.ExecuteNonQuery(query);
        }

        public Data GetConfiguration()
        {
            return Configuration.GetConfiguration();
        }
    }
}
