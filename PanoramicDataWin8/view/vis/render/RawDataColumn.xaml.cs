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
            var rawdata = args.NewValue as RawDataColumnModel;
            if (rawdata != null)
            {
                if (IsImage)
                {
                    xListView.ItemContainerStyle = (Style)Resources["ImageStyle"];
                    xListView.ItemTemplate = (DataTemplate)Resources["ImageColTemplate"];
                }
                else if (IsEditable)
                {
                    xListView.ItemContainerStyle = (Style)Resources[rawdata.Alignment == HorizontalAlignment.Right ? "RightStyle" : "LeftStyle"];
                    xListView.ItemTemplate = (DataTemplate)Resources["ValueColTemplate"];

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

        public bool IsImage
        {
            get => Model.AttributeModel.VisualizationHints.FirstOrDefault() == IDEA_common.catalog.VisualizationHint.Image;
        }

        public bool IsEditable
        {
            get => Model.AttributeModel.FuncModel.ModelType == AttributeModel.AttributeFuncModel.AttributeModelType.Assigned;
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

        private void TextBox_TextChanged2(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            var codemodel = Model.AttributeModel.FuncModel as AttributeFuncModel.AttributeCodeFuncModel;
            var dcontext = tb.DataContext as Tuple<int, object>;
            int newval;
            var func = IDEAAttributeModel.Function(Model.AttributeModel.RawName,
                MainViewController.Instance.MainModel.SchemaModel.OriginModels.First());
            var dict = (func.FuncModel as AttributeFuncModel.AttributeCodeFuncModel).Data as Dictionary<List<object>, object>;
            if (int.TryParse(tb.Text, out newval))
            {
                var key = new List<object>();
                foreach (var primaryKey in PrimaryKeys)
                {
                    key.Add(PrimaryValues[PrimaryKeys.IndexOf(primaryKey)][dcontext.Item1]);
                }
                bool found = false;
                foreach (var di in dict)
                {
                    foreach (var k in di.Key)
                        if (key[di.Key.IndexOf(k)] == k)
                        {
                            found = true;
                            dict.Remove(di.Key);
                            break;
                        }
                    if (found)
                        break;
                }
                dict[key] = newval;

                var code = "";
                if (PrimaryKeys.Count != 0)
                {
                    foreach (var di in dict)
                    {
                        code += "(";
                        foreach (var primaryKey in PrimaryKeys)
                        {
                            code += primaryKey.RawName + " == " + di.Key[PrimaryKeys.IndexOf(primaryKey)] + "&&";
                        }
                        code = code.Substring(0, code.Length - 2) + ")";
                        code +=  " ? " + di.Value + " : ";
                    }
                }
                code += "0";
                Debug.WriteLine("<<<code>>>" + code);
                func.SetCode(code, IDEA_common.catalog.DataType.Int, false);
            }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.TextChanged -= TextBox_TextChanged;
            tb.TextChanged -= TextBox_TextChanged2;
            tb.TextChanged += TextBox_TextChanged2;
        }

        private void TextBoxLoaded(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).TextChanged += TextBox_TextChanged;
        }
    }
}
