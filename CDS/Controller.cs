using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDS
{
    public abstract class Controller
    {
        protected string ip;
        public Controller(string ip)
        {
            this.ip = ip;
        }

        /// <summary>
        /// Este método estático es el encargado de configurar la estructura de la estacion,
        /// para obtener los productos, los tanques, las mangueras y surtidores, etc.
        /// Y se guarda la informacion en la tabla de la base de datos correspondiente.
        /// </summary>
        public abstract void ConfigurarEstacion();

        /// <summary>
        /// Este método estático es el encargado de procesar la informacion de los tanques
        /// y guardarla en la tabla de la base de datos correspondiente.
        /// </summary>
        public abstract void ActualizarTanques();

        /// <summary>
        /// Este método estático es el encargado de grabar los productos que trae el controlador
        /// y guardarla en la tabla de la base de datos correspondiente.
        /// </summary>
        public abstract void ActualizarProductos();

        /// <summary>
        /// Este método estático es el encargado de procesar la informacion de los surtidores
        /// y guardarla en la tabla de la base de datos correspondiente.
        /// </summary>
        public abstract void GrabarDespachos();

        /// <summary>
        /// Este método estático es el encargado de procesar la informacion del corte del ultimo turno
        /// y guardarla en la base de datos correspondiente.
        /// </summary>
        public abstract void GrabarCierre();
    }
}
