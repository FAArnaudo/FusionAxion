﻿#pragma checksum "..\..\PanelFusion.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "97D4E7E470641CDD6D3C309FEFF97449A2ACE8056A52FF81637CC0086D5CA8CF"
//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

using FusionAxion;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace FusionAxion {
    
    
    /// <summary>
    /// PanelFusion
    /// </summary>
    public partial class PanelFusion : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 11 "..\..\PanelFusion.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid GMain;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\PanelFusion.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnCerrar;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\PanelFusion.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnMinimizar;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\PanelFusion.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel SPPanel;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\PanelFusion.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnCambiarConfig;
        
        #line default
        #line hidden
        
        
        #line 60 "..\..\PanelFusion.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label LStatus;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/FusionAxion;component/panelfusion.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\PanelFusion.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 10 "..\..\PanelFusion.xaml"
            ((FusionAxion.PanelFusion)(target)).MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.Window_MouseDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.GMain = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.BtnCerrar = ((System.Windows.Controls.Button)(target));
            
            #line 20 "..\..\PanelFusion.xaml"
            this.BtnCerrar.Click += new System.Windows.RoutedEventHandler(this.BtnCerrar_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.BtnMinimizar = ((System.Windows.Controls.Button)(target));
            
            #line 27 "..\..\PanelFusion.xaml"
            this.BtnMinimizar.Click += new System.Windows.RoutedEventHandler(this.BtnMinimizar_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.SPPanel = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 6:
            this.BtnCambiarConfig = ((System.Windows.Controls.Button)(target));
            
            #line 50 "..\..\PanelFusion.xaml"
            this.BtnCambiarConfig.Click += new System.Windows.RoutedEventHandler(this.BtnCambiarConfig_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.LStatus = ((System.Windows.Controls.Label)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

