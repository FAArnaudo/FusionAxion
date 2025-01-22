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
