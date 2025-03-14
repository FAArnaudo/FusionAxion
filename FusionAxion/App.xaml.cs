using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FusionAxion.Views;

namespace FusionAxion
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private PanelFusion panelFusion;
        private ConfigurationView configurationView;
        protected void ApplicationStart(object sender, StartupEventArgs e)
        {
            if (!ConfigurationModel.ExistConfiguracion())
            {
                configurationView = new ConfigurationView();
                configurationView.Show();
                configurationView.IsVisibleChanged += (s, ev) =>
                {
                    if (configurationView.IsVisible == false && configurationView.IsLoaded)
                    {
                        InitPanelFusion();
                    }
                };
            }
            else
            {
                InitPanelFusion();
            }
        }

        private void InitPanelFusion()
        {
            panelFusion = new PanelFusion();
            panelFusion.Show();
            configurationView.Close();
        }
    }
}
