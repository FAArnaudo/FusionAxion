using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion
{
    public class Station
    {
        private static Station instance = null;
        public const int nivelesDePrecio = 5;

        private Station()
        {
            NumeroDeProductos = 0;
            NumeroDeTanques = 0;
            NumeroDeSurtidores = 0;

            NivelesDePrecio = new List<NivelDePrecio>();
            Productos = new List<Producto>();
            Tanques = new List<Tanque>();
            Surtidores = new List<Surtidor>();
        }

        public static Station Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Station();
                }

                return instance;
            }
        }
        public int NumeroDeProductos { get; set; }
        public int NumeroDeTanques { get; set; }
        public int NumeroDeSurtidores { get; set; }

        public List<NivelDePrecio> NivelesDePrecio { get; set; }
        public List<Producto> Productos { get; set; }
        public List<Tanque> Tanques { get; set; }
        public List<Surtidor> Surtidores { get; set; }
    }

    public class NivelDePrecio
    {
        public NivelDePrecio()
        {
            Nivel = 0;
            SurtidoresPorNivelDePrecio = new List<Surtidor>();
        }
        public int Nivel { get; set; }
        public List<Surtidor> SurtidoresPorNivelDePrecio { get; set; }
    }
    public class Surtidor
    {
        public Surtidor()
        {
            ID = 0;
            NumeroDeMangueras = 0;
            NivelDeSurtidor = 0;
            Mangueras = new List<Manguera>();
        }

        public int ID { get; set; }
        /// <summary>
        /// Numero de mangueras que contiene el surtidor (o cara)
        /// </summary>
        public int NumeroDeMangueras { get; set; }
        /// <summary>
        /// Es el nivel de precio al que esta funcionando este surtidor.
        /// </summary>
        public int NivelDeSurtidor { get; set; }
        public List<Manguera> Mangueras { get; set; }
    }
    public class Manguera
    {
        public Manguera()
        {
            ID = 0;
            Producto = null;
        }

        public int ID { get; set; }
        public Producto Producto { get; set; }
    }
    public class Tanque
    {
        public Tanque()
        {
            ID = 0;
            Product = null;
            CapacidadMaxima = 0;
            VolumenDeProducto = 0;
            VolumenDeAgua = 0;
            VolumenVacio = 0;
        }
        public int ID { get; set; }
        public Producto Product { get; set; }
        public double CapacidadMaxima { get; set; }
        public double VolumenDeProducto { get; set; }
        public double VolumenDeAgua { get; set; }
        public double VolumenVacio { get; set; }
    }
    public class Producto
    {
        public Producto()
        {
            Descripcion = "";
            ID = 0;
            PrecioUnitario = 0;
        }

        public string Descripcion { get; set; }
        public int ID { get; set; }
        public int ID_SIGES { get; set; }
        public double PrecioUnitario { get; set; }
        public int IdTanque { get; set; }
    }

    public class Despacho
    {
        public Despacho() { }
        public int IdProducto { get; set; }
        public double Monto { get; set; }
        public double Volumen { get; set; }
        public double PPU { get; set; }
        public int IdDespacho { get; set; }
        public string Producto { get; set; }
        public int IdSurtidor { get; set; }
        public int IdManguera { get; set; }
    }

    public class CierreDeTurno
    {
        public CierreDeTurno()
        {
            TotalesPorManguera = new List<TotalPorManguera>();
        }

        public int ID { get; set; }
        public string FechaCierre { get; set; }
        public double TotalesMonto { get; set; }
        public double TotalesVolumen { get; set; }
        public List<TotalPorManguera> TotalesPorManguera { get; set; }
        public string Estado { get; set; }
    }

    public class TotalMedioDePago
    {
        public TotalMedioDePago() { }
        public int NumeroDeMedioDePago { get; set; }
        public double TotalMonto { get; set; }
        public double TotalVolumen { get; set; }
    }

    public class TotalPorProducto
    {
        public TotalPorProducto() { }
        public int Periodo { get; set; }
        public int Nivel { get; set; }
        public int NumeroDeProducto { get; set; }
        public double PrecioUnitario { get; set; }
        public double TotalMonto { get; set; }
        public double TotalVolumen { get; set; }
    }

    public class TotalPorManguera
    {
        public TotalPorManguera() { }
        public int NumeroDeSurtidor { get; set; }
        public int NumeroDeManguera { get; set; }
        public double TotalVntasMonto { get; set; }
        public double TotalVntasVolumen { get; set; }
        public double TotalVntasSinControlMonto { get; set; }
        public double TotalVntasSinControlVolumen { get; set; }
        public double TotalPruebasMonto { get; set; }
        public double TotalPruebasVolumen { get; set; }
    }

    public class TotalPorTanque
    {
        public TotalPorTanque() { }
        public int NumeroDeTanque { get; set; }
        public double Producto { get; set; }
        public double Agua { get; set; }
        public double Vacio { get; set; }
        public double Capacidad { get; set; }
    }

    public enum CODIGO_PRODUCTOS
    {
        SUPER = 1,
        NAFTA_NORMAL,
        ULTRA_DIESEL,
        INFINIA,
        KEROSENE,
        INFINIA_DIESEL,
        GNC,
        DIESEL,
        AZUL_32
    }
}
