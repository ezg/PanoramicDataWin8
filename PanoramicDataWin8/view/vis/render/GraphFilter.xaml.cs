using PanoramicDataWin8.model.view;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class GraphFilter : Renderer
    {
        public GraphFilter()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as GraphFilterViewModel).GraphFilterOperationModel.TargetGraphOperationModel.SetPropertyNodes();
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (this.DataContext as GraphFilterViewModel).GraphFilterOperationModel.TargetGraphOperationModel.SetPropertyNodes();
        }
    }
}
