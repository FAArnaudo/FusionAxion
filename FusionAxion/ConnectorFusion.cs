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
        private Fusion cFusion = null;
        private static readonly CultureInfo culture = CultureInfo.InvariantCulture;

        public ConnectorFusion() { }

        public Fusion Fusion
        {
            get
            {
                if (cFusion == null)
                {
                    cFusion = new Fusion();
                }
                return cFusion;
            }
            set => cFusion = value;
        }

        public void ComandoConfiguracionDeLaEstacion()
        {
            Station estacion = Station.Instance;
            List<Surtidor> surtidores = new List<Surtidor>();
            List<Producto> productos = new List<Producto>();
            List<Tanque> tanques = new List<Tanque>();
            ArrayList products = new ArrayList();

            try
            {
                FusionForecourt fusionForecourt = Fusion.GetConfig();

                estacion.NumeroDeProductos = fusionForecourt.GetGradesCount();

                if (Fusion.GetFCRTProducts(products))
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
                if (Fusion.GetPumpsCount(ref pumpCount))
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
                                        ID = fusionHose.m_iPhysicalID + 1
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

        private void GetCodigoSiges(Producto producto)
        {
            if (producto.Descripcion.Contains("SUPER"))
            {
                producto.ID_SIGES = 1;
            }
            else if (producto.Descripcion.Equals("REGULAR") || producto.Descripcion.Equals("QUANTIUM"))
            {
                producto.ID_SIGES = 4;
            }
            else if (producto.Descripcion.Equals("ION_DIESEL"))
            {
                producto.ID_SIGES = 6;
            }
            else if (producto.Descripcion.Equals("GNC"))
            {
                producto.ID_SIGES = 7;
            }
            else if (producto.Descripcion.Equals("DIESEL"))
            {
                producto.ID_SIGES = 8;
            }
            else
            {
                producto.ID_SIGES = producto.ID;
                Log.Instance.WriteLog($"Producto no identificado: {producto.Descripcion}", LogType.t_error);
            }
        }

        private double ConvertDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Any, culture, out double result) ? result : result;
        }
    }
}
