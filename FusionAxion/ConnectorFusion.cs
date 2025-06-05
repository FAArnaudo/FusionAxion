using FusionClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace FusionAxion
{
    public class ConnectorFusion
    {
        private static readonly CultureInfo culture = CultureInfo.InvariantCulture;

        public ConnectorFusion() { }

        public void ComandoConfiguracionDeLaEstacion(Fusion Fusion)
        {
            Station estacion = Station.Instance;
            List<Producto> productos = new List<Producto>();
            List<Surtidor> surtidores = new List<Surtidor>();
            List<Tanque> tanques = new List<Tanque>();

            HashSet<int> tanquesNr = new HashSet<int>();

            try
            {
                FusionForecourt fusionForecourt = Fusion.GetConfig();

                ProductosFill(Fusion, fusionForecourt, productos);
                SurtidoresFill(Fusion, fusionForecourt, surtidores, productos, tanquesNr);
                TanquesFill(Fusion, tanques, productos, tanquesNr);

                estacion.Surtidores = surtidores;
                estacion.Tanques = tanques;
                estacion.Productos = productos;
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

        private void ProductosFill(Fusion Fusion, FusionForecourt fusionForecourt, List<Producto> productos)
        {
            Station estacion = Station.Instance;
            ArrayList products = new ArrayList();

            estacion.NumeroDeProductos = fusionForecourt.GetGradesCount();
            Log.Instance.WriteLog($"Numero de productos: {fusionForecourt.GetGradesCount()}\n", LogType.t_info);

            if (Fusion.GetFCRTProducts(products))
            {
                foreach (FusionProduct fusionProduct in products)
                {
                    if (fusionProduct != null)
                    {
                        Log.Instance.WriteLog($"PRODUCTO {fusionProduct.m_iProductNr}: {fusionProduct.m_iProductId}.", LogType.t_info);

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
        }

        private void SurtidoresFill(Fusion Fusion, FusionForecourt fusionForecourt, List<Surtidor> surtidores, List<Producto> productos, HashSet<int> tanquesNr)
        {
            Station estacion = Station.Instance;

            int pumpCount = 0;
            if (Fusion.GetPumpsCount(ref pumpCount))
            {
                Log.Instance.WriteLog($"Numero de surtidores: {pumpCount}\n", LogType.t_info);
                estacion.NumeroDeSurtidores = pumpCount;
                pumpCount = 0;
                foreach (FusionPump pump in fusionForecourt.o_Pump)
                {
                    if (pump != null)
                    {
                        pumpCount++;
                        Log.Instance.WriteLog($"SURTIDOR {pumpCount}: numero de mangueras: {pump.m_iHoses}", LogType.t_info);
                        Surtidor surtidor = new Surtidor
                        {
                            ID = pumpCount,
                            NumeroDeMangueras = pump.m_iHoses,
                        };

                        ManguerasFill(pump, surtidor, productos, tanquesNr);
                        surtidores.Add(surtidor);
                    }
                }
            }
        }

        private void ManguerasFill(FusionPump pump, Surtidor surtidor, List<Producto> productos, HashSet<int> tanquesNr)
        {
            foreach (FusionHose fusionHose in pump.o_Hose)
            {
                if (fusionHose != null)
                {
                    Log.Instance.WriteLog($"\tMANGUERA {fusionHose.m_iPhysicalID + 1}: Producto: {fusionHose.m_iGradeNr}, PPU: {ConvertDouble(fusionHose.m_strPPU)}, Tanque: {fusionHose.m_strTanks}", LogType.t_info);
                    Manguera manguera = new Manguera
                    {
                        ID = fusionHose.m_iPhysicalID + 1
                    };

                    if (!string.IsNullOrEmpty(fusionHose.m_strTanks))
                    {
                        _ = tanquesNr.Add(Convert.ToInt32(fusionHose.m_strTanks.Substring(1, 1)));
                    }

                    foreach (Producto producto in productos)
                    {
                        if (producto.ID == fusionHose.m_iGradeNr)
                        {
                            if (producto.PrecioUnitario == 0)
                            {
                                producto.PrecioUnitario = ConvertDouble(fusionHose.m_strPPU);
                            }
                            manguera.Producto = producto;
                            break;
                        }
                    }
                    surtidor.Mangueras.Add(manguera);
                }
            }
        }

        private void TanquesFill(Fusion Fusion, List<Tanque> tanques, List<Producto> productos, HashSet<int> tanquesNr)
        {
            Station estacion = Station.Instance;
            FusionTankInfo fusionTank = new FusionTankInfo();


            Log.Instance.WriteLog($"Numero de tanques: {tanquesNr.Count}\n", LogType.t_info);

            foreach (int numeroTanque in tanquesNr)
            {
                _ = Fusion.GetTankInfo(numeroTanque, fusionTank);

                Log.Instance.WriteLog($"TANQUE {numeroTanque}: Capacidad {fusionTank.TankVolumeCapacity()} - Producto {fusionTank.GetProductNr()}", LogType.t_info);

                Tanque tanque = new Tanque
                {
                    ID = numeroTanque,
                    CapacidadMaxima = Convert.ToDouble(fusionTank.TankVolumeCapacity()),
                    VolumenDeProducto = Convert.ToDouble(fusionTank.GetFuelVolume()),
                    Product = productos.Find(p => p.ID == fusionTank.GetProductNr())
                };

                tanques.Add(tanque);
            }
            /*
            foreach (Producto producto in productos)
            {
                if (!producto.Descripcion.Equals("GNC") && !producto.Descripcion.Equals("GLP"))
                {
                    Log.Instance.WriteLog($"TANQUE {producto.IdTanque}: Producto: {producto.Descripcion}", LogType.t_info);
                    Tanque tanque = new Tanque
                    {
                        Product = producto,
                        ID = producto.IdTanque
                    };

                    tanques.Add(tanque);
                }
            }*/

            estacion.NumeroDeTanques = tanques.Count;
        }

        public bool AxionDiscount(Fusion cFusion, int idSale, ref string descuento)
        {
            try
            {
                FusionSale fusionSale = new FusionSale();
                _ = cFusion.GetSale(idSale, fusionSale);

                descuento = fusionSale.GetPaymentInfo();

                if (!string.IsNullOrWhiteSpace(descuento))
                {
                    Log.Instance.WriteLog($"Descuento obtenido:\n{descuento}\n", LogType.t_debug);
                }
                /*
                // Tiene descuento
                descuento = "AUC=055127143~CL=39444994~DCA=600.00~DCI=200000000000000000001~" +
                "DCP=15.00%~DPN=FIDELIDAD ON~PT=200000000000000000001~" +
                "TEXTD=(1015) - Bienvenido a ON! Disfruta un 15% de descuento!~TICKET=871621";
                */
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error al obtener descuentos. Excepcion {e.Message}\n", LogType.t_error);
            }

            return descuento.StartsWith("AUC");
        }

        public CierreDeTurno ComandoCierresDeTurno(Fusion cFusion)
        {
            CierreDeTurno cierreDeTurno;

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
            Log.Instance.WriteLog($"Ejecucion de ShiftClose: {status}", LogType.t_debug);

            cierreDeTurno = new CierreDeTurno
            {
                Estado = status,
                Message = message,
                ID = int.TryParse(periodID, out int result) ? result : 0
            };

            if (flag && status.Equals("OK"))
            {
                CierreTurnoFill(cFusion, cierreDeTurno);
            }
            else
            {
                Log.Instance.WriteLog($"Type: {type}, state: {status}, Message: {message}, Error code: {errorCode}.", LogType.t_error);
                cierreDeTurno.ErrorCode = errorCode;
            }

            return cierreDeTurno;
        }

        private void CierreTurnoFill(Fusion cFusion, CierreDeTurno cierreDeTurno)
        {
            foreach (Surtidor surtidor in Station.Instance.Surtidores)
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

        private void GetCodigoSiges(Producto producto)
        {
            if (producto.Descripcion.Contains("SUPER") || producto.Descripcion.Equals("SUPER_BIO") ||
                producto.Descripcion.Contains("GASOHOL REGULAR"))
            {
                producto.ID_SIGES = 1;      // Nafta super
            }
            else if (producto.Descripcion.Equals("REGULAR") || producto.Descripcion.Equals("QUANTIUM") ||
                     producto.Descripcion.Equals("GASOHOL PREMIUM"))
            {
                producto.ID_SIGES = 4;      //Nafta Premium
            }
            else if (producto.Descripcion.Equals("ION_DIESEL") || producto.Descripcion.Equals("QUANTIUM DIESEL"))
            {
                producto.ID_SIGES = 6;      // Diesel Premium
            }
            else if (producto.Descripcion.Equals("GNC") || producto.Descripcion.Equals("GLP"))
            {
                producto.ID_SIGES = 7;      // GNC
            }
            else if (producto.Descripcion.Equals("DIESEL") || producto.Descripcion.Equals("DIESEL_BIO") ||
                     producto.Descripcion.Equals("DIESEL DB5"))
            {
                producto.ID_SIGES = 8;      // Diesel super 
            }
            else
            {
                producto.ID_SIGES = producto.ID;
                Log.Instance.WriteLog($"Producto no identificado: {producto.Descripcion}, id: {producto.ID}", LogType.t_error);
            }
        }

        private double ConvertDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Any, culture, out double result) ? result : result;
        }
    }
}
