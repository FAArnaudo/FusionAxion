# CDS - Fuel Dispenser Controller

**CDS** (Fuel Dispenser Controller) is a software solution designed as an interface between gas station pump controllers and the station management system.

This system is implemented for **CEM-44** and **FUSION** fuel dispenser controllers but can be extended to support additional pump control systems.

## Key Features

- **Station Configuration**  
  Retrieves the station configuration, including **dispensers, hoses, products, tanks**, etc.

- **Sales Recording**  
  Captures every sale at the service station forecourt, including both volume and payment amount.

- **Fuel Measurements and Pricing**  
  Allows the system to obtain fuel measurements, configure prices, and make adjustments.

- **Shift Closing**  
  Includes a feature for closing shifts, ensuring accurate management of station operations at the end of each workday.

## Database 💾

All information is stored in an **SQLite** database, which is automatically created once the system successfully communicates with the corresponding pump controller.

## Technologies Used

- **Programming Language:**

    <img src="https://sitecloudy.com/wp-content/uploads/2023/07/Que-es-C-y-cuales-son-sus-ventajas-y-desventajas-2.png" width="45" height="50" />
  
- **Database:**

    <img src="https://cdn.iconscout.com/icon/free/png-256/free-sqlite-282687.png" width="70" height="70" />
  


## Installation 🛠️

To get started with CDS, follow these steps:

1. Clone the repository.
2. Open the project in **Visual Studio**.
3. Install the necessary dependencies.
4. Configure the connection parameters for the dispenser controller.
5. Run the system.
