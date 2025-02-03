using System.Data;
using System.Windows;

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
