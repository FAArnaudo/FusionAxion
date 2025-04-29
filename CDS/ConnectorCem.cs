using Polly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
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
                throw new Exception($"Error al obtener la conexión con el controlador CEM. Excepción: {e.Message}.\n");
            }
        }

        public Station ComandoConfiguracionDeLaEstacion(byte[] command)
        {
            int confirmacion = 0;
            int surtidores = 1;
            int tanques = 3;
            int productos = 4;

            Station.Instance.GeneralMessage = "obteniendo la configuracion de la estacion";
            byte[] reply = Connections.GetConfiguration().Modo.Equals(MODO.TEST.ToString()) ? ReadAnswer("ConfiguracionDeLaEstacion") : Connections.EnviarComando(command);
            Station.Instance.GeneralMessage = "";

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
                List<Producto> productosTemp = new List<Producto>();
                for (int i = 0; i < station.NumeroDeProductos; i++)
                {
                    Producto product = new ProductoCem
                    {
                        ID = Convert.ToInt16(LeerCampoVariable(reply, ref posicion)),
                        PrecioUnitario = ConvertDouble(LeerCampoVariable(reply, ref posicion))
                    };

                    product.ID_SIGES = product.ID;

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
                    productosTemp.Add(product);
                }
                station.Productos = productosTemp;

                List<Surtidor> surtidoresTemp = new List<Surtidor>();
                for (int i = 0; i < station.NumeroDeSurtidores; i++)
                {
                    Surtidor surtidor = new Surtidor
                    {
                        NivelDeSurtidor = reply[posicion],
                        ID = i + 1
                    };

                    posicion++;

                    surtidor.NumeroDeMangueras = reply[posicion] + 1; // [0 , 1, 2, 3] + 1

                    posicion++;

                    for (int j = 0; j < surtidor.NumeroDeMangueras; j++)
                    {
                        Manguera manguera = new Manguera
                        {
                            ID = j + 1
                        };

                        foreach (Producto product in station.Productos)
                        {
                            if (product.ID == reply[posicion])          //  Recupero el numero de producto
                            {
                                manguera.Producto = product;
                                break;
                            }
                        }

                        posicion++;

                        surtidor.Mangueras.Add(manguera);
                    }
                    surtidoresTemp.Add(surtidor);
                }
                station.Surtidores = surtidoresTemp;

                List<NivelDePrecio> nivelesDePrecioTemp = new List<NivelDePrecio>();
                for (int i = 0; i < Station.nivelesDePrecio; i++)
                {
                    NivelDePrecio nivelDePrecio = new NivelDePrecio
                    {
                        Nivel = i
                    };
                    nivelesDePrecioTemp.Add(nivelDePrecio);
                }

                foreach (Surtidor surtidor in surtidoresTemp)
                {
                    foreach (NivelDePrecio nivelDePrecio in nivelesDePrecioTemp)
                    {
                        if (nivelDePrecio.Nivel == surtidor.NivelDeSurtidor)
                        {
                            nivelDePrecio.SurtidoresPorNivelDePrecio.Add(surtidor);
                        }
                    }
                }
                station.NivelesDePrecio = nivelesDePrecioTemp;

                List<Tanque> tanquesTemp = new List<Tanque>();
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

                    tanquesTemp.Add(tanque);
                }
                station.Tanques = tanquesTemp;
            }
            catch (Exception e)
            {
                throw new Exception($"Error al obtener la configuración de la estación. Excepción: {e.Message}\n");
            }

            return station;
        }

        public List<Tanque> ComandoStockDeTanques(byte[] command)
        {
            int confirmacion = 0;

            List<Tanque> tanques;

            try
            {
                Station.Instance.GeneralMessage = "Obtenioendo stock de tanques";
                byte[] reply = Connections.GetConfiguration().Modo.Equals(MODO.TEST.ToString()) ? ReadAnswer("StockDeTanques") : Connections.EnviarComando(command);
                Station.Instance.GeneralMessage = "";

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
                            Log.Instance.WriteLog($"Tanque ID: {tanque.ID}", LogType.t_debug);
                            tanque.VolumenDeProducto = ConvertDouble(LeerCampoVariable(reply, ref posicion));
                            Log.Instance.WriteLog($"VolumenDeProducto: {tanque.VolumenDeProducto}", LogType.t_debug);
                            tanque.VolumenDeAgua = ConvertDouble(LeerCampoVariable(reply, ref posicion));
                            Log.Instance.WriteLog($"VolumenDeAgua: {tanque.VolumenDeAgua}", LogType.t_debug);
                            tanque.VolumenVacio = ConvertDouble(LeerCampoVariable(reply, ref posicion));
                            Log.Instance.WriteLog($"VolumenVacio: {tanque.VolumenVacio}\n", LogType.t_debug);
                            tanque.CapacidadMaxima = tanque.VolumenDeProducto + tanque.VolumenDeAgua + tanque.VolumenVacio;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error al enviar el comando de stock de tanques. Excepción: {e.Message}.\n", LogType.t_error);

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
                    SaveAnswer(reply, $"Despacho-{numeroDeSurtidor}");
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

                SavePumpState(numeroDeSurtidor, statusVenta.ToString());

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
                Log.Instance.WriteLog($"Error al enviar el comando de informacion de surtidores. Excepcion: {e.Message}.\n", LogType.t_error);
                return null;
            }
            return despacho;
        }

        public CierreCem ComandoCierresDeTurno(byte[] command)
        {
            int posicion = 1;

            byte[] reply;
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

            CierreCem turno;

            try
            {
                if (reply[0] == 0xFF)
                {
                    turno = new CierreCem
                    {
                        Estado = "SIN VENTAS"
                    };

                    for (int i = 0; i < CierreCem.MEDIOS_DE_PAGO; i++)
                    {
                        TotalMedioDePago totalMedioDePago = new TotalMedioDePago()
                        {
                            NumeroDeMedioDePago = i + 1,
                            TotalMonto = 0,
                            TotalVolumen = 0,
                        };

                        turno.TotalesMedioDePago.Add(totalMedioDePago);
                    }

                    turno.Impuesto1 = 0;
                    turno.Impuesto2 = 0;

                    // INICIO DE CONTEO DE LOS PERIODOS
                    turno.PeriodoDePrecios = 1;
                    posicion++;

                    for (int i = 0; i < turno.PeriodoDePrecios; i++)
                    {
                        // INICIO DE CONTEO DE LOS NIVELES
                        List<List<TotalPorProducto>> totalesPorProductoPorNivel = new List<List<TotalPorProducto>>();

                        turno.NivelesDePrecio = 1;
                        posicion++;

                        for (int j = 0; j < turno.NivelesDePrecio; j++)
                        {
                            // INICIO DE LOS TOTALES POR PRODUCTO
                            List<TotalPorProducto> totalesPorProducto = new List<TotalPorProducto>();

                            for (int k = 0; k < Station.Instance.NumeroDeProductos; k++)
                            {
                                TotalPorProducto totalPorProducto = new TotalPorProducto
                                {
                                    Periodo = i + 1,
                                    Nivel = j + 1,
                                    NumeroDeProducto = k + 1,
                                    TotalMonto = 0,
                                    TotalVolumen = 0,
                                    PrecioUnitario = 0,
                                };
                                totalesPorProducto.Add(totalPorProducto);
                            }
                            totalesPorProductoPorNivel.Add(totalesPorProducto);
                        }
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
                                TotalVntasMonto = 0,
                                TotalVntasVolumen = 0,
                                TotalVntasSinControlMonto = 0,
                                TotalVntasSinControlVolumen = 0,
                                TotalPruebasMonto = 0,
                                TotalPruebasVolumen = 0
                            };
                            turno.TotalesPorManguera.Add(totalPorManguera);
                        }
                    }

                    for (int i = 0; i < Station.Instance.NumeroDeTanques; i++)
                    {
                        TotalPorTanque totalPorTanque = new TotalPorTanque
                        {
                            NumeroDeTanque = i,
                            Producto = 0,
                            Agua = 0,
                            Vacio = 0,
                            Capacidad = 0
                        };

                        turno.TotalesPorTanque.Add(totalPorTanque);
                    }

                    for (int i = 0; i < Station.Instance.NumeroDeProductos; i++)
                    {
                        ProductoEnTanque productoEnTanque = new ProductoEnTanque
                        {
                            NumeroDeProducto = i,
                            VolumenEnTanques = 0,
                            AguaEnTanques = 0,
                            VacioEnTanques = 0,
                            CapacidadEnTanques = 0
                        };

                        turno.ProductosEnTanque.Add(productoEnTanque);
                    }

                    return turno;
                }

                turno = new CierreCem();

                for (int i = 0; i < CierreCem.MEDIOS_DE_PAGO; i++)
                {
                    TotalMedioDePago totalMedioDePago = new TotalMedioDePago()
                    {
                        NumeroDeMedioDePago = i + 1,
                        TotalMonto = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                        TotalVolumen = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                    };
                    turno.TotalesMedioDePago.Add(totalMedioDePago);
                }

                turno.Impuesto1 = Convert.ToInt32(LeerCampoVariable(reply, ref posicion));
                turno.Impuesto2 = Convert.ToInt32(LeerCampoVariable(reply, ref posicion));

                // INICIO DE CONTEO DE LOS PERIODOS
                turno.PeriodoDePrecios = reply[posicion];
                posicion++;

                for (int i = 0; i < turno.PeriodoDePrecios; i++)
                {
                    // INICIO DE CONTEO DE LOS NIVELES
                    List<List<TotalPorProducto>> totalesPorProductoPorNivel = new List<List<TotalPorProducto>>();

                    turno.NivelesDePrecio = reply[posicion];
                    posicion++;

                    for (int j = 0; j < turno.NivelesDePrecio; j++)
                    {
                        // INICIO DE LOS TOTALES POR PRODUCTO
                        List<TotalPorProducto> totalesPorProducto = new List<TotalPorProducto>();

                        for (int k = 0; k < Station.Instance.NumeroDeProductos; k++)
                        {
                            TotalPorProducto totalPorProducto = new TotalPorProducto
                            {
                                Periodo = i + 1,
                                Nivel = j + 1,
                                NumeroDeProducto = k + 1,
                                TotalMonto = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                                TotalVolumen = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                                PrecioUnitario = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                            };
                            totalesPorProducto.Add(totalPorProducto);
                        }

                        totalesPorProductoPorNivel.Add(totalesPorProducto);
                    }

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
                    TotalPorTanque totalPorTanque = new TotalPorTanque
                    {
                        NumeroDeTanque = i,
                        Producto = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                        Agua = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                        Vacio = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                        Capacidad = ConvertDouble(LeerCampoVariable(reply, ref posicion))
                    };
                    turno.TotalesPorTanque.Add(totalPorTanque);
                }

                for (int i = 0; i < Station.Instance.NumeroDeProductos; i++)
                {
                    ProductoEnTanque productoEnTanque = new ProductoEnTanque
                    {
                        NumeroDeProducto = i,
                        VolumenEnTanques = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                        AguaEnTanques = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                        VacioEnTanques = ConvertDouble(LeerCampoVariable(reply, ref posicion)),
                        CapacidadEnTanques = ConvertDouble(LeerCampoVariable(reply, ref posicion))
                    };

                    turno.ProductosEnTanque.Add(productoEnTanque);
                }

                turno.Estado = "OK";
            }
            catch (Exception e)
            {
                string error;
                string name;
                switch (command[0])
                {
                    case 0x07:
                        error = $"Error al pedir intormacion del CierreDeTurno. Excepción: {e.Message}.\n";
                        name = "CierreDeTurno";
                        break;
                    case 0x0B:
                        error = $"Error al pedir intormacion del CierreDeTurnoAnterior. Excepción: {e.Message}.\n";
                        name = "CierreDeTurnoAnterior";
                        break;
                    case 0x08:
                        error = $"Error al pedir intormacion del TurnoActual. Excepción: {e.Message}.\n";
                        name = "TurnoActual";
                        break;
                    default:
                        error = $"Error al pedir intormacion del turno CierreDeTurno. Excepción: {e.Message}.\n";
                        name = "CierreDeTurno";
                        break;
                }

                SaveAnswer(reply, name + new Random().Next(100));

                Log.Instance.WriteLog(error, LogType.t_error);

                string campos = "state";
                string rows = "ERROR";

                Connections.ExecuteNonQuery(string.Format("INSERT INTO Cierres ({0}) VALUES ({1})", campos, rows));

                throw new Exception(error);
            }

            return turno;
        }

        public void ComandoAutorizacion(byte[] command)
        {
            throw new NotImplementedException();
        }

        public void ComandoEnvioPreset(byte[] command)
        {
            throw new NotImplementedException();
        }

        public void ComandoEmergenciaIndividual(byte[] command)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="respuesta"></param>
        /// <param name="nombreArchivo"></param>
        private void SaveAnswer(byte[] respuesta, string nombreArchivo)
        {
            nombreArchivo = string.Concat(nombreArchivo.Split(Path.GetInvalidFileNameChars())) + ".txt";

            string directorio = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Responses");

            if (!Directory.Exists(directorio))
            {
                _ = Directory.CreateDirectory(directorio);
            }

            string rutaCompleta = Path.Combine(directorio, nombreArchivo);

            try
            {
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
                                if (respuesta[iteraciones] == 0 && cont < 10)
                                {
                                    cont++;
                                }
                                else if (respuesta[iteraciones] != 0 && cont < 10)
                                {
                                    cont = 0;
                                }
                                else if (respuesta[iteraciones] == 0 && cont >= 10)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
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
                        throw new FormatException($"El valor '{value}' no es un byte válido.\n");
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
        private string LeerCampoVariable(byte[] data, ref int pos)
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
        private void DescartarCampoVariable(byte[] data, ref int pos)
        {
            while (data[pos] != separador)
            {
                pos++;
            }
            pos++;
        }

        private double ConvertDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Any, culture, out double result) ? result : result;
        }

        private void SavePumpState(int numero, string contenido)
        {
            try
            {
                // Ruta a la carpeta "Responses" en la base del programa
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");

                // Crear directorio si no existe
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                // Nombre del archivo del día actual
                string fileName = $"surtidor-{numero}-{DateTime.Now:dd-MM-yyyy}.txt";
                string filePath = Path.Combine(path, fileName);

                // Borra el log del día anterior
                string deleteFileName = $"surtidor-{numero}-{DateTime.Now.AddDays(-1):dd-MM-yyyy}.txt";
                string deleteFilePath = Path.Combine(path, deleteFileName);

                if (File.Exists(deleteFilePath))
                {
                    File.Delete(deleteFilePath);
                }

                // Escribe en el archivo
                using (StreamWriter outputFile = new StreamWriter(filePath, true))
                {
                    outputFile.WriteLine($"{DateTime.Now:HH:mm:ss}  Surtidor{numero}    ESTADO: {contenido}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Instance.WriteLog($"Error de permisos. Excepción: {ex.Message}\n", LogType.t_error);
            }
            catch (IOException ex)
            {
                Log.Instance.WriteLog($"Error de entrada/salida. Excepción: {ex.Message}\n", LogType.t_error);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteLog($"Error inesperado. Excepción: {ex.Message}\n", LogType.t_error);
            }
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

        public CemCommunication()
        {
            ReloadData();
        }

        public string IpController { get; set; }

        public string Protocol { get; set; }

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
                                  onRetry: (exception, _, context) =>
                                  {
                                      if (pipeClient != null)
                                      {
                                          pipeClient.Dispose();
                                          pipeClient = null;
                                      }
                                      Log.Instance.WriteLog($"Excepción: {exception.Message.Trim()} Intento: {retries}, Thread: {Thread.CurrentThread.ManagedThreadId}.\n", LogType.t_error);
                                      retries++;
                                  })
                    .ExecuteAndCapture(() =>
                    {
                        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10))) // tiempo máximo total
                        {
                            var task = Task.Run(() =>
                            {
                                if (pipeClient == null)
                                {
                                    pipeClient = new NamedPipeClientStream(IpController, pipeName);
                                }

                                // Intentar conectar con timeout de 5s
                                pipeClient.Connect(5000);

                                // Enviar comando
                                pipeClient.Write(comando, 0, comando.Length);

                                // Leer respuesta
                                buffer = new byte[pipeClient.OutBufferSize];
                                int bytesRead = pipeClient.Read(buffer, 0, buffer.Length);

                                if (bytesRead == 0)
                                {
                                    throw new IOException("No se recibió respuesta del servidor.");
                                }

                            }, cts.Token);

                            task.Wait(cts.Token); // lanza OperationCanceledException si se pasa el tiempo
                        }
                    });
                
                if (policyResult.Outcome != OutcomeType.Successful)
                {
                    _ = ConnectorSQLite.Instance.ExecuteNonQuery(
                        $"UPDATE CheckConnection SET isConnected = 0, fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}' WHERE idConnection = 1");
                    Log.Instance.WriteLog($"Fin de intentos...\n", LogType.t_error);
                    ReloadData();
                }
                else
                {
                    _ = ConnectorSQLite.Instance.ExecuteNonQuery(
                        $"UPDATE CheckConnection SET isConnected = 1, fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}' WHERE idConnection = 1");
                }
            }
            catch (OperationCanceledException)
            {
                Log.Instance.WriteLog("Timeout general al enviar o recibir datos por el pipe.\n", LogType.t_error);
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error al enviar comando. Excepción: {e.Message}.\n", LogType.t_error);
            }
            finally
            {
                if (pipeClient != null)
                {
                    pipeClient.Dispose();
                }
            }

            return buffer;
        }

        public void ReloadData()
        {
            IpController = Configuration.GetConfiguration().IP;
            Protocol = Configuration.GetConfiguration().Protocol;
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
