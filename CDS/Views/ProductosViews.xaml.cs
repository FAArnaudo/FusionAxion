using System.Data;
using System.Windows;

namespace CDS.Views
{
    /// <summary>
    /// Lógica de interacción para ProductosViews.xaml
    /// </summary>
    public partial class ProductosViews : Window
    {
        public ProductosViews()
        {
            InitializeComponent();
            Loaded += ProductosViews_Loaded;
        }

        private void ProductosViews_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable result = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT id_producto, producto, precio FROM Productos");
            DG_Productos.ItemsSource = result.AsDataView();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
