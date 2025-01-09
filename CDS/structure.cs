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
            ProductsNumber = 0;
            TanksNumber = 0;
            PumpsNumber = 0;

            NivelesDePrecio = new List<NivelDePrecio>();
            Productos = new List<Producto>();
            Tanques = new List<Tanque>();
            Surtidores = new List<Surtidor>();

            for (int i = 0; i < nivelesDePrecio; i++)
            {
                NivelDePrecio nivelDePrecio = new NivelDePrecio
                {
                    Nivel = i
                };

                NivelesDePrecio.Add(nivelDePrecio);
            }
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
        public int ProductsNumber { get; set; }
        public int TanksNumber { get; set; }
        public int PumpsNumber { get; set; }

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
    }

    public class Despacho
    {
        private int idProducto;
        private double monto;
        private double volumen;
        private double ppu;
        private int id;
        public Despacho() { }
        public int IdProducto { get => idProducto; set => idProducto = value; }
        public double Monto { get => monto; set => monto = value; }
        public double Volumen { get => volumen; set => volumen = value; }
        public double PPU { get => ppu; set => ppu = value; }
        public int ID { get => id; set => id = value; }

        public class DespachoCem : Despacho
        {
            private string status;
            private bool ventaFacturada;
            public DespachoCem()
            {
                
            }
            public string Status { get => status; set => status = value; }
            public bool VentaFacturada { get => ventaFacturada; set => ventaFacturada = value; }
        }
    }

    public class CierreDeTurno
    {
        public CierreDeTurno() { }

        public int Id { get; set; }
        public string FechaCierre { get; set; }
        public double TotalesMonto { get; set; }
        public double TotalesVolumen { get; set; }
    }
}
