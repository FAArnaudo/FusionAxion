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
