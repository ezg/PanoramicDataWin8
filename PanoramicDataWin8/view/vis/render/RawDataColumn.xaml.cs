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
using static PanoramicDataWin8.model.data.attribute.AttributeModel;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.controller.view;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.System;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class RawDataColumn : UserControl
    {
        public class RawDataColumnModel : ExtendedBindableBase
        {
            AttributeTransformationModel _model;
            double _cwidth;
            public bool ShowScroll = false;
            public ObservableCollection<object> Data;
            public Grid  RendererListView;
            public bool IsEditable { get; set; } = false;
            public bool IsImage => Model.AttributeModel.VisualizationHints.FirstOrDefault() == IDEA_common.catalog.VisualizationHint.Image;
            public AttributeTransformationModel Model
            {
                get { return _model; }
                set
                {
                    this.SetProperty(ref _model, value);
                }
            }
            public List<AttributeModel> PrimaryKeys;
            public List<List<object>> PrimaryValues;
            public double ColumnWidth
            {
                get { return _cwidth; }
                set
                {
                    this.SetProperty(ref _cwidth, value);
                }
            }
            public HorizontalAlignment Alignment;
            public RawDataColumnModel() { Data = new ObservableCollection<object>(); }
        }

        public RawDataColumn()
        {
            this.InitializeComponent();
            DataContextChanged += RawDataColumn_DataContextChanged;
        }

        private void RawDataColumn_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            Debug.WriteLine("DC = " + args.NewValue);
            var rawDataModel = args.NewValue as RawDataColumnModel;
            if (rawDataModel != null)
            {
                if (rawDataModel.IsImage)
                {
                    xListView.ItemContainerStyle = (Style)Resources["ImageStyle"];
                    xListView.ItemTemplate = (DataTemplate)Resources["ImageColTemplate"];
                }
                else if (rawDataModel.IsEditable)
                {
                    xListView.ItemContainerStyle = (Style)Resources[rawDataModel.Alignment == HorizontalAlignment.Right ? "RightStyle" : "LeftStyle"];
                    xListView.ItemTemplate = (DataTemplate)Resources["ValueColTemplate"];

                }
                else
                {
                    xListView.ItemContainerStyle = (Style)Resources[rawDataModel.Alignment == HorizontalAlignment.Right ? "RightStyle" : "LeftStyle"];
                    xListView.ItemTemplate = (DataTemplate)Resources["TextColTemplate"];
                }
            }
        }

        private void xListView_Loaded(object sender, RoutedEventArgs e)
        {
            var listView = (sender as ListView);
            var dataCol = (DataContext as RawDataColumnModel);
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
                return (DataContext as RawDataColumnModel).Model;
            }
        }
        public List<AttributeModel> PrimaryKeys
        {
            get
            {
                return (DataContext as RawDataColumnModel).PrimaryKeys;
            }
        }
        public List<List<object>> PrimaryValues
        {
            get
            {
                return (DataContext as RawDataColumnModel).PrimaryValues;
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
            var dataCol = DataContext as RawDataColumnModel;
            foreach (var dcol in dataCol.RendererListView.Children)
            {
                var cp = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollViewer>(dcol);
                if (cp != scroll)
                    cp.ChangeView(null, scroll.VerticalOffset, null, true);
            }
        }


        private void UpdateCellValue(Tuple<int, object> dcontext, string text)
        {
            int newval;
            var codemodel = Model.AttributeModel.FuncModel as AttributeFuncModel.AttributeAssignedValueFuncModel;
            var func = IDEAAttributeModel.Function(Model.AttributeModel.RawName,
                                 MainViewController.Instance.MainModel.SchemaModel.OriginModels.First());
            if (int.TryParse(text, out newval))
            {
                var key = new List<object>(PrimaryKeys.Select((pkey) => PrimaryValues[PrimaryKeys.IndexOf(pkey)][dcontext.Item1]));
                codemodel.Add(PrimaryKeys, new AttributeModel.AttributeFuncModel.AttributeAssignedValueFuncModel.Key(key), newval);
                func.SetCode(codemodel.ComputeCode(), IDEA_common.catalog.DataType.Int, false);
            }
        }

        private void Tb_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var shift = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
            var tb = sender as TextBox;
            if (e.Key == VirtualKey.Enter)
            {
                tb.LostFocus -= Tb_LostFocus;
                UpdateCellValue(tb.DataContext as Tuple<int, object>, tb.Text);
                var container = this.GetFirstAncestorOfType<Grid>();
                var ind = (this.DataContext as RawDataColumnModel).Data.IndexOf(tb.DataContext);
                var nextInd = shift ? Math.Max(0, ind - 1) : Math.Min(xListView.ItemsPanelRoot.Children.Count - 1, ind + 1);
                var listContainer = xListView.ItemsPanelRoot.Children[nextInd] as ListViewItem;
                var tbn = listContainer.GetFirstDescendantOfType<TextBox>();
                tbn.SelectAll();
                tbn.Focus(FocusState.Keyboard);
                e.Handled = true;
            }
            else
            {
                tb.LostFocus -= Tb_LostFocus;
                tb.LostFocus += Tb_LostFocus;
            }
        }

        private void Tb_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            UpdateCellValue(tb.DataContext as Tuple<int, object>, tb.Text);
            tb.KeyDown -= Tb_KeyDown;
            tb.LostFocus -= Tb_LostFocus;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.KeyDown -= Tb_KeyDown;
            tb.KeyDown += Tb_KeyDown;
        }
        
    }
}
