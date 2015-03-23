﻿using PanoramicData.controller.data;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class PlotRenderer : Renderer, AttributeViewModelEventHandler
    {
        private PlotRendererContentProvider _PlotRendererContentProvider = new PlotRendererContentProvider();

        private List<Vec> _clusterCenters = new List<Vec>();
        private List<Vec> _samples = new List<Vec>();

        private bool _isXHighlighted = false;
        private bool _isYHighlighted = false;

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
                render();
               // yAttributeView.Width = 250;
             //   yAttributeView.Height = 50;
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
                    BorderThicknes = new Thickness(0, 0, 0, 4)
                };
            } 
            if (queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).Any())
            {
                var yAom = queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).First();
                yAttributeView.DataContext = new AttributeViewModel((DataContext as VisualizationViewModel), yAom)
                {
                    IsShadow = false,
                    BorderThicknes = new Thickness(0,0,4,0),
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
            List<BarDataPoint> barDataPoints = new List<BarDataPoint>();
            foreach (var queryResultItemModel in resultModel.QueryResultItemModels)
            {
                BarDataPoint point = new BarDataPoint()
                {
                    X = double.Parse(queryResultItemModel.VisualizationResultValues[VisualizationResult.X].Value.ToString()),
                    Y = double.Parse(queryResultItemModel.VisualizationResultValues[VisualizationResult.Y].Value.ToString()),
                };

                barDataPoints.Add(point);
            }
            _PlotRendererContentProvider.BarDataPoints = barDataPoints;
            render();
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
            // turn both off 
            var xBounds = xAttributeViewBorder.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (xBounds.Intersects(e.Bounds.GetPolygon()))
            {
                if (!_isXHighlighted)
                {
                    toggle(true, xAttributeTextBlock, xAttributeViewBorder);
                    _isXHighlighted = true;
                } 
            }
            else
            {
                if (_isXHighlighted)
                {
                    toggle(false, xAttributeTextBlock, xAttributeViewBorder);
                    _isXHighlighted = false;
                } 
            }

            var yBounds = yAttributeViewBorder.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (yBounds.Intersects(e.Bounds.GetPolygon()))
            {
                if (!_isYHighlighted)
                {
                    toggle(true, yAttributeTextBlock, yAttributeViewBorder);
                    _isYHighlighted = true;
                }
            }
            else
            {
                if (_isYHighlighted)
                {
                    toggle(false, yAttributeTextBlock, yAttributeViewBorder);
                    _isYHighlighted = false;
                }
            }
        }

        public void AttributeViewModelDropped(AttributeViewModel sender, AttributeViewModelEventArgs e, bool overElement)
        {
            // turn both off 
            toggle(false, xAttributeTextBlock, xAttributeViewBorder);
            _isXHighlighted = false;
            toggle(false, yAttributeTextBlock, yAttributeViewBorder);
            _isYHighlighted = false;


            QueryModel qModel = (DataContext as VisualizationViewModel).QueryModel;

            var xBounds = xAttributeViewBorder.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (xBounds.Intersects(e.Bounds.GetPolygon()))
            {
                qModel.RemoveAttributeOperationModel(qModel.GetFunctionAttributeOperationModel(AttributeFunction.X).First());
                qModel.AddFunctionAttributeOperationModel(AttributeFunction.X, e.AttributeOperationModel);
            }

            var yBounds = yAttributeViewBorder.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (yBounds.Intersects(e.Bounds.GetPolygon()))
            {
                qModel.RemoveAttributeOperationModel(qModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).First());
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
        private float _leftOffset = 30;
        private float _rightOffset = 30;
        private float _topOffset = 30;
        private float _bottomtOffset = 30;

        public List<BarDataPoint> BarDataPoints { get; set; }
        
        public override void Clear(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(new Color(230, 230, 230));
        }

        public override void Draw(D2D.DeviceContext d2dDeviceContext)
        {
            if (BarDataPoints != null && BarDataPoints.Count > 0)
            {
                float deviceWidth = (float)(d2dDeviceContext.Size.Width - _leftOffset - _rightOffset);
                float deviceHeight = (float)(d2dDeviceContext.Size.Height - _topOffset - _bottomtOffset);

                float minX = (float)(BarDataPoints.Min(dp => dp.X));
                float minY = (float)(BarDataPoints.Min(dp => dp.Y));
                float maxX = (float)(BarDataPoints.Max(dp => dp.X));
                float maxY = (float)(BarDataPoints.Max(dp => dp.Y));

                float xScale = maxX - minX;
                float yScale = maxY - minY;

                bool flipY = true;
                var color = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 0f, 0f, 1f));

                foreach (var dp in BarDataPoints)
                {
                    float x = (float)((dp.X - minX) / xScale) * deviceWidth;
                    float y = (float)((dp.Y - minY) / yScale) * deviceHeight;

                    RectangleF rect = new RectangleF(
                        x + _leftOffset,
                        flipY ? deviceHeight - y + _topOffset : y,
                        5,
                        5);

                    d2dDeviceContext.FillRectangle(rect, color);
                }
                color.Dispose();
            }
        }
        public override void Load(D2D.DeviceContext d2dDeviceContext, DisposeCollector disposeCollector, DW.Factory1 dwFactory)
        {
        }
    }

    public class BarDataPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
