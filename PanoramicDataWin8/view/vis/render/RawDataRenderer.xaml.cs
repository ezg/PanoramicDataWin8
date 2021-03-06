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
using PanoramicDataWin8.model.view;
using static PanoramicDataWin8.view.vis.render.RawDataColumn;
using IDEA_common.range;
using Newtonsoft.Json;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class RawDataRenderer : Renderer, IScribbable, AttributeViewModelEventHandler
    {

        public ObservableCollection<RawDataColumnModel> Records { get; set; } = new ObservableCollection<RawDataColumnModel>();

        public RawDataRenderer()
        {
            this.InitializeComponent();
            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += PlotRenderer_Loaded;
            //xRawDataGridView.ItemsSource = Records;
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
                    var col = tb.GetFirstAncestorOfType<RawDataColumn>()?.DataColumnModel.AttributeTranformationModel ??
                              model.RawDataOperationModel.AttributeTransformationModelParameters.FirstOrDefault();
                    if (tb.Tag != null)
                    {
                        model.RawDataOperationModel.RemoveFilterModel(tb.Tag as FilterModel);
                        tb.FontStyle = Windows.UI.Text.FontStyle.Normal;
                        tb.FontWeight = Windows.UI.Text.FontWeights.Normal;
                        tb.Tag = null;
                    }
                    else if (col != null)
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

        void PlotRenderer_Loaded(object sender, RoutedEventArgs e)
        {
            var cp = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollBar>(this);
            if (cp != null)
            {
                cp.HorizontalAlignment = HorizontalAlignment.Left;
                cp.Background = new SolidColorBrush(Colors.DarkGray);
                cp.Margin = new Thickness(0, 0, 2, 0);
                Grid.SetColumn(cp, 0);
                var cp2 = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollContentPresenter>(this);
                Grid.SetColumn(cp2, 1);
            }
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
                }
            }
        }
        
        void OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            return;
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

            var groupBy = operationModel.AttributeTransformationModelParameters.Where((atm) => atm.GroupBy).Select((atm) => atm.AttributeModel).ToList();

            Records.Clear();
            var newRecords = new ObservableCollection<RawDataColumn.RawDataColumnModel>();
            foreach (var col in operationModel.AttributeTransformationModelParameters)
            {
                var acollection = new RawDataColumn.RawDataColumnModel
                {
                    Alignment = HorizontalAlignment.Right,
                    AttributeTranformationModel = col,
                    RendererListView = xListView,
                    ColumnWidth = 85,// records.First() is string || records.First() is IDEA_common.range.PreProcessedString ? 200 : 50,
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
                    operationModel.AttributeTransformationModelParameters.Select((atm) => atm.AttributeModel).Where((am) => !groupBy.Contains(am)).ToList(),
                    bin.Value);
            }

            setupListView(newRecords);
        }

        private void setupListView(ObservableCollection<RawDataColumnModel> newRecords)
        {
            Records = newRecords;
            this.xListView.Children.Clear();
            this.xListView.ColumnDefinitions.Clear();
            foreach (var n in newRecords)
            {
                var rawColumn = new RawDataColumn()
                {
                    DataContext = n,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                this.xListView.Children.Add(rawColumn);
                Grid.SetColumn(xListView.Children.Last() as FrameworkElement, xListView.ColumnDefinitions.Count);
                xListView.ColumnDefinitions.Add(
                    new ColumnDefinition()
                    {
                        Width = new GridLength(n.AttributeTranformationModel.AttributeModel.DataType == IDEA_common.catalog.DataType.String ? 2 : 1, GridUnitType.Star)
                    }
                );
            }
        }

        static void extractBin(RawDataOperationModel operationModel, ObservableCollection<RawDataColumnModel> newRecords, int[] binIndex, List<AttributeModel> grouped,  Bin values)
        {
            for (int i = 0; i < binIndex.Length; i++)  // load index column data
                newRecords[binIndex[i]].Data.Add("" + values.Spans[i].Min + " - " + values.Spans[i].Max);
            for (int i = 0; i < values.AggregateResults.Length/3; i++)
            {
                var res = values.AggregateResults[i * values.YSize + 2];
                if (res is DoubleValueAggregateResult)
                    newRecords[AttributeTransformationModelToIndex(operationModel, grouped[i])].Data.Add((res as DoubleValueAggregateResult).Result);
                else newRecords[AttributeTransformationModelToIndex(operationModel, grouped[i])].Data.Add(res.N);
            }
        }
        bool hasMultipleColumns = false;
        void loadRawDataResult(IResult result)
        {
            var model = (DataContext as RawDataOperationViewModel);
            Records = new ObservableCollection<RawDataColumn.RawDataColumnModel>();
            xWordCloud.TheText = "";
            xWordCloud.WeightedWords = null;
            model.RawDataOperationModel.ClearFilterModels();
            hasMultipleColumns = (result as RawDataResult).Samples.Count() > 1;
            if ((result as RawDataResult).Samples.Count() == 1 && !hasMultipleColumns &&
                model.RawDataOperationModel.AttributeTransformationModelParameters[0].AttributeModel.VisualizationHints.FirstOrDefault() != IDEA_common.catalog.VisualizationHint.Image)
            {
                xWordCloud.WeightedWords = (result as RawDataResult).WeightedWords.FirstOrDefault().Value;
            }
            
            for (int sampleIndex = 0; sampleIndex < (result as RawDataResult).Samples.Count(); sampleIndex++) {
                var s = (result as RawDataResult).Samples[model.RawDataOperationModel.AttributeTransformationModelParameters[sampleIndex].AttributeModel.RawName];
                if (s.Count > 0)
                    loadRecordsAsync(s, model.RawDataOperationModel.AttributeTransformationModelParameters[sampleIndex], sampleIndex+1 == (result as RawDataResult).Samples.Count());

            }

            setupListView(Records);

            configureLayout(true, model.RawDataOperationModel.AttributeTransformationModelParameters[0].AttributeModel.VisualizationHints.FirstOrDefault() == IDEA_common.catalog.VisualizationHint.Image);
                
            this.xRawDataGridView.ItemsSource = Records.FirstOrDefault()?.Data;
        }

        void configureLayout(bool useDefault, bool firstColumnIsImage)
        {
            if (xWordCloud.WeightedWords != null && useDefault)
            {
                xWordCloud.Visibility = Visibility.Visible;
                xRawDataGridView.Visibility = Visibility.Collapsed;
                xListView.Visibility = Visibility.Collapsed;
            } else
            {
                xWordCloud.Visibility = Visibility.Collapsed;
                if (!hasMultipleColumns)
                {
                    if (firstColumnIsImage)
                    {
                        xRawDataGridView.ItemTemplate = LayoutRoot.Resources["xImageTemplate"] as DataTemplate;
                        xRawDataGridView.ItemContainerStyle = LayoutRoot.Resources["AImageStyle"] as Style;
                    } else
                    {
                        xRawDataGridView.ItemTemplate = LayoutRoot.Resources["xTextTemplate"] as DataTemplate;
                        xRawDataGridView.ItemContainerStyle = LayoutRoot.Resources["TextStyle"] as Style;
                    }
                    xRawDataGridView.Visibility = Visibility.Visible;
                    xListView.Visibility = Visibility.Collapsed;
                }
                else
                {
                    xRawDataGridView.Visibility = Visibility.Collapsed;
                    xListView.Visibility = Visibility.Visible;
                }
            }
        }

        void loadRecordsAsync(List<object> records, AttributeTransformationModel model, bool showScroll)
        {
            var acollection = new RawDataColumn.RawDataColumnModel {
                Alignment = records.First() is string || records.First() is IDEA_common.range.PreProcessedString ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                AttributeTranformationModel = model,
                RendererListView = xListView,
                ColumnWidth =records.First() is string|| records.First() is IDEA_common.range.PreProcessedString ? 200:85,
                ShowScroll = showScroll
            };
            Records.Add(acollection);

            if (records != null)
                if (model.AttributeModel.VisualizationHints.FirstOrDefault() == IDEA_common.catalog.VisualizationHint.Image)
                {
                    var dataset = model.AttributeModel.OriginModel.Name;
                    var hostname = MainViewController.Instance.MainModel.Hostname;
                    string prepend = hostname + "/api/rawdata/" + dataset + "/" + model.AttributeModel.RawName+"/";
#pragma warning disable CS4014
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync( CoreDispatcherPriority.Low,
                    () =>  {
                        foreach (var val in records)
                        {
                            var x = JsonConvert.DeserializeObject<PreProcessedString>(val.ToString());
                            acollection.Data.Add(new MyUri(prepend +  x.Value));
                        }
                    });
#pragma warning restore CS4014

                }
                else
                {
#pragma warning disable CS4014
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low,
                        () => records.ForEach((r) => acollection.Data.Add(r)) );
#pragma warning restore CS4014
                }
        }

        void render() 
        {
            var model = (DataContext as RawDataOperationViewModel);
            configureLayout(true, model.RawDataOperationModel.AttributeTransformationModelParameters[0].AttributeModel.VisualizationHints.FirstOrDefault() == IDEA_common.catalog.VisualizationHint.Image);
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

        IGeometry AttributeViewModelEventHandler.BoundsGeometry
        {
            get
            {
                return this.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            }
        }

        AttributeModel AttributeViewModelEventHandler.CurrentAttributeModel => throw new NotImplementedException();

        public bool Consume(InkStroke inkStroke)
        {
            return false;
        }

        void AttributeViewModelEventHandler.AttributeViewModelMoved(AttributeViewModel sender, AttributeViewModelEventArgs e, bool overElement)
        {
        }

        void AttributeViewModelEventHandler.AttributeViewModelDropped(AttributeViewModel sender, AttributeViewModelEventArgs e, bool overElement)
        {
            if (overElement)
            {
                var model = (DataContext as RawDataOperationViewModel);
                model.ForceDrop(sender);
            }
        }
    }
}
