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

        private Station()
        {
            ProductsNumber = 0;
            TanksNumber = 0;
            PumpsNumber = 0;

            Products = new List<Product>();
            Tanks = new List<Tank>();
            Pumps = new List<Pump>();
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
        public List<Product> Products { get; set; }
        public List<Tank> Tanks { get; set; }
        public List<Pump> Pumps { get; set; }
    }
    public class Pump
    {
        public Pump()
        {
            Id = 0;
            HosesNumber = 0;
            Hoses = null;
        }

        public int Id { get; set; }
        public int HosesNumber { get; set; }
        public List<Hose> Hoses { get; set; }
    }
    public class Hose
    {
        private int id;
        private Product product;
        public Hose()
        {
            id = 0;
            product = null;
        }

        public int ID
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                }
            }
        }

        public Product Product
        {
            get => product;
            set
            {
                if (value != null)
                {
                    product = value;
                }
            }
        }
    }
    public class Tank
    {
        private int id;
        private Product product;
        private double maxValue;
        private double productVolume;
        public Tank()
        {
            id = 0;
            product = null;
            maxValue = 0;
            productVolume = 0;
        }
        public int ID
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                }
            }
        }

        public Product Product
        {
            get => product;
            set
            {
                if (value != null)
                {
                    product = value;
                }
            }
        }

        public double MaxVolume
        {
            get => maxValue;
            set
            {
                if (maxValue != value)
                {
                    maxValue = value;
                }
            }
        }

        public double ProductVolume
        {
            get => productVolume;
            set
            {
                if (productVolume != value)
                {
                    productVolume = value;
                }
            }
        }
    }
    public class Product
    {
        private string description;
        private int id;
        private double price;
        public Product()
        {
            description = "";
            id = 0;
            price = 0;
        }

        public string Description
        {
            get => description;
            set
            {
                if (value != null && !description.Equals(value))
                {
                    description = value;
                }
            }
        }

        public int ID
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                }
            }
        }

        public double Price
        {
            get => price;
            set
            {
                if (price != value)
                {
                    price = value;
                }
            }
        }
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
