using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
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
            DataContextChanged += RawDataColumn_DataContextChanged;
        }

        private void RawDataColumn_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var rawdata = args.NewValue as RawDataRenderer.RawColumnData;
            if (rawdata != null)
            {
                if (IsImage)
                {
                    xListView.ItemContainerStyle = (Style)Resources["ImageStyle"];
                    xListView.ItemTemplate = (DataTemplate)Resources["ImageColTemplate"];
                }
                else
                {
                    xListView.ItemContainerStyle = (Style)Resources[rawdata.Alignment == HorizontalAlignment.Right ? "RightStyle" : "LeftStyle"];
                    xListView.ItemTemplate = (DataTemplate)Resources["TextColTemplate"];

                }
            }
        }

        private void xListView_Loaded(object sender, RoutedEventArgs e)
        {
            var listView = (sender as ListView);
            var dataCol = (DataContext as RawDataRenderer.RawColumnData);
            listView.ItemsSource = dataCol.Data;
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

        public bool IsImage
        {
            get => Model.AttributeModel.VisualizationHints.FirstOrDefault() == IDEA_common.catalog.VisualizationHint.Image;
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
