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
    /// Lógica de interacción para SurtidoresViews.xaml
    /// </summary>
    public partial class SurtidoresViews : Window
    {
        public SurtidoresViews()
        {
            InitializeComponent();
            Loaded += SurtidoresViews_Loaded;
        }

        private void SurtidoresViews_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable result = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT idSurtidor, Manguera, Producto, Precio, DescProd FROM Surtidores");
            DG_Surtidores.ItemsSource = result.AsDataView();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
