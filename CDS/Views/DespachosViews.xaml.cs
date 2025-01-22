using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CDS.Views
{
    /// <summary>
    /// Lógica de interacción para DespachosViews.xaml
    /// </summary>
    public partial class DespachosViews : Window
    {
        public DespachosViews()
        {
            InitializeComponent();
            Loaded += DespachosViews_Loaded;
        }

        private void DespachosViews_Loaded(object sender, RoutedEventArgs e)
        {
            string query = "SELECT id, surtidor, manguera, producto, PPU, volumen, monto, descripcion, ";

            DG_Despachos.Columns.Clear();

            // Crear y configurar columnas de texto
            DG_Despachos.Columns.Add(CreateTextColumn("ID", "id", 60));
            DG_Despachos.Columns.Add(CreateTextColumn("Surtidor", "surtidor", 70));
            DG_Despachos.Columns.Add(CreateTextColumn("Manguera", "manguera", 70));
            DG_Despachos.Columns.Add(CreateTextColumn("Producto", "producto", 70));
            DG_Despachos.Columns.Add(CreateTextColumn("PPU", "PPU", 70));
            DG_Despachos.Columns.Add(CreateTextColumn("Volumen", "volumen", 70));
            DG_Despachos.Columns.Add(CreateTextColumn("Monto", "monto", 70));
            DG_Despachos.Columns.Add(CreateTextColumn("Descripción", "descripcion", 100));

            string controller = Configuration.GetConfiguration().Controller;

            switch (controller)
            {
                case "CEM-44":
                    DG_Despachos.Columns.Add(CreateTextColumn("Facturado", "facturado", 70));
                    DG_Despachos.Columns.Add(CreateTextColumn("YPF Ruta", "YPFRuta", 70));
                    DG_Despachos.Columns.Add(CreateTextColumn("fecha", "fecha", 120));

                    query += "facturado, YPFRuta, fecha FROM Despachos ORDER BY fecha DESC";
                    break;
                case "FUSION":

                    break;
                default:
                    DG_Despachos.Columns.Add(CreateTextColumn("Facturado", "facturado", 130));
                    DG_Despachos.Columns.Add(CreateTextColumn("YPF Ruta", "YPFRuta", 130));
                    break;
            }

            DataTable result = ConnectorSQLite.Instance.ExecuteSelectQuery(query);

            DG_Despachos.ItemsSource = result.AsDataView();
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
