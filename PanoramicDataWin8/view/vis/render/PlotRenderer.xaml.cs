using PanoramicData.controller.data;
using PanoramicData.model.data;
using PanoramicData.model.view;
using PanoramicData.utils;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.common;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;
using PanoramicData.controller.view;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.vis.menu;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class PlotRenderer : Renderer, AttributeViewModelEventHandler
    {
        private MenuViewModel _menuViewModel = null;
        private MenuView _menuView = null;

        private PlotRendererContentProvider _PlotRendererContentProvider = new PlotRendererContentProvider();

        private List<Vec> _clusterCenters = new List<Vec>();
        private List<Vec> _samples = new List<Vec>();

        public PlotRenderer()
        {
            this.InitializeComponent();
            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += PlotRenderer_Loaded;
        }

        void PlotRenderer_Loaded(object sender, RoutedEventArgs e)
        {
            dxSurface.ContentProvider = _PlotRendererContentProvider;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (DataContext != null)
            {
                (DataContext as VisualizationViewModel).PropertyChanged -= VisualizationViewModel_PropertyChanged;
                (DataContext as VisualizationViewModel).QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).CollectionChanged -= X_CollectionChanged;
                (DataContext as VisualizationViewModel).QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).CollectionChanged -= Y_CollectionChanged;
                QueryResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.QueryResultModel;
                resultModel.QueryResultModelUpdated -= resultModel_QueryResultModelUpdated;
            }
            dxSurface.Dispose();
        }

        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (DataContext as VisualizationViewModel).PropertyChanged += VisualizationViewModel_PropertyChanged;
                (DataContext as VisualizationViewModel).QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).CollectionChanged += X_CollectionChanged;
                (DataContext as VisualizationViewModel).QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).CollectionChanged += Y_CollectionChanged;
                QueryResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.QueryResultModel;
                resultModel.QueryResultModelUpdated += resultModel_QueryResultModelUpdated;
                populateHeaders();
            }
        }

        void X_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            populateHeaders();
        }
        void Y_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            populateHeaders();
        }

        void resultModel_QueryResultModelUpdated(object sender, EventArgs e)
        {
            populateData();
        }

        void VisualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            VisualizationViewModel model = (DataContext as VisualizationViewModel);
            if (e.PropertyName == model.GetPropertyName(() => model.Size))
            {
                if (xAttributeView.DataContext != null)
                {
                    (xAttributeView.DataContext as AttributeViewModel).Size = new Vec(model.Size.X - 54, 54);
                }
                if (yAttributeView.DataContext != null)
                {
                    (yAttributeView.DataContext as AttributeViewModel).Size = new Vec(54, model.Size.Y - 54);
                }

                render();
            }
        }
        
        private void populateHeaders()
        {
            VisualizationViewModel visModel = (DataContext as VisualizationViewModel);
            QueryModel queryModel = visModel.QueryModel;
            if (queryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).Any())
            {
                var xAom = queryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).First();
                xAttributeView.DataContext = new AttributeViewModel((DataContext as VisualizationViewModel), xAom)
                {
                    IsShadow = false,
                    BorderThicknes = new Thickness(0, 0, 0, 4),
                    Size = new Vec(visModel.Size.X - 54, 54)
                };
            } 
            if (queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).Any())
            {
                var yAom = queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).First();
                yAttributeView.DataContext = new AttributeViewModel((DataContext as VisualizationViewModel), yAom)
                {
                    IsShadow = false,
                    BorderThicknes = new Thickness(0,0,4,0),
                    Size = new Vec(54, visModel.Size.Y - 54),
                    TextAngle = 270
                };
            }
        }
        
        private void populateData()
        {
            QueryResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.QueryResultModel;
            if (resultModel.QueryResultItemModels.Count > 0)
            {
                ExponentialEase easingFunction = new ExponentialEase();
                easingFunction.EasingMode = EasingMode.EaseInOut;

                DoubleAnimation animation = new DoubleAnimation();
                animation.From = mainLabel.Opacity;
                animation.To = 0;
                animation.EasingFunction = easingFunction;
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, mainLabel);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                animation = new DoubleAnimation();
                animation.From = dxSurface.Opacity;
                animation.To = 1;
                animation.EasingFunction = easingFunction;
                storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, dxSurface);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                _clusterCenters.Clear();
                _samples.Clear();

                loadQueryResultItemModels(resultModel);
                render();
            }
            else
            {
                // animate between render canvas and label
                ExponentialEase easingFunction = new ExponentialEase();
                easingFunction.EasingMode = EasingMode.EaseInOut;

                DoubleAnimation animation = new DoubleAnimation();
                animation.From = mainLabel.Opacity;
                animation.To = 1;
                animation.EasingFunction = easingFunction;
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, mainLabel);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                animation = new DoubleAnimation();
                animation.From = dxSurface.Opacity;
                animation.To = 0;
                animation.EasingFunction = easingFunction;
                storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, dxSurface);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();
            }
        }

        void loadQueryResultItemModels(QueryResultModel resultModel)
        {
            List<BinnedDataPoint> binnedDataPoints = new List<BinnedDataPoint>();
            foreach (var queryResultItemModel in resultModel.QueryResultItemModels)
            {
                BinnedDataPoint point = new BinnedDataPoint()
                {
                    MinX = queryResultItemModel.BinMinXValue,
                    MinY = queryResultItemModel.BinMinYValue,
                    MaxX = queryResultItemModel.BinMaxXValue,
                    MaxY = queryResultItemModel.BinMaxYValue,
                    Size = queryResultItemModel.BinSize
                };

                binnedDataPoints.Add(point);
            }
            _PlotRendererContentProvider.BinnedDataPoints = binnedDataPoints;
            render();
        }

        void removeMenu()
        {

            if (_menuViewModel != null)
            {
                AttributeView attributeView = this.GetDescendantsOfType<AttributeView>().Where(av => av.DataContext == _menuViewModel.AttributeViewModel).FirstOrDefault();
                if (attributeView != null)
                {
                    Rct bounds = attributeView.GetBounds(MainViewController.Instance.InkableScene);
                    foreach (var menuItem in _menuViewModel.MenuItemViewModels)
                    {
                        menuItem.TargetPosition = bounds.TopLeft;
                    }
                    _menuViewModel.IsToBeRemoved = true;
                    _menuViewModel.IsDisplayed = false;
                }
            }
        }

        void render()
        {
            dxSurface.Redraw();
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                return this.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            }
        }

        public void AttributeViewModelMoved(AttributeViewModel sender, AttributeViewModelEventArgs e, bool overElement)
        {
            var xBounds = xAttributeView.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (xBounds.Intersects(e.Bounds.GetPolygon()))
            {
                if (xAttributeView.DataContext != null && !(xAttributeView.DataContext as AttributeViewModel).IsHighlighted)
                {
                    (xAttributeView.DataContext as AttributeViewModel).IsHighlighted = true;
                }
            }
            else
            {
                if (xAttributeView.DataContext != null && (xAttributeView.DataContext as AttributeViewModel).IsHighlighted)
                {
                    (xAttributeView.DataContext as AttributeViewModel).IsHighlighted = false;
                }
            }

            var yBounds = yAttributeView.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (yBounds.Intersects(e.Bounds.GetPolygon()) && !xBounds.Intersects(e.Bounds.GetPolygon()))
            {
                if (yAttributeView.DataContext != null && !(yAttributeView.DataContext as AttributeViewModel).IsHighlighted)
                {
                    (yAttributeView.DataContext as AttributeViewModel).IsHighlighted = true;
                }
            }
            else
            {
                if (yAttributeView.DataContext != null && (yAttributeView.DataContext as AttributeViewModel).IsHighlighted)
                {
                    (yAttributeView.DataContext as AttributeViewModel).IsHighlighted = false;
                }
            }
        }

        public void AttributeViewModelDropped(AttributeViewModel sender, AttributeViewModelEventArgs e, bool overElement)
        {
            // turn both off 
            if (xAttributeView.DataContext != null)
            {
                (xAttributeView.DataContext as AttributeViewModel).IsHighlighted = false;
            }
            if (yAttributeView.DataContext != null)
            {
                (yAttributeView.DataContext as AttributeViewModel).IsHighlighted = false;
            }


            QueryModel qModel = (DataContext as VisualizationViewModel).QueryModel;

            var xBounds = xAttributeView.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (xBounds.Intersects(e.Bounds.GetPolygon()))
            {
                qModel.RemoveFunctionAttributeOperationModel(AttributeFunction.X, qModel.GetFunctionAttributeOperationModel(AttributeFunction.X).First());
                qModel.AddFunctionAttributeOperationModel(AttributeFunction.X, e.AttributeOperationModel);
            }

            var yBounds = yAttributeView.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (yBounds.Intersects(e.Bounds.GetPolygon()) && !xBounds.Intersects(e.Bounds.GetPolygon()))
            {
                qModel.RemoveFunctionAttributeOperationModel(AttributeFunction.Y, qModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).First());
                qModel.AddFunctionAttributeOperationModel(AttributeFunction.Y, e.AttributeOperationModel);
            }
        }

        void toggle(bool isHighlighted, TextBlock textBlock, Border border)
        {
            ExponentialEase easingFunction = new ExponentialEase();
            easingFunction.EasingMode = EasingMode.EaseInOut;

            ColorAnimation backgroundAnimation = new ColorAnimation();
            backgroundAnimation.EasingFunction = easingFunction;
            backgroundAnimation.Duration = TimeSpan.FromMilliseconds(300);
            backgroundAnimation.From = (border.Background as SolidColorBrush).Color;

            if (isHighlighted)
            {
                backgroundAnimation.To = (Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush).Color;
                textBlock.Foreground = (Application.Current.Resources.MergedDictionaries[0]["backgroundBrush"] as SolidColorBrush);
            }
            else
            {
                backgroundAnimation.To = (Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush).Color;
                textBlock.Foreground = (Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush);
            }
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(backgroundAnimation);
            Storyboard.SetTarget(backgroundAnimation, border);
            Storyboard.SetTargetProperty(backgroundAnimation, "(Border.Background).(SolidColorBrush.Color)");
            //Storyboard.SetTargetProperty(foregroundAnimation, "(TextBlock.Foreground).Color");

            storyboard.Begin();
        }
    }

    public class PlotRendererContentProvider : DXSurfaceContentProvider
    {
        private float _leftOffset = 40;
        private float _rightOffset = 10;
        private float _topOffset = 10;
        private float _bottomtOffset = 40;

        private float _deviceWidth = 0;
        private float _deviceHeight = 0;
        private float _xScale = 0;
        private float _yScale = 0;
        private bool _flipY = true;
        private float _minX = 0;
        private float _minY = 0;
        private float _maxX = 0;
        private float _maxY = 0;

        private D2D.Brush _textBrush;
        private DW.TextFormat _textFormat;


        public List<BinnedDataPoint> BinnedDataPoints { get; set; }

        private float toScreenX(float x)
        {
            return ((x - _minX) / _xScale) * _deviceWidth + _leftOffset;
        }
        private float toScreenY(float y)
        {
            float retY = ((y - _minY) / _yScale) * _deviceHeight;
            return _flipY ? _deviceHeight - retY + _topOffset : retY + _topOffset;
        }


        public override void Clear(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(new Color(230, 230, 230));
        }

        public override void Draw(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory)
        {

            if (BinnedDataPoints != null && BinnedDataPoints.Count > 0)
            {
                _deviceWidth = (float)(d2dDeviceContext.Size.Width - _leftOffset - _rightOffset);
                _deviceHeight = (float)(d2dDeviceContext.Size.Height - _topOffset - _bottomtOffset);

                if (_deviceHeight < 0 || _deviceWidth < 0)
                {
                    return;
                }

                _minX = (float)(BinnedDataPoints.Min(dp => dp.MinX));
                _minY = (float)(BinnedDataPoints.Min(dp => dp.MinY));
                _maxX = (float)(BinnedDataPoints.Max(dp => dp.MaxX));
                _maxY = (float)(BinnedDataPoints.Max(dp => dp.MaxY));

                _xScale = _maxX - _minX;
                _yScale = _maxY - _minY;

                var binColor = new D2D.SolidColorBrush(d2dDeviceContext, new Color(40, 170, 213));
                var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));

                // draw grid and tickmarks
                // x axis
                /*float[] xExtent = getLinearTicks(minX, maxX, 10);
                for (float t = xExtent[0]; t < xExtent[1]; t += xExtent[2])
                {
                    float yFrom = (float)((minY - minY) / yScale) * deviceHeight +_topOffset;
                    float yTo = (float)((maxY - minY) / yScale) * deviceHeight + _topOffset;
                    float x = ((t - minX) / xScale) * deviceWidth + _leftOffset;
                    d2dDeviceContext.DrawLine(new Vector2(x, yFrom), new Vector2(x, yTo), white, 0.5f);

                    var layout = new DW.TextLayout(dwFactory, t.ToString() , _textFormat, 1000f, 1000f);
                    var metrics = layout.Metrics;
                    d2dDeviceContext.DrawTextLayout(new Vector2(x - metrics.Width / 2.0f, yTo + 5), layout, _textBrush);
                    //d2dDeviceContext.DrawRectangle(new RectangleF(15, 15 + i * 25, tt.Width, tt.Height), _textBrush);
                    layout.Dispose();
                }*/

                List<double> xLabel = BinnedDataPoints.Select(bin => bin.MinX).Distinct().ToList();
                var maxLength = xLabel.Max(b => b.ToString().Length);
                var maxLengthText = BinnedDataPoints.Where(bin => bin.MinX.ToString().Length == maxLength).Select(bin => bin.MinX.ToString()).First();
                var layout = new DW.TextLayout(dwFactory, maxLengthText, _textFormat, 1000f, 1000f);
                var metrics = layout.Metrics;

                int mod = (int) Math.Ceiling(1.0 / (Math.Floor((_deviceWidth / metrics.Width)) / xLabel.Count));

                int count = 0;
                foreach (var t in xLabel)
                {
                    float yFrom = toScreenY(_minY);
                    float yTo = toScreenY(_maxY);
                    float x = toScreenX((float)t);
                    d2dDeviceContext.DrawLine(new Vector2(x, yFrom), new Vector2(x, yTo), white, 0.5f);
                    if (count % mod == 0)
                    {
                        layout = new DW.TextLayout(dwFactory, t.ToString(), _textFormat, 1000f, 1000f);
                        metrics = layout.Metrics;
                        d2dDeviceContext.DrawTextLayout(new Vector2(x - metrics.Width / 2.0f, yFrom + 5), layout, _textBrush);
                        layout.Dispose();
                    }
                    count++;
                }

                List<double> yLabel = BinnedDataPoints.Select(bin => bin.MinY).Distinct().ToList();
                maxLength = yLabel.Max(b => b.ToString().Length);
                maxLengthText = BinnedDataPoints.Where(bin => bin.MinY.ToString().Length == maxLength).Select(bin => bin.MinY.ToString()).First();
                layout = new DW.TextLayout(dwFactory, maxLengthText, _textFormat, 1000f, 1000f);
                metrics = layout.Metrics;

                mod = (int)Math.Ceiling(1.0 / (Math.Floor((_deviceHeight / metrics.Height)) / yLabel.Count));

                count = 0;
                foreach (var t in yLabel)
                {
                    
                    float xFrom = toScreenX(_minX);
                    float xTo = toScreenX(_maxX);
                    float y = toScreenY((float)t);
                    d2dDeviceContext.DrawLine(new Vector2(xFrom, y), new Vector2(xTo, y), white, 0.5f);
                    if (count % mod == 0)
                    {
                        layout = new DW.TextLayout(dwFactory, t.ToString(), _textFormat, 1000f, 1000f);
                        metrics = layout.Metrics;
                        d2dDeviceContext.DrawTextLayout(new Vector2(xFrom - 10 - metrics.Width, y - metrics.Height / 2.0f), layout, _textBrush);
                        layout.Dispose();
                    }
                    count++;
                }

/*
                // y axis
                float[] yExtent = getLinearTicks(minY, maxY, 10);
                for (float t = yExtent[0]; t < yExtent[1]; t += yExtent[2])
                {
                    float xFrom = (float)((minX) / xScale) * deviceWidth + _leftOffset;
                    float xTo = (float)((maxX) / xScale) * deviceWidth + _topOffset;
                    float y = (t / yScale) * deviceHeight + _topOffset;
                    //d2dDeviceContext.DrawLine(new Vector2(xFrom, y), new Vector2(xTo, y), white, 1);

                }
*/
                // draw data
                foreach (var bin in BinnedDataPoints)
                {
                    var roundedRect = new D2D.RoundedRectangle();
                    float xFrom = toScreenX((float)bin.MinX);
                    float yFrom = toScreenY((float)bin.MinY);
                    float xTo = toScreenX((float)bin.MaxX);
                    float yTo = toScreenY((float)bin.MaxY);
                    float w = (float)Math.Max((xTo - xFrom) * (float)bin.Size, 5.0);
                    float h = (float)Math.Max((yFrom - yTo) * (float)bin.Size, 5.0);

                    roundedRect.Rect = new RectangleF(
                        xFrom + ((xTo - xFrom) - w) / 2.0f,
                        yTo + ((yFrom - yTo) - h) / 2.0f,
                        w,
                        h);
                    roundedRect.RadiusX = roundedRect.RadiusY = 4;

                    if (bin.Size > 0)
                    {
                        d2dDeviceContext.FillRoundedRectangle(roundedRect, binColor);
                        d2dDeviceContext.DrawRoundedRectangle(roundedRect, white, 1f);

                    }
                }
                binColor.Dispose();
                white.Dispose();
            }
        }
        public override void Load(D2D.DeviceContext d2dDeviceContext, DisposeCollector disposeCollector, DW.Factory1 dwFactory)
        {
            // reusable structure representing a text font with size and style
            _textFormat = disposeCollector.Collect(new DW.TextFormat(dwFactory, "Abel", SharpDX.DirectWrite.FontWeight.Normal, SharpDX.DirectWrite.FontStyle.Normal, 11f));

            // reusable brush structure
            _textBrush = disposeCollector.Collect(new D2D.SolidColorBrush(d2dDeviceContext, new Color(17, 17, 17)));

            // prebaked text - useful for constant labels as it greatly improves performance
            //_textLayout = disposeCollector.Collect(new DW.TextLayout(dwFactory, "Demo DirectWrite text here.", _textFormat, 100f, 100f));
        }

        private float[] getLinearTicks(double min, double max, double m)
        {
            double span = max - min;

            double step = Math.Pow(10, Math.Floor(Math.Log10(span / m)));
            double err = m / span * step;

            if (err <= .15) 
              step *= 10;
            else if (err <= .35)
              step *= 5;
            else if (err <= .75)
              step *= 2;

            float[] ret = new float[3];
            ret[0] = (float)(Math.Ceiling(min / step) * step);
            ret[1] = (float)(Math.Floor(max / step) * step + step * .5);
            ret[2] = (float)step;

            return ret;
        }
    }

    public class BinnedDataPoint
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public double Size { get; set; }
    }
}
