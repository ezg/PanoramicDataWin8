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
using IDEA_common.operations.rawdata;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

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
                    tb.MinWidth = 200;
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

        public ObservableCollection<object> Records { get; set; } = new ObservableCollection<object>();
        public RawDataRenderer()
        {
            this.InitializeComponent();
            
            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += PlotRenderer_Loaded;
           // xRawDataView.ItemsSource = Records;
            xRawDataGridView.ItemsSource = Records;
            this.SizeChanged += RawDataRenderer_SizeChanged;
            this.Tapped += RawDataRenderer_Tapped;
        }

        private void RawDataRenderer_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            xRawDataGridView.IsHitTestVisible = true;
            var res = VisualTreeHelper.FindElementsInHostCoordinates(e.GetPosition(null), this);
            xRawDataGridView.IsHitTestVisible = false;
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
        }

        private void RawDataRenderer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var b = new Binding();
            b.Source = this;
            b.Path = new PropertyPath("ActualWidth");
            this.xRawDataGridView.ItemsPanelRoot.SetBinding(WidthProperty, b);
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

            Records.Clear();
            if ((result as RawDataResult)?.Samples != null)
                loadRecordsAsync((result as RawDataResult).Samples);
        }

        void loadRecordsAsync(List<object> records)
        {
            
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
#pragma warning disable CS4014
            dispatcher.RunAsync(
                CoreDispatcherPriority.Low,
                async () =>
                {
                    foreach (var val in records)
                    {
                        Records.Add(val);
                        await Task.Delay(5);
                    }
                });
#pragma warning restore CS4014

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
