using System.Data;
using System.Windows;

namespace CDS.Views
{
    /// <summary>
    /// Lógica de interacción para TanquesViews.xaml
    /// </summary>
    public partial class TanquesViews : Window
    {
        public TanquesViews()
        {
            InitializeComponent();
            Loaded += TanquesViews_Loaded;
        }

        private void TanquesViews_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable result = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT id_tanque, volumen_actual, capacidad_maxima, actualizado FROM Tanques");
            DG_Tanques.ItemsSource = result.AsDataView();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
