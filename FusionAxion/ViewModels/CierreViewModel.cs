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
    public class CierreViewModel : ViewModelBase
    {
        public ObservableCollection<CierreDeTurno> Datos { get; set; }

        public CierreViewModel()
        {
            Datos = new ObservableCollection<CierreDeTurno>();
            CargarDatos();
        }

        private void CargarDatos()
        {
            DataTable data = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT id, monto_contado, volumen_contado, state, message, fecha " +
                                                                                $"FROM Cierres " +
                                                                                $"ORDER BY id DESC LIMIT 10");

            if (data.Rows.Count != 0)
            {
                foreach (DataRow row in data.Rows)
                {
                    DateTime fecha = Convert.ToDateTime(row["fecha"]);

                    CierreDeTurno cierreDeTurno = new CierreDeTurno
                    {
                        ID = Convert.ToInt32(row["id"]),
                        TotalesMonto = Convert.ToDouble(row["monto_contado"]),
                        TotalesVolumen = Convert.ToDouble(row["volumen_contado"]),
                        Estado = row["state"].ToString(),
                        Message = row["message"].ToString(),
                        FechaCierre = fecha.ToString("dd-MM-yyyy HH:mm:ss")
                    };

                    Datos.Add(cierreDeTurno);
                }
            }
        }
    }
}
