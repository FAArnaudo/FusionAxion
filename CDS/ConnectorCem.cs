using Polly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDS
{
    public class ConnectorCem
    {

        private readonly byte separador = 0x7E;
        private readonly CultureInfo culture = CultureInfo.InvariantCulture;    // Especifica la cultura que utiliza el punto como separador decimal
        public IConnections Connections { get; set; }

        public ConnectorCem(IConnections connections)
        {
            Connections = connections;
        }

        public bool PoleoEnLinea(byte[] command)
        {
            int confirmation = 0;
            byte[] reply;

            try
            {
                if (Connections.GetConfiguration().Modo.Equals(MODO.NORMAL.ToString()))
                {
                    reply = Connections.EnviarComando(command);

                    if (!File.Exists(Environment.CurrentDirectory + "\\Responses\\poleo.txt"))
                    {
                        SaveAnswer(reply, "poleo");
                    }
                }
                else
                {
                    reply = ReadAnswer("poleo");
                }

                return reply[confirmation] == 0x0;
            }
            catch (Exception e)
            {
                throw new Exception($"Error al obtener la conexión con el controlador CEM. Excepción: {e.Message}");
            }
        }

        public Station ComandoConfiguracionDeLaEstacion(byte[] command)
        {
            int confirmacion = 0;
            int surtidores = 1;
            int tanques = 3;
            int productos = 4;

            byte[] reply = Connections.GetConfiguration().Modo.Equals(MODO.TEST.ToString()) ? ReadAnswer("ConfiguracionDeLaEstacion") : Connections.EnviarComando(command);

            Station station;
            try
            {
                if (reply == null || reply[confirmacion] != 0x0)
                {
                    throw new Exception("No se recibió mensaje de confirmación al solicitar la configuración de la estación.");
                }

                if (!File.Exists(Environment.CurrentDirectory + "\\Responses\\ConfiguracionDeLaEstacion.txt"))
                {
                    SaveAnswer(reply, "ConfiguracionDeLaEstacion");
                }

                station = Station.Instance;

                station.NumeroDeSurtidores = reply[surtidores];
                station.NumeroDeTanques = reply[tanques];
                station.NumeroDeProductos = reply[productos];

                int posicion = productos + 1;

                for (int i = 0; i < station.NumeroDeProductos; i++)
                {
                    Producto product = new ProductoCem
                    {
                        ID = Convert.ToInt16(LeerCampoVariable(reply, ref posicion)),
                        PrecioUnitario = ConvertDouble(LeerCampoVariable(reply, ref posicion))
                    };

                    DescartarCampoVariable(reply, ref posicion);

                    switch (product.ID)
                    {
                        case 1:
                            product.Descripcion = "NAFTA SUPER";
                            break;
                        case 2:
                            product.Descripcion = "NAFTA NORMAL";
                            break;
                        case 3:
                            product.Descripcion = "ULTRA DIESEL";
                            break;
                        case 4:
                            product.Descripcion = "NAFTA INFINIA";
                            break;
                        case 5:
                            product.Descripcion = "KEROSENE";
                            break;
                        case 6:
                            product.Descripcion = "INFINIA DIESEL";
                            break;
                        case 7:
                            product.Descripcion = "GNC";
                            break;
                        case 8:
                            product.Descripcion = "DIESEL 500";
                            break;
                        case 9:
                            product.Descripcion = "AZUL-32";
                            break;
                        default:
                            product.Descripcion = "N/Utilizado";
                            break;
                    }

                    station.Productos.Add(product);
                }

                for (int i = 0; i < station.NumeroDeSurtidores; i++)
                {
                    Surtidor pump = new Surtidor
                    {
                        NivelDeSurtidor = reply[posicion],
                        ID = i + 1
                    };

                    posicion++;

                    pump.NumeroDeMangueras = reply[posicion] + 1; // [0 , 1, 2, 3] + 1

                    posicion++;

                    for (int j = 0; j < pump.NumeroDeMangueras; j++)
                    {
                        Manguera hose = new Manguera
                        {
                            ID = j + 1
                        };

                        foreach (Producto product in station.Productos)
                        {
                            if (product.ID == reply[posicion])          //  Recupero el numero de producto
                            {
                                hose.Producto = product;
                                break;
                            }
                        }

                        posicion++;

                        pump.Mangueras.Add(hose);
                    }

                    station.Surtidores.Add(pump);
                }

                foreach (Surtidor surtidor in station.Surtidores)
                {
                    foreach (NivelDePrecio nivelDePrecio in station.NivelesDePrecio)
                    {
                        if (nivelDePrecio.Nivel == surtidor.NivelDeSurtidor)
                        {
                            nivelDePrecio.SurtidoresPorNivelDePrecio.Add(surtidor);
                        }
                    }
                }

                for (int i = 0; i < station.NumeroDeTanques; i++)
                {
                    Tanque tanque = new Tanque
                    {
                        ID = i + 1,
                    };

                    foreach (Producto product in station.Productos)
                    {
                        if (product.ID == reply[posicion])
                        {
                            tanque.Product = product;
                            break;
                        }
                    }

                    posicion++;

                    station.Tanques.Add(tanque);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error al obtener la configuración de la estación. Excepción: {e.Message}");
            }

            return station;
        }

        public List<Tanque> ComandoStockDeTanques(byte[] command)
        {
            int confirmacion = 0;

            List<Tanque> tanques;

            try
            {
                byte[] reply = Connections.GetConfiguration().Modo.Equals(MODO.TEST.ToString()) ? ReadAnswer("StockDeTanques") : Connections.EnviarComando(command);

                if (reply == null || reply[confirmacion] != 0x0)
                {
                    return null;
                }

                if (!File.Exists(Environment.CurrentDirectory + "\\Responses\\StockDeTanques.txt"))
                {
                    SaveAnswer(reply, "StockDeTanques");
                }

                int posicion = confirmacion + 1;

                tanques = Station.Instance.Tanques;

                for (int i = 0; i < tanques.Count; i++)
                {
                    foreach (Tanque tanque in Station.Instance.Tanques)
                    {
                        if (tanque.ID == (i + 1))
                        {
                            tanque.VolumenDeProducto = ConvertDouble(LeerCampoVariable(reply, ref posicion));
                            tanque.VolumenDeAgua = ConvertDouble(LeerCampoVariable(reply, ref posicion));
                            tanque.VolumenVacio = ConvertDouble(LeerCampoVariable(reply, ref posicion));
                            tanque.CapacidadMaxima = tanque.VolumenDeProducto + tanque.VolumenDeAgua + tanque.VolumenVacio;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error al enviar el comando de stock de tanques. Excepción: {e.Message}", LogType.t_error);

                return null;
            }

            return tanques;
        }

        public DespachoCem ComandoInformacionDeDespacho(byte[] command)
        {
            int confirmacion = 0;
            int status = 1;
            int nro_venta = 2;
            int codigo_producto = 3;
            int numeroDeSurtidor = Convert.ToInt16(command[0] & 0x0F);

            DespachoCem despacho = null;

            try
            {
                byte[] reply = Connections.GetConfiguration().Modo.Equals(MODO.TEST.ToString()) ? ReadAnswer("Despacho-" + numeroDeSurtidor) : Connections.EnviarComando(command);

                if (reply == null && reply[confirmacion] != 0x0)
                {
                    return despacho;
                }

                if (!File.Exists(Environment.CurrentDirectory + $"\\Responses\\Despacho-{numeroDeSurtidor}.txt"))
                {
                    SaveAnswer(reply, $"{numeroDeSurtidor}");
                }

                // Proceso ultima venta
                DespachoCem.ESTADO_SURTIDOR statusVenta;

                bool despachando = false;
                bool detenido = false;

                switch (reply[status])
                {
                    case 0x01:
                        statusVenta = DespachoCem.ESTADO_SURTIDOR.DISPONIBLE;
                        break;
                    case 0x02:
                        statusVenta = DespachoCem.ESTADO_SURTIDOR.EN_SOLICITUD;
                        break;
                    case 0x03:
                        statusVenta = DespachoCem.ESTADO_SURTIDOR.DESPACHANDO;
                        despachando = true;
                        break;
                    case 0x04:
                        statusVenta = DespachoCem.ESTADO_SURTIDOR.AUTORIZADO;
                        break;
                    case 0x05:
                        statusVenta = DespachoCem.ESTADO_SURTIDOR.VENTA_FINALIZADA_IMPAGA;
                        break;
                    case 0x08:
                        statusVenta = DespachoCem.ESTADO_SURTIDOR.DEFECTUOSO;
                        break;
                    case 0x09:
                        statusVenta = DespachoCem.ESTADO_SURTIDOR.ANULADO;
                        break;
                    case 0x0A:
                        statusVenta = DespachoCem.ESTADO_SURTIDOR.DETENIDO;
                        detenido = true;
                        break;
                    default:
                        despachando = true;
                        detenido = true;
                        statusVenta = DespachoCem.ESTADO_SURTIDOR.DETENIDO;
                        break;
                }

                int posicion = codigo_producto + 1;

                if (!despachando && !detenido)
                {
                    despacho = new DespachoCem
                    {
                        Status = statusVenta,
                        NroDeVenta = reply[nro_venta],
                        IdProducto = reply[codigo_producto],
                        Monto = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                        Volumen = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                        PPU = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                        VentaFacturada = Convert.ToBoolean(reply[posicion])
                    };
                    posicion++;
                    despacho.IdDespacho = Convert.ToInt32(LeerCampoVariable(reply, ref posicion));
                }
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"\nError al enviar el comando de informacion de surtidores.Excepcion: {e.Message}", LogType.t_error);

                return null;
            }
            return despacho;
        }

        public CierreDeTurnoCem ComandoCierresDeTurno(byte[] command)
        {
            int posicion = 1;

            byte[] reply;

            string messageError = "Inicio";

            switch (command[0])
            {
                case 0x07:
                    reply = Connections.GetConfiguration().Modo.Equals(MODO.TEST.ToString()) ? ReadAnswer("CierreDeTurno") : Connections.EnviarComando(command);
                    break;
                case 0x0B:
                    reply = Connections.GetConfiguration().Modo.Equals(MODO.TEST.ToString()) ? ReadAnswer("CierreDeTurnoAnterior") : Connections.EnviarComando(command);
                    break;
                case 0x08:
                    reply = Connections.GetConfiguration().Modo.Equals(MODO.TEST.ToString()) ? ReadAnswer("TurnoActual") : Connections.EnviarComando(command);
                    break;
                default:
                    reply = Connections.GetConfiguration().Modo.Equals(MODO.TEST.ToString()) ? ReadAnswer("CierreDeTurno") : Connections.EnviarComando(command);
                    break;
            }

            CierreDeTurnoCem turno;
            try
            {
                messageError = "En comprobacion de turno sin ventas";
                if (reply[0] == 0xFF)
                {
                    turno = new CierreDeTurnoCem
                    {
                        Estado = "SIN VENTAS"
                    };

                    return turno;
                }

                messageError = "Creando el turno";
                turno = new CierreDeTurnoCem();

                for (int i = 0; i < CierreDeTurnoCem.MEDIOS_DE_PAGO; i++)
                {
                    messageError = $"Creando el {i} total medio de pago";
                    TotalMedioDePago totalMedioDePago = new TotalMedioDePago()
                    {
                        NumeroDeMedioDePago = i + 1,
                        TotalMonto = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                        TotalVolumen = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                    };

                    messageError = $"Guardando el {i} total medio de pago";
                    turno.TotalesMedioDePago.Add(totalMedioDePago);
                }

                messageError = $"Guardando los impuestos";
                turno.Impuesto1 = Convert.ToInt32(LeerCampoVariable(reply, ref posicion));
                turno.Impuesto2 = Convert.ToInt32(LeerCampoVariable(reply, ref posicion));

                // INICIO DE CONTEO DE LOS PERIODOS
                messageError = "Obteniendo el periodo de precios";
                turno.PeriodoDePrecios = reply[posicion];
                posicion++;

                for (int i = 0; i < turno.PeriodoDePrecios; i++)
                {
                    // INICIO DE CONTEO DE LOS NIVELES
                    List<List<TotalPorProducto>> totalesPorProductoPorNivel = new List<List<TotalPorProducto>>();

                    messageError = "Obteniendo los niveles de precio";
                    turno.NivelesDePrecio = reply[posicion];
                    posicion++;

                    for (int j = 0; j < turno.NivelesDePrecio; j++)
                    {
                        // INICIO DE LOS TOTALES POR PRODUCTO
                        List<TotalPorProducto> totalesPorProducto = new List<TotalPorProducto>();

                        messageError = "Al consultar el numero de productos";
                        for (int k = 0; k < Station.Instance.NumeroDeProductos; k++)
                        {
                            messageError = $"Al crear el {k} totalPorProducto";
                            TotalPorProducto totalPorProducto = new TotalPorProducto
                            {
                                Periodo = i + 1,
                                Nivel = j + 1,
                                TotalMonto = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                                TotalVolumen = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                                PrecioUnitario = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                            };

                            foreach (Producto producto in Station.Instance.Productos)
                            {
                                if (producto.PrecioUnitario == totalPorProducto.PrecioUnitario)
                                {
                                    totalPorProducto.NumeroDeProducto = producto.ID;
                                    break;
                                }
                            }
                            messageError = $"Al agregar el {k} totalPorProducto";
                            totalesPorProducto.Add(totalPorProducto);
                        }

                        messageError = $"Al agregar el {j} totalesPorProducto";
                        totalesPorProductoPorNivel.Add(totalesPorProducto);
                    }

                    messageError = $"Al agregar el {i} totalesPorProductoPorNivel";
                    turno.TotalesPorPeriodoPorNivelPorProducto.Add(totalesPorProductoPorNivel);
                }

                foreach (Surtidor surtidor in Station.Instance.Surtidores)
                {
                    for (int j = 0; j < surtidor.NumeroDeMangueras; j++)
                    {
                        TotalPorManguera totalPorManguera = new TotalPorManguera
                        {
                            NumeroDeSurtidor = surtidor.ID,
                            NumeroDeManguera = j + 1,
                            TotalVntasMonto = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                            TotalVntasVolumen = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                            TotalVntasSinControlMonto = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                            TotalVntasSinControlVolumen = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                            TotalPruebasMonto = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                            TotalPruebasVolumen = ConvertDouble(LeerCampoVariable(reply, ref posicion))
                        };

                        turno.TotalesPorManguera.Add(totalPorManguera);
                    }
                }
                 
                for (int i = 0; i < Station.Instance.NumeroDeTanques; i++)
                {
                    messageError = $"Al crear el {i} ProductoEnTanque";
                    TotalPorTanque totalPorTanque = new TotalPorTanque
                    {
                        NumeroDeTanque = i,
                        Producto = LeerCampoVariable(reply, ref posicion),
                        Agua = LeerCampoVariable(reply, ref posicion),
                        Vacio = LeerCampoVariable(reply, ref posicion),
                        Capacidad = LeerCampoVariable(reply, ref posicion)
                    };

                    messageError = $"Al agregar el {i} ProductoEnTanque";
                    turno.TotalesPorTanque.Add(totalPorTanque);
                }

                for (int i = 0; i < Station.Instance.NumeroDeProductos; i++)
                {
                    messageError = $"Al crear el {i} ProductoEnTanque";
                    ProductoEnTanque productoEnTanque = new ProductoEnTanque
                    {
                        NumeroDeProducto = i,
                        VolumenEnTanques = LeerCampoVariable(reply, ref posicion),
                        AguaEnTanques = LeerCampoVariable(reply, ref posicion),
                        VacioEnTanques = LeerCampoVariable(reply, ref posicion),
                        CapacidadEnTanques = LeerCampoVariable(reply, ref posicion)
                    };

                    messageError = $"Al agregar el {i} ProductoEnTanque";
                    turno.ProductosEnTanque.Add(productoEnTanque);
                }

                turno.Estado = "OK";
            }
            catch (Exception e)
            {
                string error;
                switch (command[0])
                {
                    case 0x07:
                        error = $"Error al pedir intormacion del CierreDeTurno. Excepción: {e.Message}";
                        break;
                    case 0x0B:
                        error = $"Error al pedir intormacion del CierreDeTurnoAnterior. Excepción: {e.Message}";
                        break;
                    case 0x08:
                        error = $"Error al pedir intormacion del TurnoActual. Excepción: {e.Message}";
                        break;
                    default:
                        error = $"Error al pedir intormacion del turno CierreDeTurno. Excepción: {e.Message}";
                        break;
                }

                Log.Instance.WriteLog(error, LogType.t_error);

                string campos = "state";

                string rows = string.Format("'{0}'",
                                           $"ERROR");

                Connections.ExecuteNonQuery(string.Format("INSERT INTO Cierres ({0}) VALUES ({1})", campos, rows));

                throw new Exception(error + messageError);
            }

            return turno;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="respuesta"></param>
        /// <param name="nombreArchivo"></param>
        public void SaveAnswer(byte[] respuesta, string nombreArchivo)
        {
            nombreArchivo = string.Concat(nombreArchivo.Split(Path.GetInvalidFileNameChars())) + ".txt";

            string directorio = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Responses");

            if (!Directory.Exists(directorio))
            {
                _ = Directory.CreateDirectory(directorio);
            }

            string rutaCompleta = Path.Combine(directorio, nombreArchivo);

            if (!File.Exists(rutaCompleta))
            {
                using (StreamWriter sw = File.AppendText(rutaCompleta))
                {
                    int cont = 0;
                    for (int iteraciones = 0; iteraciones < respuesta.Length; iteraciones++)
                    {
                        sw.WriteLine(respuesta[iteraciones].ToString("X2")); // Escribe en formato hexadecimal

                        if (iteraciones > 0)
                        {
                            if (respuesta[iteraciones] == 0 && respuesta[iteraciones - 1] == 0 && cont < 6)
                            {
                                cont++;
                            }
                            else if (respuesta[iteraciones] == 0 && cont >= 6)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        /*
         * Se utiliza para testear las respuestas reales del Cem-44
         * se lee un .txt que contiene las respuestas y las guarda en un byte,
         * para simular la respuesta.
         */
        public byte[] ReadAnswer(string nombreArchivo)
        {
            // Obtener la ruta del directorio donde se ejecuta el programa
            string directorioEjecucion = AppDomain.CurrentDomain.BaseDirectory;

            // Combinar la ruta del directorio con el nombre del archivo
            string rutaArchivo = Path.Combine(directorioEjecucion, "Responses", nombreArchivo + ".txt");

            // Verificar si el archivo existe
            if (!File.Exists(rutaArchivo))
            {
                throw new FileNotFoundException($"El archivo '{rutaArchivo}' no existe.");
            }

            // Leer todas las líneas del archivo
            string[] lines = File.ReadAllLines(rutaArchivo);

            // Lista para almacenar los bytes leídos
            List<byte> byteList = new List<byte>();

            // Procesar cada línea del archivo
            foreach (string line in lines)
            {
                // Dividir la línea en valores numéricos individuales
                string[] numericValues = line.Split(',');

                // Convertir cada valor numérico en un byte y agregarlo a la lista
                foreach (string value in numericValues)
                {
                    if (byte.TryParse(value.Trim(), out byte parsedValue))
                    {
                        byteList.Add(parsedValue);
                    }
                    else
                    {
                        throw new FormatException($"El valor '{value}' no es un byte válido.");
                    }
                }
            }

            // Convertir la lista a un arreglo de bytes y retornarlo
            return byteList.ToArray();
        }

        /// <summary>
        /// Metodo para leer los campos variables, por ejemplo precios o cantidades.
        /// El metodo para frenar la iteracion, es un valor conocido, proporcionado por el fabricante
        /// denominado como "separador".
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pos"></param>
        public string LeerCampoVariable(byte[] data, ref int pos)
        {
            string ret = "";
            ret += Encoding.ASCII.GetString(new byte[] { data[pos] });
            int i = pos + 1;
            while (data[i] != separador)
            {
                ret += Encoding.ASCII.GetString(new byte[] { data[i] });
                i++;
            }
            i++;
            pos = i;
            return ret;
        }

        /// <summary>
        /// Metodo para saltearse los valores que no son utilizados en la respuesta del CEM.
        /// Al finalizar el proceso del metodo, el valor de la posicion queda seteada para
        /// el siguiente dato a procesar.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pos"></param>
        public void DescartarCampoVariable(byte[] data, ref int pos)
        {
            while (data[pos] != separador)
            {
                pos++;
            }
            pos++;
        }

        public double ConvertDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Any, culture, out double result) ? result : result;
        }
    }

    public interface IConnections
    {
        byte[] EnviarComando(byte[] comando);

        Data GetConfiguration();

        int ExecuteNonQuery(string query);
    }

    public class CemCommunication : IConnections
    {

        private readonly string pipeName = "CEM44POSPIPE";
        private string ipController;
        private string protocol;
        public CemCommunication()
        {
            ReloadData();
        }

        public string IpController { get => ipController; set => ipController = value; }

        public string Protocol { get => protocol; set => protocol = value; }

        public byte[] EnviarComando(byte[] comando)
        {
            byte[] buffer = null;
            NamedPipeClientStream pipeClient = null;

            try
            {
                int retries = 1;

                // Política de reintentos
                PolicyResult policyResult = Policy.Handle<Exception>()
                    .WaitAndRetry(retryCount: 4,
                                  sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)),
                                  onRetry: (exception, TimeSpan, conttext) =>
                                  {
                                      // Cerrar el pipe en caso de fallo
                                      if (pipeClient != null)
                                      {
                                          pipeClient.Dispose();
                                          pipeClient = null; // Limpiar el pipe para la nueva conexión
                                      }
                                      Log.Instance.WriteLog($"\n\t  Excepción: {exception.Message.Trim()} Intento: {retries}", LogType.t_error);
                                      retries++;
                                  }).ExecuteAndCapture(() =>
                                  {
                                      // Crear el pipeClient si está cerrado
                                      if (pipeClient == null)
                                      {
                                          pipeClient = new NamedPipeClientStream(ipController, pipeName);
                                      }

                                      // Conectar con tiempo de espera
                                      pipeClient.Connect(5000);

                                      // Enviar el comando
                                      pipeClient.Write(comando, 0, comando.Length);

                                      // Leer respuesta
                                      buffer = new byte[pipeClient.OutBufferSize];
                                      _ = pipeClient.Read(buffer, 0, buffer.Length);
                                  });
                // Verificación de resultado de conexión
                if (policyResult.Outcome != 0)
                {
                    _ = ConnectorSQLite.Instance.ExecuteNonQuery($"UPDATE CheckConnection SET isConnected = 0, fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}' WHERE idConnection = 1");

                    Log.Instance.WriteLog($"  Fin de intentos...\n", LogType.t_error);
                    ReloadData();
                }
                else
                {
                    _ = ConnectorSQLite.Instance.ExecuteNonQuery($"UPDATE CheckConnection SET isConnected = 1, fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}' WHERE idConnection = 1");
                }
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error al enviar comando. Excepcón: {e.Message}", LogType.t_error);
            }
            finally
            {
                // Asegurarse de cerrar el pipe al final
                if (pipeClient != null)
                {
                    pipeClient.Dispose();
                }
            }

            return buffer;
        }

        public void ReloadData()
        {
            ipController = Configuration.GetConfiguration().IP;
            protocol = Configuration.GetConfiguration().Protocol;
        }

        public Data GetConfiguration()
        {
            return Configuration.GetConfiguration();
        }

        public int ExecuteNonQuery(string query)
        {
            return ConnectorSQLite.Instance.ExecuteNonQuery(query);
        }
    }
}
