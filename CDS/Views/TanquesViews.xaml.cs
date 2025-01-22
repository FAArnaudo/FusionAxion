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
