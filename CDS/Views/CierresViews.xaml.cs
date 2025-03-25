using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CDS.Views
{
    /// <summary>
    /// Lógica de interacción para CierresViews.xaml
    /// </summary>
    public partial class CierresViews : Window
    {
        public CierresViews()
        {
            InitializeComponent();
            Loaded += VerCierres_Loaded;
        }

        private void VerCierres_Loaded(object sender, RoutedEventArgs e)
        {
            string query = "SELECT id, fecha";

            DG_CierreDeTurno.Columns.Clear();

            // Crear y configurar columnas de texto
            DG_CierreDeTurno.Columns.Add(CreateTextColumn("ID Cierre", "id", 60));
            DG_CierreDeTurno.Columns.Add(CreateTextColumn("Fecha", "fecha", 150));

            string controller = Configuration.GetConfiguration().Controller;

            switch (controller)
            {
                case "CEM-44":
                    DG_CierreDeTurno.Columns.Add(CreateTextColumn("Monto Contado", "monto_contado", 130));
                    DG_CierreDeTurno.Columns.Add(CreateTextColumn("Volumen Contado", "volumen_contado", 130));
                    DG_CierreDeTurno.Columns.Add(CreateTextColumn("Monto YPF Ruta", "monto_YPFruta", 140));
                    DG_CierreDeTurno.Columns.Add(CreateTextColumn("Volumen YPF Ruta", "volumen_YPFruta", 140));

                    query += ", monto_contado, volumen_contado, monto_YPFruta, volumen_YPFruta";
                    break;
                case "FUSION":
                    DG_CierreDeTurno.Columns.Add(CreateTextColumn("Mensaje", "message", 200));

                    query += ", message";
                    break;
                default:
                    break;
            }

            DG_CierreDeTurno.Columns.Add(CreateTextColumn("Estado", "state", 90));
            query += ", state FROM Cierres";

            DataTable result = ConnectorSQLite.Instance.ExecuteSelectQuery(query);

            DG_CierreDeTurno.ItemsSource = result.AsDataView();
        }

        // Método auxiliar para crear columnas de texto
        private DataGridTextColumn CreateTextColumn(string header, string bindingPath, double width)
        {
            DataGridTextColumn column = new DataGridTextColumn
            {
                Header = header,
                Width = width,
                Binding = new Binding(bindingPath)
            };
            return column;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
