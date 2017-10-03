using PanoramicDataWin8.utils;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public class ObjectToFrameworkElementConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            var g = new Grid();
            if (value is string)
            {
                var tb = new TextBlock();
                tb.FontFamily = FontFamily.XamlAutoFontFamily;
                tb.FontSize = 14;
                tb.Foreground = new SolidColorBrush(Colors.Black);
                tb.Text = value as string;
                tb.Width = 100;
                tb.Height = 25;
                g.Children.Add(tb);
                g.Background = new SolidColorBrush(Colors.LightGray);
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

        public ObservableCollection<string> Records { get; set; } = new ObservableCollection<string>();
        public RawDataRenderer()
        {
            this.InitializeComponent();

            // dxSurface.ContentProvider = _plotRendererContentProvider;
            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += PlotRenderer_Loaded;
            xRawDataView.ItemsSource = Records;
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
                    if (!operationModel.GetAttributeUsageTransformationModel(AttributeUsage.X).Any())
                    {
                    }
                }
            }
        }
        void OperationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RawDataOperationModel operationModel = (RawDataOperationModel)((OperationViewModel)DataContext).OperationModel;
            if (e.PropertyName == operationModel.GetPropertyName(() => operationModel.Result))
            {
                var result = (DataContext as RawDataOperationViewModel).OperationModel.Result;
                if (result != null)
                {
                    loadResult(result);
                    render();
                }
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
            var model = (DataContext as RawDataOperationViewModel);
            var clone = (RawDataOperationModel)model.OperationModel.ResultCauserClone;
            var xIom = clone.GetAttributeUsageTransformationModel(AttributeUsage.X).FirstOrDefault();
            AttributeTransformationModel valueIom = null;

            if (clone.GetAttributeUsageTransformationModel(AttributeUsage.Value).Any())
            {
                valueIom = clone.GetAttributeUsageTransformationModel(AttributeUsage.Value).First();
            }
            else if (clone.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue).Any())
            {
                valueIom = clone.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue).First();
            }
            //_plotRendererContentProvider.UpdateFilterModels(model.HistogramOperationModel.FilterModels.ToList());
            //_plotRendererContentProvider.UpdateData(result, model.HistogramOperationModel.IncludeDistribution,
            //    model.HistogramOperationModel.BrushColors,
            //    xIom, yIom, valueIom, 30);

            Records.Add("Hello");
            Records.Add("I Must");
            Records.Add("Be Going");
            Records.Add("Hello");
            Records.Add("I Must");
            Records.Add("Be Going");
            Records.Add("Hello");
            Records.Add("I Must");
            Records.Add("Be Going");
            Records.Add("Hello");
            Records.Add("I Must");
            Records.Add("Be Going");
            Records.Add("Hello");
            Records.Add("I Must");
            Records.Add("Be Going");
            Records.Add("Hello");
            Records.Add("I Must");
            Records.Add("Be Going");
            Records.Add("Hello");
            Records.Add("I Must");
            Records.Add("Be Going");
            Records.Add("Hello");
            Records.Add("I Must");
            Records.Add("Be Going");
            Records.Add("Hello");
            Records.Add("I Must");
            Records.Add("Be Going");
            Records.Add("Hello");
            Records.Add("I Must");
            Records.Add("Be Going");
            Records.Add("Hello");
            Records.Add("I Must");
            Records.Add("Be Going");
            Records.Add("Hello");
            Records.Add("I Must");
            Records.Add("Be Going");
        }
        void render(bool sizeChanged = false)
        {
            //viewBox.Visibility = Visibility.Collapsed;
            //dxSurface?.Redraw();
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
