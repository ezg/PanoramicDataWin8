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
using IDEA_common.operations;
using PanoramicDataWin8.controller.data;
using PanoramicDataWin8.controller.data.virt;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.common;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.view.vis.menu;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class ClassifierRenderer : Renderer, IScribbable
    {
        private MenuViewModel _menuViewModel = null;
        private MenuView _menuView = null;
        private int _currentViewIndex = 0;

        private ClassifierRendererContentProvider _classifierRendererContentProvider = new ClassifierRendererContentProvider();

        public ClassifierRenderer()
        {
            this.InitializeComponent();

            dxSurface.ContentProvider = _classifierRendererContentProvider;
            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += PlotRenderer_Loaded;
            InputFieldView.InputFieldViewModelTapped += InputFieldViewInputFieldViewModelTapped;
        }

        void PlotRenderer_Loaded(object sender, RoutedEventArgs e)
        {
            _classifierRendererContentProvider.CompositionScaleX = dxSurface.CompositionScaleX;
            _classifierRendererContentProvider.CompositionScaleY = dxSurface.CompositionScaleY;
        }

        public override void Dispose()
        {
            base.Dispose();
            InputFieldView.InputFieldViewModelTapped -= InputFieldViewInputFieldViewModelTapped;
            if (DataContext != null)
            {
                ((ClassificationOperationViewModel) DataContext).PropertyChanged -= VisualizationViewModel_PropertyChanged;
                ((ClassificationOperationViewModel) DataContext).ClassificationOperationModel.OperationModelUpdated -= ClassificationOperationModel_OperationModelUpdated;
                ((ClassificationOperationViewModel)DataContext).ClassificationOperationModel.PropertyChanged -= QueryModel_PropertyChanged;
            }
            dxSurface.Dispose();
        }

        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                ((ClassificationOperationViewModel) DataContext).PropertyChanged += VisualizationViewModel_PropertyChanged;
                ((ClassificationOperationViewModel)DataContext).ClassificationOperationModel.OperationModelUpdated += ClassificationOperationModel_OperationModelUpdated;
                //((ClassificationOperationViewModel)DataContext).ClassificationOperationModel.RequestRender += PlotRenderer_RequestRender;
                ((ClassificationOperationViewModel)DataContext).ClassificationOperationModel.PropertyChanged += QueryModel_PropertyChanged;
                //mainLabel.Text = ((ClassificationOperationViewModel) DataContext).ClassificationOperationModel.VisualizationType.ToString();
                mainLabel.Text = ((ClassificationOperationViewModel)DataContext).ClassificationOperationModel.TaskModel.Name.Replace("_", " ").ToString();
                tbType.Text = ((ClassificationOperationViewModel)DataContext).ClassificationOperationModel.TaskModel.Name.Replace("_", " ").ToString();
            }
        }

        private void PlotRenderer_RequestRender(object sender, EventArgs e)
        {
            if (DataContext != null && (DataContext as ClassificationOperationViewModel).ClassificationOperationModel.Result != null)
            {
                render();
            }
        }

        private void ClassificationOperationModel_OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            ClassificationOperationModel classificationOperationModel = ((ClassificationOperationViewModel) DataContext).ClassificationOperationModel;
            mainLabel.Text = classificationOperationModel.TaskModel.Name.Replace("_", " ").ToString();
            tbType.Text = classificationOperationModel.TaskModel.Name.Replace("_", " ").ToString();
            render();
        }

        private void QueryModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ClassificationOperationModel model = (DataContext as ClassificationOperationViewModel).ClassificationOperationModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Result))
            {
                populateData();
                updateProgressAndNullVisualization();
            }
        }

        void VisualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ClassificationOperationViewModel model = ((ClassificationOperationViewModel) DataContext);
            if (e.PropertyName == model.GetPropertyName(() => model.Size))
            {
                render();
            }
            if (e.PropertyName == model.GetPropertyName(() => model.Size) ||
                e.PropertyName == model.GetPropertyName(() => model.Position))
            {
                setMenuViewModelAnkerPosition();
            }
            mainLabel.Text = ((ClassificationOperationViewModel)DataContext).ClassificationOperationModel.TaskModel.Name.ToString();
        }

        private void updateProgressAndNullVisualization()
        {
            IResult resultModel = ((ClassificationOperationViewModel) DataContext).ClassificationOperationModel.Result;

            /*if (descriptionModel != null)
            {
                tbType.Text = ((OperationViewModel)DataContext).OperationModel.TaskType.ToString() + " : " + (descriptionModel.F1s[descriptionModel.Labels[0]] * 100.0).ToString("F1") + "%";
            }
            else*/
            {
                tbType.Text = ((ClassificationOperationViewModel)DataContext).ClassificationOperationModel.TaskModel.Name.Replace("_", " ").ToString();
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
            IResult resultModel = ((ClassificationOperationViewModel)DataContext).ClassificationOperationModel.Result;
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

                loadResults(resultModel);
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

        void loadResults(IResult result)
        {
            ClassificationOperationViewModel model = ((ClassificationOperationViewModel) DataContext);
            int max = 3 + model.ClassificationOperationModel.GetUsageAttributeTransformationModel(InputUsage.Feature).Count;
            _currentViewIndex = (_currentViewIndex) % max;
            _classifierRendererContentProvider.UpdateData(result, model.ClassificationOperationModel, (ClassificationOperationModel) model.ClassificationOperationModel.Clone(), _currentViewIndex);

            render();
        }

        void removeMenu()
        {
            if (_menuViewModel != null)
            {
                InputFieldView inputFieldView = this.GetDescendantsOfType<InputFieldView>().Where(av => av.DataContext == _menuViewModel.AttributeTransformationViewModel).FirstOrDefault();
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
            var visModel = ((ClassificationOperationViewModel) DataContext);
            visModel.ActiveStopwatch.Restart();

            AttributeTransformationViewModel model = (sender as InputFieldView).DataContext as AttributeTransformationViewModel;
            //if (HeaderObjects.Any(ho => ho.AttributeTransformationViewModel == model))
            {
                bool createNew = true;
                if (_menuViewModel != null && !_menuViewModel.IsToBeRemoved)
                {
                    createNew = _menuViewModel.AttributeTransformationViewModel != model;
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
                InputFieldView inputFieldView = this.GetDescendantsOfType<InputFieldView>().Where(av => av.DataContext == _menuViewModel.AttributeTransformationViewModel).FirstOrDefault();

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

            ClassificationOperationViewModel model = ((ClassificationOperationViewModel) DataContext);
            if (model.ClassificationOperationModel.Result != null)
            {
                int max = 3 + model.ClassificationOperationModel.GetUsageAttributeTransformationModel(InputUsage.Feature).Count;
                
                _currentViewIndex = (_currentViewIndex + 1) % max;
                _classifierRendererContentProvider.ViewIndex = _currentViewIndex;

                render();
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
        

        public bool IsDeletable
        {
            get { return false; }
        }

        public IGeometry Geometry
        {
            get
            {
                ClassificationOperationViewModel model = this.DataContext as ClassificationOperationViewModel;

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
            ClassificationOperationViewModel model = ((ClassificationOperationViewModel)DataContext);
            int max = 3 + model.ClassificationOperationModel.GetUsageAttributeTransformationModel(InputUsage.Feature).Count;
            if (_currentViewIndex == max - 1)
            {
                GeneralTransform gt = MainViewController.Instance.InkableScene.TransformToVisual(dxSurface);
                List<Windows.Foundation.Point> selectionPoints = inkStroke.Points.Select(p => gt.TransformPoint(p)).ToList();
                _classifierRendererContentProvider.ProcessStroke(selectionPoints, inkStroke.IsErase);
                render();
            }

            /*GeneralTransform gt = MainViewController.Instance.InkableScene.TransformToVisual(dxSurface);
            List<Windows.Foundation.Point> selectionPoints = inkStroke.Points.Select(p => gt.TransformPoint(p)).ToList();

            IList<Vec> convexHull = Convexhull.convexhull(selectionPoints);
            IGeometry convexHullPoly = convexHull.Select(vec => new Windows.Foundation.Point(vec.X, vec.Y)).ToList().GetPolygon();

            List<FilterModel> hits = new List<FilterModel>();
            foreach (var geom in _plotRendererContentProvider.HitTargets.Keys)
            {
                if (convexHullPoly.Intersects(geom))
                {
                    hits.Add(_plotRendererContentProvider.HitTargets[geom]);
                }
            }
            if (hits.Count > 0)
            {
                foreach (var valueComparison in hits[0].ValueComparisons)
                {
                    Debug.WriteLine((valueComparison.AttributeOperationModel.AttributeModel.RawName + " " +
                                     valueComparison.Value));
                }

                OperationModel histogramOperationModel = (DataContext as OperationViewModel).OperationModel;
                var vcs = hits.SelectMany(h => h.ValueComparisons).ToList();

                var xAom = histogramOperationModel.GetUsageAttributeTransformationModel(InputUsage.X).First();
                var yAom = histogramOperationModel.GetUsageAttributeTransformationModel(InputUsage.Y).First();

                if (hits.Any(h => histogramOperationModel.FilterModels.Contains(h)))
                {
                    histogramOperationModel.RemoveFilterModels(hits);
                }
                else
                {
                    histogramOperationModel.AddFilterModels(hits);
                }
            }*/
            return true;
        }
    }
}
