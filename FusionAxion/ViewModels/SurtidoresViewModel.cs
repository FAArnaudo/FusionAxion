using FusionAxion.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion.ViewModels
{
    public class SurtidoresViewModel : ViewModelBase
    {
        public ObservableCollection<SurtidorInfo> Datos { get; set; }
        public SurtidoresViewModel()
        {
            Datos = new ObservableCollection<SurtidorInfo>();
            CargarDatos();
        }

        private void CargarDatos()
        {
            DataTable data = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT IdSurtidor, Manguera, DescProd, Precio " +
                                                                         $"FROM Surtidores");

            if (data.Rows.Count != 0)
            {
                foreach (DataRow row in data.Rows)
                {
                    SurtidorInfo dato = new SurtidorInfo
                    {
                        Surtidor = Convert.ToInt32(row["IdSurtidor"]),
                        Manguera = Convert.ToInt32(row["Manguera"]),
                        Producto = row["DescProd"].ToString(),
                        PPU = Convert.ToDouble(row["Precio"]),
                    };

                    Datos.Add(dato);
                }
            }
        }
    }

    public class SurtidorInfo
    {
        public SurtidorInfo() { }

        public int Surtidor { get; set; }
        public int Manguera { get; set; }
        public string Producto { get; set; }
        public double PPU { get; set; }
    }
}
