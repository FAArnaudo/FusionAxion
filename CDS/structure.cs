using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDS
{
    public class Station
    {
        private int productsNumber;
        private int tanksNumber;
        private int pumpsNumber;
        private List<Product> products;
        private List<Tank> tanks;
        private List<Pump> pumps;
        public Station()
        {
            productsNumber = 0;
            tanksNumber = 0;
            pumpsNumber = 0;
        }
        public int ProductsNumber { get => productsNumber; set => productsNumber = value; }
        public int TanksNumber { get => tanksNumber; set => tanksNumber = value; }
        public int PumpsNumber { get => pumpsNumber; set => pumpsNumber = value; }
        public List<Product> Products { get => products; set => products = value; }
        public List<Tank> Tanks { get => tanks; set => tanks = value; }
        public List<Pump> Pumps { get => pumps; set => pumps = value; }
    }
    public class Pump
    {
        private int id;
        private int hosesNumber;
        private List<Hose> hoses;
        public Pump()
        {
            id = 0;
            hosesNumber = 0;
            hoses = null;
        }

        public int Id { get => id; set => id = value; }
        public int HosesNumber { get => hosesNumber; set => hosesNumber = value; }
        public List<Hose> Hoses { get => hoses; set => hoses = value; }
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
}
