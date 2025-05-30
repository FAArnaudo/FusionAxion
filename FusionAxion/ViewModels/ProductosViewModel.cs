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
    public class ProductosViewModel : ViewModelBase
    {
        public ObservableCollection<Producto> Datos { get; set; }
        public ProductosViewModel()
        {
            Datos = new ObservableCollection<Producto>();
            CargarDatos();
        }

        private void CargarDatos()
        {
            DataTable data = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT id_producto, producto, precio " +
                                                                         $"FROM Productos");

            if (data.Rows.Count != 0)
            {
                foreach (DataRow row in data.Rows)
                {
                    Producto dato = new Producto
                    {
                        ID = Convert.ToInt32(row["id_producto"]),
                        Descripcion = row["producto"].ToString(),
                        PrecioUnitario = Convert.ToDouble(row["precio"]),
                    };

                    Datos.Add(dato);
                }
            }
        }
    }
}
