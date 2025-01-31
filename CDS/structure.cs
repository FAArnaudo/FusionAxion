using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDS
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
        public double PrecioUnitario { get; set; }
        public int IdTanque { get; set; }
    }
    public class ProductoCem : Producto
    {
        private int idProductoDespacho;
        public ProductoCem() { idProductoDespacho = 0; }
        public int IdProductoDespacho
        {
            get => idProductoDespacho;
            set
            {
                if (idProductoDespacho != value)
                {
                    idProductoDespacho = value;
                }
            }
        }
    }

    public class Despacho
    {
        private int idProducto;
        private int idSurtidor;
        private int idManguera;
        private string producto;
        private double monto;
        private double volumen;
        private double ppu;
        private int idDespacho;
        public Despacho() { }
        public int IdProducto { get => idProducto; set => idProducto = value; }
        public double Monto { get => monto; set => monto = value; }
        public double Volumen { get => volumen; set => volumen = value; }
        public double PPU { get => ppu; set => ppu = value; }
        public int IdDespacho { get => idDespacho; set => idDespacho = value; }
        public string Producto { get => producto; set => producto = value; }
        public int IdSurtidor { get => idSurtidor; set => idSurtidor = value; }
        public int IdManguera { get => idManguera; set => idManguera = value; }
    }
    public class DespachoCem : Despacho
    {
        private ESTADO_SURTIDOR status;
        private bool ventaFacturada;
        private int nroDeVenta;
        public DespachoCem() { }
        public ESTADO_SURTIDOR Status { get => status; set => status = value; }
        public bool VentaFacturada { get => ventaFacturada; set => ventaFacturada = value; }
        public int NroDeVenta { get => nroDeVenta; set => nroDeVenta = value; }

        public enum ESTADO_SURTIDOR
        {
            DISPONIBLE,
            EN_SOLICITUD,
            DESPACHANDO,
            AUTORIZADO,
            VENTA_FINALIZADA_IMPAGA,
            DEFECTUOSO,
            ANULADO,
            DETENIDO
        }
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

    public class CierreFusion : CierreDeTurno
    {
        public CierreFusion() { }

        public string ErrorCode { get; set; }
        public string Message { get; set; }
    }

    public class CierreCem : CierreDeTurno
    {
        public const int MEDIOS_DE_PAGO = 8;
        public CierreCem()
        {
            TotalesMedioDePago = new List<TotalMedioDePago>();
            TotalesPorPeriodoPorNivelPorProducto = new List<List<List<TotalPorProducto>>>();
            TotalesPorTanque = new List<TotalPorTanque>();
            ProductosEnTanque = new List<ProductoEnTanque>();
        }
        public List<TotalMedioDePago> TotalesMedioDePago { get; set; }
        public int Impuesto1 { get; set; }
        public int Impuesto2 { get; set; }
        public int PeriodoDePrecios { get; set; }
        public int NivelesDePrecio { get; set; }
        public List<List<List<TotalPorProducto>>> TotalesPorPeriodoPorNivelPorProducto { get; set; }
        public List<TotalPorTanque> TotalesPorTanque { get; set; }
        public List<ProductoEnTanque> ProductosEnTanque { get; set; }
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

    public class ProductoEnTanque
    {
        public ProductoEnTanque() { }
        public int NumeroDeProducto { get; set; }
        public double VolumenEnTanques { get; set; }
        public double AguaEnTanques { get; set; }
        public double VacioEnTanques { get; set; }
        public double CapacidadEnTanques { get; set; }
    }
}
