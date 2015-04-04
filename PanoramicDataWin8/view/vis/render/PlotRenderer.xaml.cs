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
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.vis.menu;
using PanoramicDataWin8.controller.data.sim;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class PlotRenderer : Renderer, AttributeViewModelEventHandler
    {
        private MenuViewModel _menuViewModel = null;
        private MenuView _menuView = null;

        private PlotRendererContentProvider _plotRendererContentProvider = new PlotRendererContentProvider();

        public PlotRenderer()
        {
            this.InitializeComponent();

            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += PlotRenderer_Loaded;
            AttributeView.AttributeViewModelTapped += AttributeView_AttributeViewModelTapped;
        }

        void PlotRenderer_Loaded(object sender, RoutedEventArgs e)
        {
            _plotRendererContentProvider.CompositionScaleX = dxSurface.CompositionScaleX;
            _plotRendererContentProvider.CompositionScaleY = dxSurface.CompositionScaleY;
            dxSurface.ContentProvider = _plotRendererContentProvider;
        }

        public override void Dispose()
        {
            base.Dispose();
            AttributeView.AttributeViewModelTapped -= AttributeView_AttributeViewModelTapped;
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
                mainLabel.Text = (DataContext as VisualizationViewModel).QueryModel.VisualizationType.ToString();
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
            updateProgressAndNullVisualization();
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
            if (e.PropertyName == model.GetPropertyName(() => model.Size) ||
                e.PropertyName == model.GetPropertyName(() => model.Position))
            {
                setMenuViewModelAnkerPosition();
            }      
        }
        
        private void populateHeaders()
        {
            removeMenu();
            VisualizationViewModel visModel = (DataContext as VisualizationViewModel);
            QueryModel queryModel = visModel.QueryModel;
            if (queryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).Any())
            {
                var xAom = queryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).First();
                xAttributeView.DataContext = new AttributeViewModel((DataContext as VisualizationViewModel), xAom)
                {
                    IsShadow = false,
                    BorderThicknes = new Thickness(0, 0, 0, 4),
                    Size = new Vec(visModel.Size.X - 54, 54),
                    AttachmentOrientation = AttachmentOrientation.Top
                };
            }
            else
            {
                xAttributeView.DataContext = new AttributeViewModel((DataContext as VisualizationViewModel), null)
                {
                    IsDraggableByPen = false,
                    IsDraggable = false,
                    IsShadow = false,
                    BorderThicknes = new Thickness(0, 0, 0, 4),
                    Size = new Vec(visModel.Size.X - 54, 54),
                    AttachmentOrientation = AttachmentOrientation.Top
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
                    TextAngle = 270,
                    AttachmentOrientation = AttachmentOrientation.Left
                };
            }
            else
            {
                yAttributeView.DataContext = new AttributeViewModel((DataContext as VisualizationViewModel), null)
                {
                    IsDraggableByPen = false,
                    IsDraggable = false,
                    IsShadow = false,
                    BorderThicknes = new Thickness(0, 0, 4, 0),
                    Size = new Vec(54, visModel.Size.Y - 54),
                    TextAngle = 270,
                    AttachmentOrientation = AttachmentOrientation.Left
                };
            }
        }

        private void updateProgressAndNullVisualization()
        {
            QueryResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.QueryResultModel;

            // progress
            double size = 14;
            double thickness = 2;

            double progress = resultModel.Progress;

            tbPercentage1.Text = (progress * 100).ToString("F1") + "%";
            double percentage = Math.Min(progress, 0.999999);
            if (percentage > 0.5)
            {
                arcSegement1.IsLargeArc = true;
            }
            else
            {
                arcSegement1.IsLargeArc = false;
            }
            double angle = 2 * Math.PI * percentage - Math.PI / 2.0;
            double x = size / 2.0;
            double y = size / 2.0;

            Windows.Foundation.Point p = new Windows.Foundation.Point(Math.Cos(angle) * (size / 2.0 - thickness / 2.0) + x, Math.Sin(angle) * (size / 2.0 - thickness / 2.0) + y);
            arcSegement1.Point = p;
            if ((size / 2.0 - thickness / 2.0) > 0.0)
            {
                arcSegement1.Size = new Size((size / 2.0 - thickness / 2.0), (size / 2.0 - thickness / 2.0));
            }

            // null labels
            if (resultModel.XNullCount > 0)
            {
                tbXNull.Visibility = Windows.UI.Xaml.Visibility.Visible;
                tbXNull.Text = "x null values : " + resultModel.XNullCount;
            }
            else
            {
                tbXNull.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (resultModel.YNullCount > 0)
            {
                tbYNull.Visibility = Windows.UI.Xaml.Visibility.Visible;
                tbYNull.Text = "y null values : " + resultModel.YNullCount;
            }
            else
            {
                tbYNull.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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
                animation.Duration = TimeSpan.FromMilliseconds(300);
                animation.From = mainLabel.Opacity;
                animation.To = 0;
                animation.EasingFunction = easingFunction;
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, mainLabel);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                animation = new DoubleAnimation();
                animation.Duration = TimeSpan.FromMilliseconds(300);
                animation.From = dxSurfaceGrid.Opacity;
                animation.To = 1;
                animation.EasingFunction = easingFunction;
                storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, dxSurfaceGrid);
                Storyboard.SetTargetProperty(animation, "Opacity");
                // storyboard.Begin();

                dxSurfaceGrid.Opacity = 1;
                mainLabel.Opacity = 0;

                loadQueryResultItemModels(resultModel);
                render();
            }
            else
            {
                // animate between render canvas and label
                ExponentialEase easingFunction = new ExponentialEase();
                easingFunction.EasingMode = EasingMode.EaseInOut;

                DoubleAnimation animation = new DoubleAnimation();
                animation.Duration = TimeSpan.FromMilliseconds(300);
                animation.From = mainLabel.Opacity;
                animation.To = 1;
                animation.EasingFunction = easingFunction;
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, mainLabel);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                animation = new DoubleAnimation();
                animation.Duration = TimeSpan.FromMilliseconds(300);
                animation.From = dxSurfaceGrid.Opacity;
                animation.To = 0;
                animation.EasingFunction = easingFunction;
                storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, dxSurfaceGrid);
                Storyboard.SetTargetProperty(animation, "Opacity");
                //storyboard.Begin();
                dxSurfaceGrid.Opacity = 0;
                mainLabel.Opacity = 1;
            }
        }

        void loadQueryResultItemModels(QueryResultModel resultModel)
        {
            _plotRendererContentProvider.XAxisType = resultModel.XAxisType;
            _plotRendererContentProvider.YAxisType = resultModel.YAxisType;
            _plotRendererContentProvider.XBinRange = resultModel.XBinRange;
            _plotRendererContentProvider.YBinRange = resultModel.YBinRange;
            List<BinnedDataPoint> binnedDataPoints = new List<BinnedDataPoint>();
            foreach (var queryResultItemModel in resultModel.QueryResultItemModels)
            {
                BinnedDataPoint point = new BinnedDataPoint()
                {
                    MinX = queryResultItemModel.Bin.BinMinX,
                    MinY = queryResultItemModel.Bin.BinMinY,
                    MaxX = queryResultItemModel.Bin.BinMaxX,
                    MaxY = queryResultItemModel.Bin.BinMaxY,
                    Size = queryResultItemModel.Bin.Size,
                    LabelX = queryResultItemModel.Bin.LabelX,
                    LabelY = queryResultItemModel.Bin.LabelY,
                };

                binnedDataPoints.Add(point);
            }
            _plotRendererContentProvider.BinnedDataPoints = binnedDataPoints;
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

        void AttributeView_AttributeViewModelTapped(object sender, EventArgs e)
        {
            var visModel = (DataContext as VisualizationViewModel);
            visModel.ActiveStopwatch.Restart();

            AttributeViewModel model = (sender as AttributeView).DataContext as AttributeViewModel;
            //if (HeaderObjects.Any(ho => ho.AttributeViewModel == model))
            {
                bool createNew = true;
                if (_menuViewModel != null && !_menuViewModel.IsToBeRemoved)
                {
                    createNew = _menuViewModel.AttributeViewModel != model;
                    removeMenu();
                }

                if (createNew)
                {
                    AttributeView attributeView = sender as AttributeView;
                    var menuViewModel = model.CreateMenuViewModel(attributeView.GetBounds(MainViewController.Instance.InkableScene));
                    if (menuViewModel.MenuItemViewModels.Count > 0)
                    {
                        _menuViewModel = menuViewModel;
                        _menuView = new MenuView()
                        {
                            DataContext = _menuViewModel
                        };
                        setMenuViewModelAnkerPosition();
                        MainViewController.Instance.InkableScene.Add(_menuView);
                        _menuViewModel.IsDisplayed = true;
                    }
                }
            }
        }

        private void setMenuViewModelAnkerPosition()
        {
            if (_menuViewModel != null)
            {
                AttributeView attributeView = this.GetDescendantsOfType<AttributeView>().Where(av => av.DataContext == _menuViewModel.AttributeViewModel).FirstOrDefault();

                if (attributeView != null)
                {
                    if (_menuViewModel.IsToBeRemoved)
                    {
                        Rct bounds = attributeView.GetBounds(MainViewController.Instance.InkableScene);
                        foreach (var menuItem in _menuViewModel.MenuItemViewModels)
                        {
                            menuItem.TargetPosition = bounds.TopLeft;
                        }
                    }
                    else
                    {
                        Rct bounds = attributeView.GetBounds(MainViewController.Instance.InkableScene);
                        _menuViewModel.AnkerPosition = bounds.TopLeft;
                    }
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
                if (qModel.GetFunctionAttributeOperationModel(AttributeFunction.X).Any())
                {
                    qModel.RemoveFunctionAttributeOperationModel(AttributeFunction.X, qModel.GetFunctionAttributeOperationModel(AttributeFunction.X).First());
                }
                qModel.AddFunctionAttributeOperationModel(AttributeFunction.X, e.AttributeOperationModel);
            }

            var yBounds = yAttributeView.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (yBounds.Intersects(e.Bounds.GetPolygon()) && !xBounds.Intersects(e.Bounds.GetPolygon()))
            {
                if (qModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).Any())
                {
                    qModel.RemoveFunctionAttributeOperationModel(AttributeFunction.Y, qModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).First());
                }
                qModel.AddFunctionAttributeOperationModel(AttributeFunction.Y, e.AttributeOperationModel);
            }
        }
    }

    public class PlotRendererContentProvider : DXSurfaceContentProvider
    {
        private float _leftOffset = 40;
        private float _rightOffset = 10;
        private float _topOffset = 10;
        private float _bottomtOffset = 45;

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

        public AxisType XAxisType { get; set; }
        public AxisType YAxisType { get; set; }        
        public BinRange XBinRange { get; set; }
        public BinRange YBinRange { get; set; }
        public float CompositionScaleX { get; set; }
        public float CompositionScaleY { get; set; }
        public List<BinnedDataPoint> BinnedDataPoints { get; set; }

        private float toScreenX(float x)
        {
            return ((x - _minX) / _xScale) * (_deviceWidth) + (_leftOffset);
        }
        private float toScreenY(float y)
        {
            float retY = ((y - _minY) / _yScale) * (_deviceHeight);
            return _flipY ? (_deviceHeight) - retY + (_topOffset) : retY + (_topOffset);
        }


        public override void Clear(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(new Color(230, 230, 230));
        }

        public override void Draw(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory)
        {
            var mat = Matrix3x2.Identity;
            mat.ScaleVector = new Vector2(CompositionScaleX, CompositionScaleY);
            d2dDeviceContext.Transform = mat; 

            if (BinnedDataPoints != null && BinnedDataPoints.Count > 0)
            {
                if (MainViewController.Instance.MainModel.GraphRenderOption == GraphRenderOptions.Grid)
                {
                    renderGrid(d2dDeviceContext, dwFactory);
                }
                else if (MainViewController.Instance.MainModel.GraphRenderOption == GraphRenderOptions.Cell)
                {
                    renderCell(d2dDeviceContext, dwFactory);
                }
            }
        }

        private void drawString(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory, float x, float y, string text, 
            bool leftAligned,
            bool horizontallyCentered, bool verticallyCentered)
        {
            var layout = new DW.TextLayout(dwFactory, text, _textFormat, 1000f, 1000f);
            var metrics = layout.Metrics;

            if (horizontallyCentered)
            {
                x -= metrics.Width / 2.0f;
            }
            if (verticallyCentered)
            {
                y -= metrics.Height / 2.0f;
            }
            if (!leftAligned)
            {
                x -= metrics.Width;
            }

            d2dDeviceContext.DrawTextLayout(new Vector2(x, y), layout, _textBrush); 
        }

        private void computeSizesAndRenderLabels(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory, bool renderLines)
        {
            var xLabels = BinnedDataPoints.Select(bin => new { Label = bin.LabelX.TrimTo(20), MinValue = bin.MinX, MaxValue = bin.MaxX }).Distinct().ToList();
            var yLabels = BinnedDataPoints.Select(bin => new { Label = bin.LabelY.TrimTo(20), MinValue = bin.MinY, MaxValue = bin.MaxY }).Distinct().ToList();
            var maxXLabelLength = xLabels.Max(b => b.Label.Length);
            var maxXLabel = xLabels.Where(b => b.Label.Length == maxXLabelLength).First();
            var maxYLabelLength = yLabels.Max(b => b.Label.Length);
            var maxYLabel = yLabels.Where(b => b.Label.Length == maxYLabelLength).First();

            var layoutX = new DW.TextLayout(dwFactory, maxXLabel.Label, _textFormat, 1000f, 1000f);
            var metricsX = layoutX.Metrics;
            var layoutY = new DW.TextLayout(dwFactory, maxYLabel.Label, _textFormat, 1000f, 1000f);
            var metricsY = layoutY.Metrics;

            _leftOffset = Math.Max(10, metricsY.Width + 10 + 20);

            _deviceWidth = (float)(d2dDeviceContext.Size.Width / CompositionScaleX - _leftOffset - _rightOffset);
            _deviceHeight = (float)(d2dDeviceContext.Size.Height / CompositionScaleY - _topOffset - _bottomtOffset);

            _minX = (float)(BinnedDataPoints.Min(dp => dp.MinX));
            _minY = (float)(BinnedDataPoints.Min(dp => dp.MinY));
            _maxX = (float)(BinnedDataPoints.Max(dp => dp.MaxX));
            _maxY = (float)(BinnedDataPoints.Max(dp => dp.MaxY));

            _xScale = _maxX - _minX;
            _yScale = _maxY - _minY;

            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));

            // x labels and grid lines
            int mod = (int)Math.Ceiling(1.0 / (Math.Floor((_deviceWidth / (metricsX.Width + 5))) / xLabels.Count));
            int count = 0;
            foreach (var label in xLabels)
            {
                float yFrom = toScreenY(_minY);
                float yTo = toScreenY(_maxY);
                float xFrom = toScreenX((float)label.MinValue);
                float xTo = toScreenX((float)label.MaxValue);
                bool lastLabel = count + 1 == xLabels.Count;

                if (renderLines)
                {
                    d2dDeviceContext.DrawLine(new Vector2(xFrom, yFrom), new Vector2(xFrom, yTo), white, 0.5f);
                    if (lastLabel)
                    {
                        d2dDeviceContext.DrawLine(new Vector2(xTo, yFrom), new Vector2(xTo, yTo), white, 0.5f);
                    }
                }
                if (count % mod == 0)
                {
                    if (XAxisType == AxisType.Quantitative)
                    {
                        drawString(d2dDeviceContext, dwFactory, xFrom, yFrom + 5, label.Label.ToString(), true, true, false);
                        if (lastLabel)
                        {
                            //drawString(d2dDeviceContext, dwFactory, xTo, yFrom + 5, label.Label.ToString(), true, true, false);
                        }
                    }
                    else
                    {
                        drawString(d2dDeviceContext, dwFactory, xFrom + (xTo - xFrom) / 2.0f, yFrom + 5, label.Label.ToString(), true, true, false);
                    }
                }
                count++;
            }

            // y labels and grid lines
            mod = (int)Math.Ceiling(1.0 / (Math.Floor((_deviceHeight / (metricsY.Height + 5))) / yLabels.Count));
            count = 0;
            foreach (var label in yLabels)
            {
                float xFrom = toScreenX(_minX);
                float xTo = toScreenX(_maxX);
                float yFrom = toScreenY((float)label.MinValue);
                float yTo = toScreenY((float)label.MaxValue); 
                bool lastLabel = count + 1 == yLabels.Count;

                if (renderLines)
                {
                    d2dDeviceContext.DrawLine(new Vector2(xFrom, yFrom), new Vector2(xTo, yFrom), white, 0.5f);
                    if (lastLabel)
                    {
                        d2dDeviceContext.DrawLine(new Vector2(xFrom, yTo), new Vector2(xTo, yTo), white, 0.5f);
                    }
                }
                if (count % mod == 0)
                {
                    if (YAxisType == AxisType.Quantitative)
                    {
                        drawString(d2dDeviceContext, dwFactory, xFrom - 10, yFrom, label.Label.ToString(), false, false, true);
                    }
                    else
                    {
                        drawString(d2dDeviceContext, dwFactory, xFrom - 10, yFrom + (yTo - yFrom) / 2.0f, label.Label.ToString(), false, false, true);
                    }
                }
                count++;
            }
            white.Dispose();
            layoutX.Dispose();
            layoutY.Dispose();
        }

        private void renderCell(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory)
        {
            computeSizesAndRenderLabels(d2dDeviceContext, dwFactory, false);
            if (_deviceHeight < 0 || _deviceWidth < 0)
            {
                return;
            }

            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));

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

                float alpha = 0.1f * (float) Math.Log10(bin.Size) + 1f;
                var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 230, 230, 230), Windows.UI.Color.FromArgb(255, 40, 170, 213), (float)bin.Size);
                var binColor = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(lerpColor.R / 255f, lerpColor.G / 255f, lerpColor.B / 255f, 1f));

                /*if (bin.Size > 0)
                {
                    roundedRect.Rect = new RectangleF(
                      xFrom + ((xTo - xFrom) - w) / 2.0f,
                      yTo + ((yFrom - yTo) - h) / 2.0f,
                      w,
                      h);
                    roundedRect.RadiusX = roundedRect.RadiusY = 4;
                    d2dDeviceContext.FillRoundedRectangle(roundedRect, binColor);
                    //d2dDeviceContext.DrawRoundedRectangle(roundedRect, white, 1f);
                }*/

                roundedRect.Rect = new RectangleF(
                    xFrom,
                    yTo,
                    xTo - xFrom,
                    yFrom - yTo);
                roundedRect.RadiusX = roundedRect.RadiusY = 4;
                d2dDeviceContext.FillRoundedRectangle(roundedRect, binColor);
                if (BinnedDataPoints.Count < 10000)
                {
                    d2dDeviceContext.DrawRoundedRectangle(roundedRect, white, 0.5f);
                }


                binColor.Dispose();
            }
            white.Dispose();
        }

        private void renderGrid(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory)
        {
            computeSizesAndRenderLabels(d2dDeviceContext, dwFactory, BinnedDataPoints.Count < 10000);
            if (_deviceHeight < 0 || _deviceWidth < 0)
            {
                return;
            }

            var binColor = new D2D.SolidColorBrush(d2dDeviceContext, new Color(40, 170, 213));
            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));

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

        public override void Load(D2D.DeviceContext d2dDeviceContext, DisposeCollector disposeCollector, DW.Factory1 dwFactory)
        {
            // reusable structure representing a text font with size and style
            _textFormat = disposeCollector.Collect(new DW.TextFormat(dwFactory, "Abel", SharpDX.DirectWrite.FontWeight.Normal, SharpDX.DirectWrite.FontStyle.Normal, 11f));

            // reusable brush structure
            _textBrush = disposeCollector.Collect(new D2D.SolidColorBrush(d2dDeviceContext, new Color(17, 17, 17)));

            // prebaked text - useful for constant labels as it greatly improves performance
            //_textLayout = disposeCollector.Collect(new DW.TextLayout(dwFactory, "Demo DirectWrite text here.", _textFormat, 100f, 100f));
        }
    }

    public class BinnedDataPoint
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public double Size { get; set; }
        public string LabelX { get; set; }
        public string LabelY { get; set; }
    }
}
