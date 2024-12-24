# CDS
Controlador de Surtidores as "CDS" is a software created as an interface between a hardware integration for the gas station pump controller and a management system.
This system is an implementation for a few dispenser controller hardware such as CEM-44 and FUSION but could be extended for more pump control systems.

The management system needs certain functions to achieve sales automation and avoid errors due to mishandling of handwritten data.
Therefore, the key to the CDS is:
  1. Obtain the station configuration (Dispensers, hoses, products, tanks, etc).
  2. Capture every sale at the service station forecourt, both volume and dollar amount.
  3. Obtain fuel measurements, know the prices or configure them.
  4. Es muy importante poder realizar cortes de turno, para encapsular las ventas y poder tener unbuen control de los turnos.

This information would be reflected in a SQLite database, which is automatically created by the system once it manages to communicate with the corresponding controller.
