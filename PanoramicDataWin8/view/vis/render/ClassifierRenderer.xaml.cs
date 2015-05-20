using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using GeoAPI.Geometries;
using PanoramicDataWin8.controller.data;
using PanoramicDataWin8.controller.data.virt;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.common;
using PanoramicDataWin8.view.vis.menu;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class ClassifierRenderer : Renderer
    {
        private MenuViewModel _menuViewModel = null;
        private MenuView _menuView = null;

        private ClassifierRendererContentProvider _classifierRendererContentProvider = new ClassifierRendererContentProvider();

        public ClassifierRenderer()
        {
            this.InitializeComponent();

            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += PlotRenderer_Loaded;
            InputFieldView.InputFieldViewModelTapped += InputFieldViewInputFieldViewModelTapped;
        }

        void PlotRenderer_Loaded(object sender, RoutedEventArgs e)
        {
            _classifierRendererContentProvider.CompositionScaleX = dxSurface.CompositionScaleX;
            _classifierRendererContentProvider.CompositionScaleY = dxSurface.CompositionScaleY;
            dxSurface.ContentProvider = _classifierRendererContentProvider;
        }

        public override void Dispose()
        {
            base.Dispose();
            InputFieldView.InputFieldViewModelTapped -= InputFieldViewInputFieldViewModelTapped;
            if (DataContext != null)
            {
                ((VisualizationViewModel) DataContext).PropertyChanged -= VisualizationViewModel_PropertyChanged;
                ((VisualizationViewModel) DataContext).QueryModel.QueryModelUpdated -= QueryModel_QueryModelUpdated;
                ResultModel resultModel = ((VisualizationViewModel) DataContext).QueryModel.ResultModel;
                resultModel.ResultModelUpdated -= resultModel_ResultModelUpdated;
            }
            dxSurface.Dispose();
        }

        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                ((VisualizationViewModel) DataContext).PropertyChanged += VisualizationViewModel_PropertyChanged;
                ((VisualizationViewModel) DataContext).QueryModel.QueryModelUpdated += QueryModel_QueryModelUpdated;
                ResultModel resultModel = ((VisualizationViewModel) DataContext).QueryModel.ResultModel;
                resultModel.ResultModelUpdated += resultModel_ResultModelUpdated;
                mainLabel.Text = ((VisualizationViewModel) DataContext).QueryModel.VisualizationType.ToString();
                mainLabel.Text = ((VisualizationViewModel) DataContext).QueryModel.JobType.ToString();
                tbType.Text = ((VisualizationViewModel) DataContext).QueryModel.JobType.ToString();
            }
        }

        void QueryModel_QueryModelUpdated(object sender, QueryModelUpdatedEventArgs e)
        {
            QueryModel queryModel = ((VisualizationViewModel) DataContext).QueryModel;
            mainLabel.Text = queryModel.JobType.ToString();
            tbType.Text = queryModel.JobType.ToString();
        }

        void resultModel_ResultModelUpdated(object sender, EventArgs e)
        {
            populateData();
            updateProgressAndNullVisualization();
        }

        void VisualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            VisualizationViewModel model = ((VisualizationViewModel) DataContext);
            if (e.PropertyName == model.GetPropertyName(() => model.Size))
            {
                render();
            }
            if (e.PropertyName == model.GetPropertyName(() => model.Size) ||
                e.PropertyName == model.GetPropertyName(() => model.Position))
            {
                setMenuViewModelAnkerPosition();
            }
            mainLabel.Text = ((VisualizationViewModel) DataContext).QueryModel.JobType.ToString();
        }

        private void updateProgressAndNullVisualization()
        {
            ResultModel resultModel = ((VisualizationViewModel) DataContext).QueryModel.ResultModel;
            ClassfierResultDescriptionModel descriptionModel = (resultModel.ResultDescriptionModel as ClassfierResultDescriptionModel);

            if (descriptionModel != null)
            {
                tbType.Text = ((VisualizationViewModel)DataContext).QueryModel.JobType.ToString() + " : " + (descriptionModel.F1s[descriptionModel.Labels[0]] * 100.0).ToString("F1") + "%";
            }
            else
            {
                tbType.Text = ((VisualizationViewModel)DataContext).QueryModel.JobType.ToString();
            }

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
        }

        private void populateData()
        {
            ClassfierResultDescriptionModel resultModel = ((VisualizationViewModel)DataContext).QueryModel.ResultModel.ResultDescriptionModel as ClassfierResultDescriptionModel;
            if (resultModel != null)
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
                storyboard.Begin();

                dxSurfaceGrid.Opacity = 1;
                mainLabel.Opacity = 0;

                loadResults(((VisualizationViewModel)DataContext).QueryModel.ResultModel);
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
                storyboard.Begin();
                dxSurfaceGrid.Opacity = 0;
                mainLabel.Opacity = 1;
            }
        }

        void loadResults(ResultModel resultModel)
        {
            VisualizationViewModel model = ((VisualizationViewModel) DataContext);
            _classifierRendererContentProvider.UpdateData(resultModel, model.QueryModel.Clone(), 0);

            render(); render();
        }

        void removeMenu()
        {
            if (_menuViewModel != null)
            {
                InputFieldView inputFieldView = this.GetDescendantsOfType<InputFieldView>().Where(av => av.DataContext == _menuViewModel.InputFieldViewModel).FirstOrDefault();
                if (inputFieldView != null)
                {
                    Rct bounds = inputFieldView.GetBounds(MainViewController.Instance.InkableScene);
                    foreach (var menuItem in _menuViewModel.MenuItemViewModels)
                    {
                        menuItem.TargetPosition = bounds.TopLeft;
                    }
                    _menuViewModel.IsToBeRemoved = true;
                    _menuViewModel.IsDisplayed = false;
                }
            }
        }

        void InputFieldViewInputFieldViewModelTapped(object sender, EventArgs e)
        {
            var visModel = ((VisualizationViewModel) DataContext);
            visModel.ActiveStopwatch.Restart();

            InputFieldViewModel model = (sender as InputFieldView).DataContext as InputFieldViewModel;
            //if (HeaderObjects.Any(ho => ho.InputFieldViewModel == model))
            {
                bool createNew = true;
                if (_menuViewModel != null && !_menuViewModel.IsToBeRemoved)
                {
                    createNew = _menuViewModel.InputFieldViewModel != model;
                    removeMenu();
                }

                if (createNew)
                {
                    InputFieldView inputFieldView = sender as InputFieldView;
                    var menuViewModel = model.CreateMenuViewModel(inputFieldView.GetBounds(MainViewController.Instance.InkableScene));
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
                InputFieldView inputFieldView = this.GetDescendantsOfType<InputFieldView>().Where(av => av.DataContext == _menuViewModel.InputFieldViewModel).FirstOrDefault();

                if (inputFieldView != null)
                {
                    if (_menuViewModel.IsToBeRemoved)
                    {
                        Rct bounds = inputFieldView.GetBounds(MainViewController.Instance.InkableScene);
                        foreach (var menuItem in _menuViewModel.MenuItemViewModels)
                        {
                            menuItem.TargetPosition = bounds.TopLeft;
                        }
                    }
                    else
                    {
                        Rct bounds = inputFieldView.GetBounds(MainViewController.Instance.InkableScene);
                        _menuViewModel.AnkerPosition = bounds.TopLeft;
                    }
                }
            }
        }

        private List<Windows.Foundation.Point> _selectionPoints = new List<Windows.Foundation.Point>();
        public override void StartSelection(Windows.Foundation.Point point)
        {
            GeneralTransform gt = MainViewController.Instance.InkableScene.TransformToVisual(dxSurface);
            _selectionPoints = new List<Windows.Foundation.Point> { gt.TransformPoint(point) };
        }

        public override void MoveSelection(Windows.Foundation.Point point)
        {
            GeneralTransform gt = MainViewController.Instance.InkableScene.TransformToVisual(dxSurface);
            _selectionPoints.Add(gt.TransformPoint(point));
        }

        public override void EndSelection()
        {
            IList<Vec> convexHull = Convexhull.convexhull(_selectionPoints);
            IGeometry convexHullPoly = convexHull.Select(vec => new Windows.Foundation.Point(vec.X, vec.Y)).ToList().GetPolygon();

            List<FilterModel> hits = new List<FilterModel>();
            
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
    }
}
