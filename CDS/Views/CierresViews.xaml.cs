using System.Data;
using System.Windows;

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
            DataTable result = ConnectorSQLite.Instance.ExecuteSelectQuery("SELECT id, fecha, monto_contado, volumen_contado, monto_YPFruta, volumen_YPFruta, state FROM Cierres");
            DG_CierreDeTurno.ItemsSource = result.AsDataView();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
