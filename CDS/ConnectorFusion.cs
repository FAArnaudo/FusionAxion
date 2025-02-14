using FusionClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace CDS
{
    public class ConnectorFusion
    {
        private static readonly CultureInfo culture = CultureInfo.InvariantCulture;
        public ConnectorFusion() { }

        public void ComandoConfiguracionDeLaEstacion(Fusion cFusion)
        {
            Station estacion = Station.Instance;
            List<Surtidor> surtidores = new List<Surtidor>();
            List<Producto> productos = new List<Producto>();
            List<Tanque> tanques = new List<Tanque>();
            ArrayList products = new ArrayList();

            try
            {
                FusionForecourt fusionForecourt = cFusion.GetConfig();

                estacion.NumeroDeProductos = fusionForecourt.GetGradesCount();

                if (cFusion.GetFCRTProducts(products))
                {
                    foreach (FusionProduct fusionProduct in products)
                    {
                        if (fusionProduct != null)
                        {
                            Producto producto = new Producto
                            {
                                ID = fusionProduct.m_iProductNr,
                                Descripcion = fusionProduct.m_iProductId,
                            };

                            GetCodigoSiges(producto);

                            productos.Add(producto);
                        }
                    }
                }

                int pumpCount = 0;
                if (cFusion.GetPumpsCount(ref pumpCount))
                {
                    estacion.NumeroDeSurtidores = pumpCount;
                    pumpCount = 0;
                    foreach (FusionPump pump in fusionForecourt.o_Pump)
                    {
                        if (pump != null)
                        {
                            pumpCount++;
                            Surtidor surtidor = new Surtidor
                            {
                                ID = pumpCount,
                                NumeroDeMangueras = pump.m_iHoses,
                            };

                            foreach (FusionHose fusionHose in pump.o_Hose)
                            {
                                if (fusionHose != null)
                                {
                                    Manguera manguera = new Manguera
                                    {
                                        ID = fusionHose.m_iPhysicalID
                                    };

                                    foreach (Producto producto in productos)
                                    {
                                        if (producto.ID == fusionHose.m_iGradeNr && producto.PrecioUnitario == 0)
                                        {
                                            producto.PrecioUnitario = ConvertDouble(fusionHose.m_strPPU);
                                            producto.IdTanque = Convert.ToInt32(fusionHose.m_strTanks.Substring(1, 1));
                                            manguera.Producto = producto;
                                            break;
                                        }
                                        else if (producto.ID == fusionHose.m_iGradeNr)
                                        {
                                            manguera.Producto = producto;
                                            break;
                                        }
                                    }
                                    surtidor.Mangueras.Add(manguera);
                                }
                            }
                            surtidores.Add(surtidor);
                        }
                    }
                }
                estacion.Surtidores = surtidores;
                estacion.Productos = productos;

                foreach (Producto producto in estacion.Productos)
                {
                    Tanque tanque = new Tanque
                    {
                        Product = producto,
                        ID = producto.IdTanque
                    };

                    tanques.Add(tanque);
                }
                estacion.NumeroDeTanques = tanques.Count;
                estacion.Tanques = tanques;
            }
            catch (NullReferenceException e)
            {
                throw new NullReferenceException($"Error al obtener la configuración de la estación - NullReferenceException. Excepción: {e.Message}");
            }
            catch (Exception e)
            {
                throw new Exception($"Error al obtener la configuración de la estación. Excepción: {e.Message}");
            }
        }

        public bool PumaDiscount(Fusion cFusion, int idSale, ref string descuento)
        {
            Log.Instance.WriteLog($"Verificando descuento Puma. ID: {idSale}", LogType.t_debug);

            try
            {
                bool tieneDescuento = cFusion.GetMPPaymentObject(idSale, ref descuento);

                if (tieneDescuento && !string.IsNullOrEmpty(descuento))
                {
                    Log.Instance.WriteLog($"Descuento {tieneDescuento}. \nID: {idSale}\nDescuento: {descuento}", LogType.t_debug);
                    return true;
                }

                Log.Instance.WriteLog($"Descuento {tieneDescuento}. \nID: {idSale}\nDescuento: {descuento}", LogType.t_debug);
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error al obtener descuentos. Excepcion {e.Message}", LogType.t_error);
            }

            return false;
        }

        public bool AxionDiscount(Fusion cFusion, int idSale, ref string descuento)
        {
            try
            {
                FusionSale fusionSale = new FusionSale();
                _ = cFusion.GetSale(idSale, fusionSale);

                descuento = fusionSale.GetPaymentInfo();
                /*
                // Tiene descuento
                descuento = "AUC=055127143~CL=39444994~DCA=600.00~DCI=200000000000000000001~" +
                "DCP=15.00%~DPN=FIDELIDAD ON~PT=200000000000000000001~" +
                "TEXTD=(1015) - Bienvenido a ON! Disfruta un 15% de descuento!~TICKET=871621";
                */
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error al obtener descuentos. Excepcion {e.Message}", LogType.t_error);
            }

            return descuento.StartsWith("AUC");
        }

        public CierreFusion ComandoCierresDeTurno(Fusion cFusion)
        {
            CierreFusion cierreDeTurno;

            bool flag;
            string type, periodType;
            type = "S"; // S - Shift

            string status, message, errorCode, periodID;
            status = "";
            message = "";
            errorCode = "";
            periodID = "";
            periodType = "";

            flag = cFusion.ShiftClose(type, ref status, ref message, ref errorCode, ref periodID, ref periodType);
            Log.Instance.WriteLog($"Ejecucion SHiftClose: {flag}", LogType.t_debug);

            cierreDeTurno = new CierreFusion
            {
                Estado = status,
                Message = message
            };

            if (flag && status.Equals("OK"))
            {
                Log.Instance.WriteLog($"Estado devuelto del cierre: {status}", LogType.t_debug);

                cierreDeTurno.ID = Convert.ToInt32(periodID);

                Station station = Station.Instance;

                foreach (Surtidor surtidor in station.Surtidores)
                {
                    foreach (Manguera manguera in surtidor.Mangueras)
                    {
                        string totalVolumen = "";
                        string totalMonto = "";
                        if (cFusion.GetTotalizers(surtidor.ID, manguera.ID, ref totalVolumen, ref totalMonto) != 0)
                        {
                            TotalPorManguera totalPorManguera = new TotalPorManguera
                            {
                                NumeroDeManguera = manguera.ID,
                                NumeroDeSurtidor = surtidor.ID,
                                TotalVntasVolumen = ConvertDouble(totalVolumen),
                                TotalVntasSinControlVolumen = ConvertDouble(totalVolumen),
                                TotalVntasMonto = ConvertDouble(totalMonto),
                                TotalVntasSinControlMonto = ConvertDouble(totalMonto)
                            };

                            cierreDeTurno.TotalesPorManguera.Add(totalPorManguera);
                        }
                    }
                }
            }
            else
            {
                Log.Instance.WriteLog($"Type: {type}, state: {status}, Message: {message}, Error code: {errorCode}.", LogType.t_error);
                cierreDeTurno.ErrorCode = errorCode;
            }

            return cierreDeTurno;
        }

        public double ConvertDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Any, culture, out double result) ? result : result;
        }

        private void GetCodigoSiges(Producto producto)
        {
            if (producto.Descripcion.Contains("SUPER"))
            {
                producto.ID_SIGES = 1;
            }
            else if (producto.Descripcion.Equals("MAX_PREMIUM") || producto.Descripcion.Equals("REGULAR") || producto.Descripcion.Equals("QUANTIUM"))
            {
                producto.ID_SIGES = 4;
            }
            else if(producto.Descripcion.Equals("ION_DIESEL"))
            {
                producto.ID_SIGES = 6;
            }
            else if (producto.Descripcion.Equals("GNC"))
            {
                producto.ID_SIGES = 7;
            }
            else if(producto.Descripcion.Equals("PUMA_DIESEL") || producto.Descripcion.Equals("DIESEL"))
            {
                producto.ID_SIGES = 8;
            }
            else
            {
                producto.ID_SIGES = producto.ID;
            }
        }
    }
}
