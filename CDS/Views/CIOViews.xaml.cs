using System;
using System.IO;
using System.Windows;

namespace CDS.Views
{
    /// <summary>
    /// Lógica de interacción para CIOViews.xaml
    /// </summary>
    public partial class CIOViews : Window
    {
        private static readonly string txtCIO = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/CIO.txt";
        public CIOViews()
        {
            InitializeComponent();
            Loaded += CIOViews_Loaded;
        }

        private void CIOViews_Loaded(object sender, RoutedEventArgs e)
        {
            if (ExisteConfiguracion())
            {
                // Lee todas las líneas del archivo
                string[] lines = File.ReadAllLines(txtCIO);

                for (int i = 0; i < lines.Length; i++)
                {
                    try
                    {
                        string line = lines[i].Trim().Substring(15);
                        lines[i] = !string.IsNullOrEmpty(line) ? line : "";

                        switch (i)
                        {
                            case 0:
                                ipVox_TextBox.Text = line;
                                break;
                            case 1:
                                ipBridge_TextBox.Text = line;
                                break;
                            case 2:
                                ipServer_TextBox.Text = line;
                                break;
                            case 3:
                                ipLibre_TextBox.Text = line;
                                break;
                            default:
                                ruteoEstatico_TextBox.Text = line;
                                break;
                        }
                    }
                    catch (ArgumentOutOfRangeException) { }
                }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Crea el archivo CIO.txt
                using (StreamWriter outputFile = new StreamWriter(txtCIO, false))
                {
                    outputFile.WriteLine($"IP VOX:        {ipVox_TextBox.Text.Trim()}");
                    outputFile.WriteLine($"IP BRIDGE:     {ipBridge_TextBox.Text.Trim()}");
                    outputFile.WriteLine($"IP SERVER:     {ipServer_TextBox.Text.Trim()}");
                    outputFile.WriteLine($"IP LIBRE:      {ipLibre_TextBox.Text.Trim()}");
                    outputFile.WriteLine($"RUTEO ESTATICO:{ruteoEstatico_TextBox.Text.Trim()}");
                }
                Close();
                _ = ConnectorSQLite.Instance.ExecuteNonQuery("UPDATE Datos_CIO " +
                                                        $"SET ip_vox = '{ipVox_TextBox.Text}', ip_bridge = '{ipBridge_TextBox.Text}', " +
                                                        $"ip_server = '{ipServer_TextBox.Text}', ip_libre = '{ipLibre_TextBox.Text}', " +
                                                        $"ruteo_estatico = '{ruteoEstatico_TextBox.Text}' " +
                                                        $"WHERE id_cio = {1}");
            }
            catch (Exception ex)
            {
                Log.Instance.WriteLog("Error al guardar la configuración. Excepción: " + ex.Message, LogType.t_error);
            }
        }

        private bool ExisteConfiguracion()
        {
            return File.Exists(txtCIO);
        }
    }
}
