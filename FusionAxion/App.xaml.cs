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
        protected void ApplicationStart(object sender, StartupEventArgs e)
        {
            if (!ConfigurationModel.ExistConfiguracion())
            {
                OpenConfigView();
            }
            else
            {
                ControllerFusion.Instance.Connect(ConfigurationModel.GetConfiguration().IP);

                if (!ControllerFusion.Instance.CheckConnection())
                {
                    OpenConfigView();
                }
                else
                {
                    InitPanelFusion();
                }
            }
            //InitPanelFusion();
        }

        private void InitPanelFusion()
        {
            panelFusion = new PanelFusion();
            panelFusion.Show();
        }

        private void OpenConfigView()
        {
            ConfigurationView configurationView = new ConfigurationView();
            configurationView.Show();
            configurationView.IsVisibleChanged += (s, ev) =>
            {
                if (configurationView.IsVisible == false && configurationView.IsLoaded)
                {
                    InitPanelFusion();
                }
            };
        }
    }
}
