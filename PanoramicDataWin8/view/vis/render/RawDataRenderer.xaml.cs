﻿using PanoramicDataWin8.utils;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using GeoAPI.Geometries;
using IDEA_common.operations;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.view.inq;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Data;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;
using PanoramicDataWin8.utils;
using Windows.UI.Xaml.Controls.Primitives;
using IDEA_common.operations.rawdata;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using IDEA_common.operations.histogram;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public class ObjectToFrameworkElementConverter : IValueConverter
    {
        static public Grid LastHit = null;
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            var g = new Grid();
            if (value != null)
            {
                if (false) // data is an image
                {
                    var ib = new Image();
                    ib.Source = new BitmapImage(new Uri("https://static.pexels.com/photos/39803/pexels-photo-39803.jpeg"));
                    ib.Width = ib.Height = 200;
                    g.Children.Add(ib);
                    g.CanDrag = false;
                    g.PointerPressed += (sender, e) =>
                    {
                        if (LastHit != g && LastHit != null)
                            LastHit.CanDrag = false;
                        LastHit = g;
                    };
                    g.DragStarting += (UIElement sender, DragStartingEventArgs args) =>
                    {
                        args.Data.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
                        args.Data.Properties.Add("MYFORMAT", new Uri("https://static.pexels.com/photos/39803/pexels-photo-39803.jpeg"));
                    };
                }
                else
                {
                    var tb = new TextBlock();
                    tb.FontFamily = FontFamily.XamlAutoFontFamily;
                    tb.FontSize = 14;
                    tb.Foreground = new SolidColorBrush(Colors.Black);
                    tb.Text = value.ToString();
                    tb.Height = 25;
                    tb.MinWidth = 50;
                    tb.HorizontalAlignment = HorizontalAlignment.Stretch;
                    g.Children.Add(tb);
                }
            }
            return g;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public sealed partial class RawDataRenderer : Renderer, IScribbable
    {
        public class RawColumnData : ExtendedBindableBase {
            AttributeTransformationModel _model;
            double _cwidth;
            public bool ShowScroll = false;
            public ObservableCollection<object> Data;
            public RawDataRenderer Renderer;
            public AttributeTransformationModel Model
            {
                get { return _model; }
                set
                {
                    this.SetProperty(ref _model, value);
                }
            }
            public double ColumnWidth
            {
                get { return _cwidth; }
                set
                {
                    this.SetProperty(ref _cwidth, value);
                }
            }
            public RawColumnData() { Data = new ObservableCollection<object>(); }
        }

        public ObservableCollection<RawColumnData> Records { get; set; } = new ObservableCollection<RawColumnData>();

        public RawDataRenderer()
        {
            this.InitializeComponent();
            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += PlotRenderer_Loaded;
            //xRawDataGridView.ItemsSource = Records;
            this.SizeChanged += RawDataRenderer_SizeChanged;
            this.Tapped += RawDataRenderer_Tapped;
            MainViewController.Instance.MainPage.AddHandler(PointerPressedEvent, new PointerEventHandler(RawDataRenderer_PointerPressed), true);
        }

        private void RawDataRenderer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var res = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(null).Position, this);
            if (res.Count() == 0)
            {
                xRawDataGridView.IsHitTestVisible = false;
                xListView.IsHitTestVisible = false;
            }
        }
        

        private void RawDataRenderer_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var model = (DataContext as RawDataOperationViewModel);
            xRawDataGridView.IsHitTestVisible = true;
            xListView.IsHitTestVisible = true;
            var res = VisualTreeHelper.FindElementsInHostCoordinates(e.GetPosition(null), this);
            foreach (var r in res)
                if (r is Image)
                {
                    var img = r as Image;
                    if (img.Tag != null)
                        (xRawDataGridView.Parent as Panel).Children.Remove(img.Parent as Panel);
                    else
                    {
                        var ib = new Image();
                        ib.Source = img.Source;
                        ib.Tag = "FullSize";
                        var g = new Grid();
                        g.Background = new SolidColorBrush(Colors.LightGray);
                        g.HorizontalAlignment = HorizontalAlignment.Stretch;
                        g.VerticalAlignment = VerticalAlignment.Stretch;
                        g.Children.Add(ib);
                        (xRawDataGridView.Parent as Panel).Children.Add(g);
                    }
                    break;
                }
                else if (r is TextBlock)
                {
                    var tb = r as TextBlock;
                    var col = tb.GetFirstAncestorOfType<RawDataColumn>()?.Model ??
                              model.RawDataOperationModel.AttributeTransformationModelParameters.First();
                    if (tb.Tag != null)
                    {
                        model.RawDataOperationModel.RemoveFilterModel(tb.Tag as FilterModel);
                        tb.FontStyle = Windows.UI.Text.FontStyle.Normal;
                        tb.FontWeight = Windows.UI.Text.FontWeights.Normal;
                        tb.Tag = null;
                    }
                    else
                    {
                        tb.FontStyle = Windows.UI.Text.FontStyle.Italic;
                        tb.FontWeight = Windows.UI.Text.FontWeights.ExtraBold;
                        tb.SelectAll();
                        var fm = new FilterModel();
                        var vc = col.AttributeModel.DataType == IDEA_common.catalog.DataType.String ?
                            new ValueComparison(col, IDEA_common.operations.recommender.Predicate.CONTAINS, tb.Text) :
                            new ValueComparison(col, IDEA_common.operations.recommender.Predicate.EQUALS, ToObject(col.AttributeModel, tb.Text));

                        fm.ValueComparisons.Add(vc);
                        model.RawDataOperationModel.AddFilterModel(fm);
                        tb.Tag = fm;
                    }
                }
            e.Handled = true;
        }

        object ToObject(AttributeModel model, string text)
        {
            switch (model.DataType)
            {
                case IDEA_common.catalog.DataType.String: return text;
                case IDEA_common.catalog.DataType.Float:
                    {
                        float f;
                        float.TryParse(text, out f);
                        return f;
                    }
                case IDEA_common.catalog.DataType.Double:
                    {
                        double d;
                        double.TryParse(text, out d);
                        return d;
                    }
                case IDEA_common.catalog.DataType.Int:
                    {
                        int i;
                        int.TryParse(text, out i);
                        return i;
                    }
                case IDEA_common.catalog.DataType.DateTime:
                    {
                        DateTime d;
                        DateTime.TryParse(text, out d);
                        return d;
                    }
                default:
                    return text;
            }
        }

        private void RawDataRenderer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //this.xRawDataGridView.ItemsPanelRoot.SetBinding(WidthProperty, b);
            // this.xRawDataView.ItemsPanelRoot.SetBinding(WidthProperty, b);
        }
        

        void PlotRenderer_Loaded(object sender, RoutedEventArgs e)
        {
            var cp = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollBar>(this);
            cp.HorizontalAlignment = HorizontalAlignment.Left;
            cp.Background = new SolidColorBrush(Colors.DarkGray);
            cp.Margin = new Thickness(0, 0, 2, 0);
            Grid.SetColumn(cp, 0);
            var cp2 = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollContentPresenter>(this);
            Grid.SetColumn(cp2, 1);
        }

        public override void Dispose()
        {
            base.Dispose();
            (DataContext as RawDataOperationViewModel).OperationModel.OperationModelUpdated -= OperationModelUpdated;
            (DataContext as RawDataOperationViewModel).OperationModel.PropertyChanged -= OperationModel_PropertyChanged;
            if (dxSurface != null)
            {
                dxSurface.Dispose();
            }
        }

        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (DataContext as RawDataOperationViewModel).OperationModel.OperationModelUpdated -= OperationModelUpdated;
                (DataContext as RawDataOperationViewModel).OperationModel.OperationModelUpdated += OperationModelUpdated;
                (DataContext as RawDataOperationViewModel).OperationModel.PropertyChanged -= OperationModel_PropertyChanged;
                (DataContext as RawDataOperationViewModel).OperationModel.PropertyChanged += OperationModel_PropertyChanged;

                var result = (DataContext as RawDataOperationViewModel).OperationModel.Result;
                if (result != null)
                {
                    loadResult(result);
                    render();
                }
                else
                {
                    var operationModel = (RawDataOperationModel)((OperationViewModel)DataContext).OperationModel;
                    if (!operationModel.AttributeTransformationModelParameters.Any())
                    {
                    }
                }
            }
        }
        void OperationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var operationModel = (RawDataOperationModel)((OperationViewModel)DataContext).OperationModel;
            if (e.PropertyName == operationModel.GetPropertyName(() => operationModel.Result))
            {
                var result = (DataContext as RawDataOperationViewModel).OperationModel.Result;
                if (result != null)
                {
                    loadResult(result);
                    render();
                }
            }
            else if (e.PropertyName == "Function")
            {
                render(operationModel.Function);
            }
        }

        void OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            if (e is FilterOperationModelUpdatedEventArgs &&
                ((FilterOperationModelUpdatedEventArgs)e).FilterOperationModelUpdatedEventType == FilterOperationModelUpdatedEventType.ClearFilterModels)
            {
                render();
            }
            if (e is FilterOperationModelUpdatedEventArgs &&
                ((FilterOperationModelUpdatedEventArgs)e).FilterOperationModelUpdatedEventType == FilterOperationModelUpdatedEventType.FilterModels)
            {
                render();
            }
            if (e is VisualOperationModelUpdatedEventArgs)
            {
                render();
            }
        }

        void loadResult(IResult result)
        {
            if (result is RawDataResult)
                loadRawDataResult(result);
            else if (result is HistogramResult)
                loadHistogramResult(result);
        }

        static int AttributeTransformationModelToIndex(RawDataOperationModel operationModel, AttributeModel am)
        {
            for (int i = 0; i < operationModel.AttributeTransformationModelParameters.Count; i++)
            {
                var opAtm = operationModel.AttributeTransformationModelParameters[i];
                if (opAtm.AttributeModel == am)
                    return i;
            }
            return -1;
        }

        void loadHistogramResult(IResult result)
        {
            var hresult = result as HistogramResult;
            var operationModel = (RawDataOperationModel)((OperationViewModel)DataContext).OperationModel;

            var groupBy = operationModel.AttributeTransformationModelParameters.Where((atm) => atm.GroupBy).Select((atm)=>atm.AttributeModel).ToList();

            Records.Clear();
            var newRecords = new ObservableCollection<RawColumnData>();
            foreach (var col in  operationModel.AttributeTransformationModelParameters)
            {
                var acollection = new RawColumnData
                {
                    Model = col,
                    ColumnWidth = 200,// records.First() is string || records.First() is IDEA_common.range.PreProcessedString ? 200 : 50,
                    Renderer = this,
                    ShowScroll = false
                };
                newRecords.Add(acollection);
            }

            int[] indices = groupBy.Select((g) => AttributeTransformationModelToIndex(operationModel, g)).ToArray();
            foreach (var bin in hresult.Bins)
            {
                extractBin(operationModel,
                    newRecords, 
                     indices,
                    operationModel.AttributeTransformationModelParameters.Select((atm)=>atm.AttributeModel).Where((am) => !groupBy.Contains(am)).ToList(),
                    bin.Value);
            }
            Records = newRecords;

            this.xListView.ItemsSource = newRecords;
        }

        static void extractBin(RawDataOperationModel operationModel, ObservableCollection<RawColumnData> newRecords, int[] binIndex, List<AttributeModel> grouped,  Bin values)
        {
            for (int i = 0; i < binIndex.Length; i++)  // load index column data
                newRecords[binIndex[i]].Data.Add("" + values.Spans[i].Min + " - " + values.Spans[i].Max);
            for (int i = 0; i < values.AggregateResults.Length/3; i++)
            {
                var res = values.AggregateResults[i, 2];
                if (res is DoubleValueAggregateResult)
                    newRecords[AttributeTransformationModelToIndex(operationModel, grouped[i])].Data.Add((res as DoubleValueAggregateResult).Result);
                else newRecords[AttributeTransformationModelToIndex(operationModel, grouped[i])].Data.Add(res.N);
            }
        }

        void loadRawDataResult(IResult result)
        {
            var model = (DataContext as RawDataOperationViewModel);
            Records = new ObservableCollection<RawColumnData>();
            xWordCloud.TheText = "";
            model.RawDataOperationModel.ClearFilterModels();
            if ((result as RawDataResult).Samples.Count() == 1 && (result as RawDataResult).WeightedWords?.Count > 0)
            {
                xWordCloud.WeightedWords = (result as RawDataResult).WeightedWords.FirstOrDefault().Value;
                xWordCloud.Visibility = Visibility.Visible;
                xRawDataGridView.Visibility = Visibility.Collapsed;
                xListView.Visibility = Visibility.Collapsed;
            }
            else
            {
                xWordCloud.Visibility = Visibility.Collapsed;
                for (int sampleIndex = 0; sampleIndex < (result as RawDataResult).Samples.Count(); sampleIndex++) {
                    var s = (result as RawDataResult).Samples[model.RawDataOperationModel.AttributeTransformationModelParameters[sampleIndex].AttributeModel.RawName];
                    if (s.Count > 0)
                        loadRecordsAsync(s, model.RawDataOperationModel.AttributeTransformationModelParameters[sampleIndex], sampleIndex+1 == (result as RawDataResult).Samples.Count());

                }
                if ((result as RawDataResult).Samples.Count() == 1)
                {
                    xRawDataGridView.Visibility = Visibility.Visible;
                    xListView.Visibility = Visibility.Collapsed;
                }
                else
                {
                    xRawDataGridView.Visibility = Visibility.Collapsed;
                    xListView.Visibility = Visibility.Visible;
                }

            }
            this.xListView.ItemsSource = Records;
            this.xRawDataGridView.ItemsSource = Records.FirstOrDefault()?.Data;
        }

        void loadRecordsAsync(List<object> records, AttributeTransformationModel model, bool showScroll)
        {
            var acollection = new RawColumnData {
                Model = model,
                ColumnWidth =records.First() is string|| records.First() is IDEA_common.range.PreProcessedString ? 200:50,
                Renderer = this,
                ShowScroll = showScroll
            };
            Records.Add(acollection);
            if (records != null)
#pragma warning disable CS4014
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Low,
                () =>
                {
                    foreach (var val in records)
                    {
                        acollection.Data.Add(val);
                    }
                });
#pragma warning restore CS4014
        }

        void render(RawDataOperationModel.FunctionApplied function = null) 
        {
            var model = (DataContext as RawDataOperationViewModel);
            if (function?.Sorted?.Item2 != null)
            {
                var sortField = function?.Sorted.Item1;
                var sortDir = function?.Sorted.Item2;

                var combinde = new List<object[]>();
                for (var i = 0; i < Records.First().Data.Count; i++)
                {
                    var objList = new object[Records.Count];
                    for (int j = 0; j < Records.Count; j++)
                        objList[j] = Records[j].Data[i];
                    combinde.Add(objList);
                }
                var attrModelIndex = model.RawDataOperationModel.AttributeTransformationModelParameters.IndexOf((atm) => atm.AttributeModel.RawName == sortField);
                if (model.RawDataOperationModel.AttributeTransformationModelParameters[attrModelIndex].AttributeModel.DataType == IDEA_common.catalog.DataType.Int)
                    Sort(combinde.OrderBy( (obj) => (long)obj[attrModelIndex]), sortDir == false);
                else if (model.RawDataOperationModel.AttributeTransformationModelParameters[attrModelIndex].AttributeModel.DataType == IDEA_common.catalog.DataType.Double)
                    Sort(combinde.OrderBy((obj) => (double)obj[attrModelIndex]), sortDir == false);
                else
                    Sort(combinde.OrderBy((obj) => obj[attrModelIndex].ToString()), sortDir == false);
            }
            xListView.ItemsSource = Records;
        }
        public void Sort(IEnumerable<object[]> sortedList, bool down = false)
        {
            for (var index = 0; index < Records.Count; index++)
            {
                Records[index].Data.Clear();
                foreach (var x in down ? sortedList.Reverse() : sortedList)
                    Records[index].Data.Add(x[index]);
            }
        }
        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                return this.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            }
        }

        public bool IsDeletable
        {
            get { return false; }
        }

        public IGeometry Geometry
        {
            get
            {
                var model = this.DataContext as RawDataOperationViewModel;

                Rct bounds = new Rct(model.Position, model.Size);
                return bounds.GetPolygon();
            }
        }

        public List<IScribbable> Children
        {
            get { return new List<IScribbable>(); }
        }
        public bool Consume(InkStroke inkStroke)
        {
            return false;
        }
    }
}
