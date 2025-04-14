using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CDS
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex;
        private const string MutexName = "MiAppUnicaPorUsuario";

        protected override void OnStartup(StartupEventArgs e)
        {
            mutex = new Mutex(true, MutexName, out bool isNewInstance);

            if (!isNewInstance)
            {
                _ = MessageBox.Show("El programa ya se encuentra en ejecución.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown(); // Cierra esta nueva instancia
                return;
            }

            base.OnStartup(e);
        }
    }
}
