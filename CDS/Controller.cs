
namespace CDS
{
    public abstract class Controller
    {
        public Controller() { }

        /// <summary>
        /// Este método es el encargado de verificar la conexión con el controlador y notificarlo
        /// en la base de datos.
        /// </summary>
        /// <returns> true: conexión activa. false: sin conexión </returns>
        public abstract bool VerificarConexión();

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
        public abstract void GrabarDespachos(Surtidor surtidor);

        /// <summary>
        /// Este método estático es el encargado de procesar la informacion del corte del ultimo turno
        /// y guardarla en la base de datos correspondiente.
        /// </summary>
        public abstract void GrabarCierre();
    }
}
