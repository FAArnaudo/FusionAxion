CDS - Controlador de Surtidores
CDS (Controlador de Surtidores) es un software que actúa como interfaz entre el hardware de controladores de bombas de gasolinera y el sistema de gestión de la estación de servicio.

Este sistema está diseñado para integrarse principalmente con los controladores de surtidores CEM-44 y FUSION, pero puede extenderse a otros sistemas de control de bombas.

Funcionalidades Clave
Obtención de Configuración de la Estación
Captura la configuración de la estación, incluyendo dispensadores, mangueras, productos, tanques, entre otros.

Registro de Ventas
Registra las ventas realizadas en el área de servicio, tanto en volumen como en el monto del pago.

Obtención de Mediciones de Combustible
Permite obtener mediciones de combustible, configurar precios y realizar ajustes en la estación.

Cierre de Turno
El sistema incluye una funcionalidad para realizar el cierre de turno, asegurando la correcta gestión de las operaciones al final de cada jornada.

Base de Datos
Toda la información se refleja en una base de datos SQLite que se crea automáticamente una vez que el sistema establece comunicación con el controlador correspondiente.

Tecnología Utilizada
Lenguaje de Programación:

C#
Base de Datos:

SQLite
Iconos:
Puedes incluir iconos para hacer la documentación más atractiva. Por ejemplo:

🛠️ Instalación
💾 Base de Datos
🚗 Ventas y Combustible
⏳ Cierre de Turno
Instalación
Clona el repositorio.
Abre el proyecto en Visual Studio.
Instala las dependencias necesarias.
Configura los parámetros de conexión con el controlador de surtidor.
Ejecuta el sistema.
