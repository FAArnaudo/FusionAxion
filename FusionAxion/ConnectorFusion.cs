using FusionClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion
{
    public class ConnectorFusion
    {
        private static readonly CultureInfo culture = CultureInfo.InvariantCulture;

        public ConnectorFusion() { }

        public void ComandoConfiguracionDeLaEstacion(Fusion Fusion)
        {
            Log.Instance.WriteLog($"", LogType.t_info);
            Station estacion = Station.Instance;
            List<Surtidor> surtidores = new List<Surtidor>();
            List<Producto> productos = new List<Producto>();
            List<Tanque> tanques = new List<Tanque>();
            ArrayList products = new ArrayList();

            try
            {
                FusionForecourt fusionForecourt = Fusion.GetConfig();

                estacion.NumeroDeProductos = fusionForecourt.GetGradesCount();
                Log.Instance.WriteLog($"Numeero de productos: {fusionForecourt.GetGradesCount()}", LogType.t_info);

                if (Fusion.GetFCRTProducts(products))
                {
                    foreach (FusionProduct fusionProduct in products)
                    {
                        if (fusionProduct != null)
                        {
                            Log.Instance.WriteLog($"Producto: {fusionProduct.m_iProductId}, ID: {fusionProduct.m_iProductNr}", LogType.t_info);

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
                if (Fusion.GetPumpsCount(ref pumpCount))
                {
                    Log.Instance.WriteLog($"Numero de surtidores: {pumpCount}", LogType.t_info);
                    estacion.NumeroDeSurtidores = pumpCount;
                    pumpCount = 0;
                    foreach (FusionPump pump in fusionForecourt.o_Pump)
                    {
                        if (pump != null)
                        {
                            pumpCount++;
                            Log.Instance.WriteLog($"Surtidor: {pumpCount}, numero de mangueras: {pump.m_iHoses}", LogType.t_info);
                            Surtidor surtidor = new Surtidor
                            {
                                ID = pumpCount,
                                NumeroDeMangueras = pump.m_iHoses,
                            };

                            foreach (FusionHose fusionHose in pump.o_Hose)
                            {
                                if (fusionHose != null)
                                {
                                    Log.Instance.WriteLog($"\tManguera: {fusionHose.m_iPhysicalID + 1}, Producto: {fusionHose.m_iGradeNr}, PPU: {fusionHose.m_strPPU}, Tanque: {fusionHose.m_strTanks}", LogType.t_info);
                                    Manguera manguera = new Manguera
                                    {
                                        ID = fusionHose.m_iPhysicalID + 1
                                    };

                                    foreach (Producto producto in productos)
                                    {
                                        if (producto.ID == fusionHose.m_iGradeNr && producto.PrecioUnitario == 0)
                                        {
                                            producto.PrecioUnitario = ConvertDouble(fusionHose.m_strPPU);
                                            producto.IdTanque = 0;
                                            if (!string.IsNullOrEmpty(fusionHose.m_strTanks))
                                            {
                                                producto.IdTanque = Convert.ToInt32(fusionHose.m_strTanks.Substring(1, 1));
                                            }
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
                    if (!producto.Descripcion.Equals("GNC"))
                    {
                        Log.Instance.WriteLog($"Tanque: {producto.IdTanque}, Producto: {producto.Descripcion}", LogType.t_info);
                        Tanque tanque = new Tanque
                        {
                            Product = producto,
                            ID = producto.IdTanque
                        };

                        tanques.Add(tanque);
                    }
                }
                estacion.NumeroDeTanques = tanques.Count;
                estacion.Tanques = tanques;
            }
            catch (NullReferenceException e)
            {
                Log.Instance.WriteLog($"Error al obtener la configuración de la estación - NullReferenceException. Excepción: {e.Message}.\n", LogType.t_error);
                throw new NullReferenceException($"Error al obtener la configuración de la estación - NullReferenceException. Excepción: {e.Message}");
            }
            catch (Exception e)
            {
                Log.Instance.WriteLog($"Error al obtener la configuración de la estación. Excepción: {e.Message}.\n", LogType.t_error);
                throw new Exception($"Error al obtener la configuración de la estación. Excepción: {e.Message}");
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
