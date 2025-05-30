using FusionAxion.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FusionAxion.ViewModels
{
    public class TanquesViewModel : ViewModelBase
    {
        public ObservableCollection<Tanque> Datos { get; set; }

        public TanquesViewModel()
        {
            Datos = new ObservableCollection<Tanque>();
            CargarDatos();
        }

        private void Actualizar()
        {
            ControllerFusion.Instance.ActualizarTanques();
            Thread.Sleep(200);
        }

        private void CargarDatos()
        {
            DataTable data = ConnectorSQLite.Instance.ExecuteSelectQuery($"SELECT id_tanque, volumen_actual, capacidad_maxima, actualizado " +
                                                                         $"FROM Tanques");

            DateTime fecha = Convert.ToDateTime(data.Rows[0]["actualizado"]);
            TimeSpan diferencia = DateTime.Now - fecha;

            if (data.Rows.Count != 0)
            {
                if (diferencia.TotalMinutes > 5)
                {
                    Actualizar();
                }

                foreach (DataRow row in data.Rows)
                {
                    fecha = Convert.ToDateTime(row["actualizado"]);

                    Tanque dato = new Tanque
                    {
                        ID = Convert.ToInt32(row["id_tanque"]),
                        VolumenDeProducto = Convert.ToDouble(row["volumen_actual"]),
                        CapacidadMaxima = Convert.ToDouble(row["capacidad_maxima"]),
                        Fecha = fecha.ToString("dd-MM-yyyy HH:mm:ss")
                    };

                    Datos.Add(dato);
                }
            }
        }
    }
}
