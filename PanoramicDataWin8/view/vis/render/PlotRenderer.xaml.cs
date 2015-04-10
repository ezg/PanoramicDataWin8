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
using PanoramicData.controller.view;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.vis.menu;
using PanoramicDataWin8.controller.data.sim;
using PanoramicDataWin8.controller.data.sim.binrange;

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
            VisualizationViewModel model = (DataContext as VisualizationViewModel);
            _plotRendererContentProvider.UpdateData(resultModel,
                model.QueryModel.Clone(),
                model.QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).FirstOrDefault(),
                model.QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).FirstOrDefault());
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
}
