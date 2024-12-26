using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDS
{
    public class ControllerFusion : Controller
    {
        private string ip;
        private string estacion;
        private IDiscount discount;


        public ControllerFusion(string ip, string estacion)
        {
            IP = ip;
            Estacion = estacion;
            SetFlagDiscount();
        }

        public string IP
        {
            get => ip;
            set
            {
                if (ip == null || !ip.Equals(value))
                {
                    ip = value;
                }
            }
        }

        public string Estacion
        {
            get => estacion;
            set
            {
                if (estacion == null || !estacion.Equals(value))
                {
                    estacion = value;
                }
            }
        }

        private void SetFlagDiscount()
        {
            switch (Estacion)
            {
                case "AXION":
                    SetDiscount(new DiscountAxion());
                    break;
                case "PUMA":
                    SetDiscount(new DiscountPuma());
                    break;
                default:
                    break;
            }
        }
        private IDiscount GetDiscount()
        {
            return discount;
        }

        private void SetDiscount(IDiscount value)
        {
            discount = value;
        }

        public override void ActualizarProductos()
        {
            throw new NotImplementedException();
        }

        public override void ActualizarTanques()
        {
            throw new NotImplementedException();
        }

        public override void ConfigurarEstacion()
        {
            throw new NotImplementedException();
        }

        public override void GrabarCierre()
        {
            throw new NotImplementedException();
        }

        public override void GrabarDespachos()
        {
            throw new NotImplementedException();
        }

        public void CheckDiscount()
        {
            GetDiscount().CheckDiscount();
        }
    }
    internal interface IDiscount
    {
        void CheckDiscount();
    }

    public class DiscountPuma : IDiscount
    {
        public DiscountPuma() { }

        public void CheckDiscount()
        {
            throw new NotImplementedException();
        }
    }

    public class DiscountAxion : IDiscount
    {
        public DiscountAxion() { }
        public void CheckDiscount()
        {
            throw new NotImplementedException();
        }
    }
}
