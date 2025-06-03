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
    public class DespachosPorSurtidorViewModel
    {
        public ObservableCollection<Despacho> Datos { get; set; }
        public int surtidor;
        public DespachosPorSurtidorViewModel(int surtidor)
        {
            Datos = new ObservableCollection<Despacho>();
            this.surtidor = surtidor;
            CargarDatos();
        }

        private void CargarDatos()
        {
            DataTable data = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT id, surtidor, manguera, descripcion, PPU, monto, volumen, fecha " +
                                                                         $"FROM Despachos " +
                                                                         $"WHERE surtidor =  {surtidor} " +
                                                                         $"ORDER BY id DESC " +
                                                                         $"LIMIT 100");

            if (data.Rows.Count != 0)
            {
                foreach (DataRow row in data.Rows)
                {
                    DateTime fecha = Convert.ToDateTime(row["fecha"]);

                    Despacho dato = new Despacho
                    {
                        IdDespacho = Convert.ToInt32(row["id"]),
                        IdSurtidor = Convert.ToInt32(row["surtidor"]),
                        IdManguera = Convert.ToInt32(row["manguera"]),
                        Producto = row["descripcion"].ToString(),
                        PPU = Convert.ToDouble(row["PPU"]),
                        Monto = Convert.ToDouble(row["monto"]),
                        Volumen = Convert.ToDouble(row["volumen"]),
                        Fecha = fecha.ToString("dd-MM-yyyy HH:mm:ss")
                    };

                    Datos.Add(dato);
                }
            }
        }
    }
}
