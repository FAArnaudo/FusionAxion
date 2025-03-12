using FusionClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion
{
    public class ConnectorFusion
    {
        private Fusion cFusion = null;
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
    }
}
