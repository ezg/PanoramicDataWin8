using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class RawDataColumn : UserControl
    {
        public RawDataColumn()
        {
            this.InitializeComponent();
        }

        private void xListView_Loaded(object sender, RoutedEventArgs e)
        {
            var dataCol = (DataContext as RawDataRenderer.RawColumnData);
            xListView.ItemsSource = dataCol.Data;
            var cp = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollViewer>(this);
            if (cp != null)
                cp.ViewChanged += Cp_ViewChanged;
            if (!dataCol.ShowScroll)
            {
                var sbar = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollBar>(this);
                if (sbar != null)
                    sbar.Visibility = Visibility.Collapsed;
            } else
            {
                //var scroll = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollViewer>(this);
                //foreach (var dcol in dataCol.Renderer.xListView.ItemsPanelRoot.Children)
                //{
                //    var otherScroll = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollViewer>(dcol);
                //    if (otherScroll != scroll)
                //        otherScroll.SetBinding(ScrollViewer.VerticalOffsetProperty, new Binding() { Source = scroll, Path = new PropertyPath(nameof(ScrollViewer.VerticalOffsetProperty)) });
                //}
            }
        }

        public AttributeTransformationModel Model
        {
            get
            {
                return (DataContext as RawDataRenderer.RawColumnData).Model;
            }
        }

        private void Cp_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            //var dataCol = (DataContext as RawDataRenderer.RawColumnData);
            //var scroll = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollViewer>(this);
            //foreach (var dcol in dataCol.Renderer.xListView.ItemsPanelRoot.Children)
            //{
            //    var otherScroll = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollViewer>(dcol);
            //    if (otherScroll != scroll)
            //        otherScroll.SetBinding(ScrollViewer.VerticalOffsetProperty, new Binding() { Source = scroll, Path = new PropertyPath(nameof(ScrollViewer.VerticalOffsetProperty)) });
            //}
            var scroll = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollViewer>(this);
            var dataCol = (DataContext as RawDataRenderer.RawColumnData);
            foreach (var dcol in dataCol.Renderer.xListView.Children)
            {
                var cp = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollViewer>(dcol);
                if (cp != scroll)
                    cp.ChangeView(null, scroll.VerticalOffset, null, true);
            }
        }
    }
}
