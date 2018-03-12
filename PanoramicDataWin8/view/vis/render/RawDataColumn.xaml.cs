using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.controller.view;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.System;
using IDEA_common.catalog;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class RawDataColumn : UserControl
    {
        public class RawDataColumnModel : ExtendedBindableBase
        {
            AttributeTransformationModel _model;
            double _cwidth;
            public bool                         ShowScroll = false;
            public ObservableCollection<object> Data = new ObservableCollection<object>();
            public Grid                         RendererListView;
            public bool                         IsEditable { get; set; } = false;
            public bool                         IsImage => AttributeTranformationModel.AttributeModel.VisualizationHints.FirstOrDefault() == VisualizationHint.Image;
            public AttributeTransformationModel AttributeTranformationModel
            {
                get => _model;
                set => SetProperty(ref _model, value);
            }
            public List<AttributeModel>         PrimaryKeys;
            public List<List<object>>           PrimaryValues;
            public double                       ColumnWidth
            {
                get => _cwidth;
                set => SetProperty(ref _cwidth, value);
            }
            public HorizontalAlignment          Alignment;
        }

        public RawDataColumnModel DataColumnModel { get => DataContext as RawDataColumnModel; }

        public RawDataColumn()
        {
            this.InitializeComponent();
            DataContextChanged += (sender, e) =>  // set the style of the cell based on properties of the data column model
            {
                if (DataColumnModel?.IsImage == true)
                {
                    xListView.ItemContainerStyle = (Style)Resources["ImageStyle"];
                    xListView.ItemTemplate = (DataTemplate)Resources["ImageColTemplate"];
                }
                else if (DataColumnModel?.IsEditable == true)
                {
                    xListView.ItemContainerStyle = (Style)Resources[DataColumnModel.Alignment == HorizontalAlignment.Right ? "RightStyle" : "LeftStyle"];
                    xListView.ItemTemplate = (DataTemplate)Resources["ValueColTemplate"];

                }
                else if (DataColumnModel != null)
                {
                    xListView.ItemContainerStyle = (Style)Resources[DataColumnModel.Alignment == HorizontalAlignment.Right ? "RightStyle" : "LeftStyle"];
                    xListView.ItemTemplate = (DataTemplate)Resources["TextColTemplate"];
                }
            };
        }

        /// <summary>
        /// Configure the display so that only one scroll bar is shown on the right of the view, 
        /// instead of one scroll bar for each column. Then synch all the scrollbars to move together.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void xListView_Loaded(object sender, RoutedEventArgs e)
        {
            xListView.ItemsSource = DataColumnModel.Data;
            var scrollViewer = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollViewer>(this);
            if (scrollViewer != null)
                scrollViewer.ViewChanged += scrollViewer_ViewChanged;
            if (!DataColumnModel.ShowScroll)
            {
                var scrollBar = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollBar>(this);
                if (scrollBar != null)
                    scrollBar.Visibility = Visibility.Collapsed;
            } 
            void scrollViewer_ViewChanged(object s, ScrollViewerViewChangedEventArgs a)
            {
                foreach (var dcol in DataColumnModel.RendererListView.Children)
                {
                    var columnScrollViewer = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollViewer>(dcol);
                    if (columnScrollViewer != scrollViewer)
                        columnScrollViewer.ChangeView(null, scrollViewer.VerticalOffset, null, true);
                }
            }
        }

        /// <summary>
        /// Handle editing of a data value when it gets keyboard focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EditableDataValue_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;

            void Tb_KeyDown(object s, KeyRoutedEventArgs a)
            {
                var shift = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
                if (a.Key == VirtualKey.Enter)
                {
                    tb.LostFocus -= Tb_LostFocus;
                    EditableDataValue_Update();
                    var container = this.GetFirstAncestorOfType<Grid>();
                    var ind = (this.DataContext as RawDataColumnModel).Data.IndexOf(tb.DataContext);
                    var nextInd = shift ? Math.Max(0, ind - 1) : Math.Min(xListView.ItemsPanelRoot.Children.Count - 1, ind + 1);
                    var listContainer = xListView.ItemsPanelRoot.Children[nextInd] as ListViewItem;
                    var nextTextBoxToEdit = listContainer.GetFirstDescendantOfType<TextBox>();
                    nextTextBoxToEdit.SelectAll();
                    nextTextBoxToEdit.Focus(FocusState.Keyboard);
                    a.Handled = true;
                }
                else
                {
                    tb.LostFocus -= Tb_LostFocus;
                    tb.LostFocus += Tb_LostFocus;
                }
            }
            void Tb_LostFocus(object s, RoutedEventArgs a)
            {
                EditableDataValue_Update();
                tb.KeyDown -= Tb_KeyDown;
                tb.LostFocus -= Tb_LostFocus;
            }
            tb.KeyDown -= Tb_KeyDown;
            tb.KeyDown += Tb_KeyDown;

            void EditableDataValue_Update()
            {
                var dcontext = tb.DataContext as Tuple<int, object>;
                string text = tb.Text;
                var primKeys = DataColumnModel.PrimaryKeys;
                var AttributeModel = DataColumnModel.AttributeTranformationModel.AttributeModel;
                var codemodel = AttributeModel.FuncModel as AttributeFuncModel.AttributeAssignedValueFuncModel;
                var key = primKeys.Select((pkey) => DataColumnModel.PrimaryValues[primKeys.IndexOf(pkey)][dcontext.Item1]).ToList();
                switch (AttributeModel.DataType)
                {
                    case DataType.Int:
                        int newval;
                        if (int.TryParse(text, out newval))
                        {
                            codemodel?.Add(DataColumnModel.PrimaryKeys, new AttributeModel.AttributeFuncModel.AttributeAssignedValueFuncModel.Key(key), newval);
                        }
                        else tb.Text = "";
                        break;
                    case DataType.Double:
                        double newdoub;
                        if (double.TryParse(text, out newdoub))
                        {
                            codemodel?.Add(DataColumnModel.PrimaryKeys, new AttributeModel.AttributeFuncModel.AttributeAssignedValueFuncModel.Key(key), newdoub);
                        }
                        else tb.Text = "";
                        break;
                    case DataType.String:
                        codemodel?.Add(DataColumnModel.PrimaryKeys, new AttributeModel.AttributeFuncModel.AttributeAssignedValueFuncModel.Key(key), text);
                        break;
                }
                if (codemodel != null)
                {
                    var func = IDEAAttributeModel.Function(AttributeModel.RawName, MainViewController.Instance.MainModel.SchemaModel.OriginModels.First());
                    func.SetCode(codemodel.ComputeCode(AttributeModel.DataType), AttributeModel.DataType, false);
                }
            }
        }
    }
}
