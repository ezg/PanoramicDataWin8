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
using Windows.UI.Xaml.Navigation;
using GeoAPI.Geometries;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.common;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.view.vis.menu;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public partial class XYRenderer : ContentControl, InputFieldViewModelEventHandler
    {
        public delegate void EventHandler(bool sizeChanged = false);
        public event EventHandler Render;

        public delegate void LoadResultItemModelsHandler(ResultModel resultModel);
        public event LoadResultItemModelsHandler LoadResultItemModels;

        private MenuViewModel _menuViewModel = null;
        private MenuView _menuView = null;
        private VisualizationViewModel _visualizationViewModel = null;

        public XYRenderer()
        {
            this.InitializeComponent();

            DataContextChanged += PlotRenderer_DataContextChanged;
            InputFieldView.InputFieldViewModelTapped += InputFieldViewInputFieldViewModelTapped;
        }

        public void Dispose()
        {
            removeMenu();
            InputFieldView.InputFieldViewModelTapped -= InputFieldViewInputFieldViewModelTapped;
            DataContextChanged -= PlotRenderer_DataContextChanged;
            removeEventHandlers();
        }

        private void removeEventHandlers()
        {
            if (_visualizationViewModel != null)
            {
                _visualizationViewModel.PropertyChanged -= VisualizationViewModel_PropertyChanged;
                _visualizationViewModel.QueryModel.GetUsageInputOperationModel(InputUsage.X).CollectionChanged -= X_CollectionChanged;
                _visualizationViewModel.QueryModel.GetUsageInputOperationModel(InputUsage.Y).CollectionChanged -= Y_CollectionChanged;
                _visualizationViewModel.QueryModel.FilterModels.CollectionChanged -= FilterModels_CollectionChanged;
                ResultModel resultModel = _visualizationViewModel.QueryModel.ResultModel;
                resultModel.ResultModelUpdated -= resultModel_ResultModelUpdated;
            }
        }

        protected override void OnApplyTemplate()
        {
            ((TextBlock)GetTemplateChild("mainLabel")).Text = (DataContext as VisualizationViewModel).QueryModel.VisualizationType.ToString();
            populateHeaders();
        }

        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            removeEventHandlers();
            if (args.NewValue != null)
            {
                _visualizationViewModel = (VisualizationViewModel) DataContext;
                _visualizationViewModel.PropertyChanged += VisualizationViewModel_PropertyChanged;
                _visualizationViewModel.QueryModel.GetUsageInputOperationModel(InputUsage.X).CollectionChanged += X_CollectionChanged;
                _visualizationViewModel.QueryModel.GetUsageInputOperationModel(InputUsage.Y).CollectionChanged += Y_CollectionChanged;
                _visualizationViewModel.QueryModel.FilterModels.CollectionChanged += FilterModels_CollectionChanged;
                ResultModel resultModel = _visualizationViewModel.QueryModel.ResultModel;
                resultModel.ResultModelUpdated += resultModel_ResultModelUpdated;
                ApplyTemplate();
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

        void resultModel_ResultModelUpdated(object sender, EventArgs e)
        {
            populateData();
            updateProgressAndNullVisualization();
        }

        void VisualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            VisualizationViewModel model = (DataContext as VisualizationViewModel);
            if (e.PropertyName == model.GetPropertyName(() => model.Size))
            {
                if (((InputFieldView)GetTemplateChild("xInputFieldView")).DataContext != null)
                {
                    (((InputFieldView)GetTemplateChild("xInputFieldView")).DataContext as InputFieldViewModel).Size = new Vec(model.Size.X - 54, 54);
                }
                if (((InputFieldView)GetTemplateChild("yInputFieldView")).DataContext != null)
                {
                    (((InputFieldView)GetTemplateChild("yInputFieldView")).DataContext as InputFieldViewModel).Size = new Vec(54, model.Size.Y - 54);
                }

                render(true);
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
            if (queryModel.GetUsageInputOperationModel(InputUsage.X).Any())
            {
                var xAom = queryModel.GetUsageInputOperationModel(InputUsage.X).First();
                ((InputFieldView)GetTemplateChild("xInputFieldView")).DataContext = new InputFieldViewModel((DataContext as VisualizationViewModel), xAom) 
                {
                    IsShadow = false,
                    BorderThicknes = new Thickness(0, 4, 0, 0),
                    Size = new Vec(visModel.Size.X - 54, 54),
                    AttachmentOrientation = AttachmentOrientation.Top
                };
            }
            else
            {
                ((InputFieldView)GetTemplateChild("xInputFieldView")).DataContext = new InputFieldViewModel((DataContext as VisualizationViewModel), null)
                {
                    IsDraggableByPen = false,
                    IsDraggable = false,
                    IsShadow = false,
                    BorderThicknes = new Thickness(0, 4, 0, 0),
                    Size = new Vec(visModel.Size.X - 54, 54),
                    AttachmentOrientation = AttachmentOrientation.Top
                };
            }

            if (queryModel.GetUsageInputOperationModel(InputUsage.Y).Any())
            {
                var yAom = queryModel.GetUsageInputOperationModel(InputUsage.Y).First();
                ((InputFieldView)GetTemplateChild("yInputFieldView")).DataContext = new InputFieldViewModel((DataContext as VisualizationViewModel), yAom)
                {
                    IsShadow = false,
                    BorderThicknes = new Thickness(0, 0, 4, 0),
                    Size = new Vec(54, visModel.Size.Y - 54),
                    TextAngle = 270,
                    AttachmentOrientation = AttachmentOrientation.Left
                };
            }
            else
            {
                ((InputFieldView)GetTemplateChild("yInputFieldView")).DataContext = new InputFieldViewModel((DataContext as VisualizationViewModel), null)
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
            ResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.ResultModel;

            // progress
            double size = 14;
            double thickness = 2;

            double progress = resultModel.Progress;

            var arcSegement1 = (ArcSegment)GetTemplateChild("arcSegement1");
            var tbPercentage1 = (TextBlock)GetTemplateChild("tbPercentage1");
            var tbNull = (TextBlock)GetTemplateChild("tbNull");

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
            if (resultModel.ResultDescriptionModel != null && (resultModel.ResultDescriptionModel as VisualizationResultDescriptionModel).NullCount > 0)
            {
                tbNull.Visibility = Windows.UI.Xaml.Visibility.Visible;
                tbNull.Text = "null values : " + (resultModel.ResultDescriptionModel as VisualizationResultDescriptionModel).NullCount;
            }
            else
            {
                tbNull.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void populateData()
        {
            ResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.ResultModel;

            var contentPresenter = (ContentPresenter)GetTemplateChild("contentPresenter");
            var mainLabel = (TextBlock)GetTemplateChild("mainLabel");

            contentPresenter.Opacity = 1;
            mainLabel.Opacity = 0;

            if (resultModel.ResultItemModels.Count > 0 || resultModel.Progress == 1.0 || resultModel.ResultType == ResultType.Complete)
            {
                loadResultItemModels(resultModel);
                render();
            }

            var animationGrid = (Grid)GetTemplateChild("animationGrid");
            var progressGrid = (Grid)GetTemplateChild("progressGrid");
            /*if (MainViewController.Instance.MainModel.Mode == Mode.batch &&
                resultModel.ResultType == ResultType.Clear &&
                ((VisualizationViewModel)DataContext).QueryModel.GetUsageInputOperationModel(InputUsage.X).Any() &&
                ((VisualizationViewModel)DataContext).QueryModel.GetUsageInputOperationModel(InputUsage.Y).Any())
            {

                animationGrid.Opacity = 1.0;
                contentPresenter.Opacity = 0.0;
                progressGrid.Opacity = 0.0;
            }
            else*/
            {
                animationGrid.Opacity = 0.0;
                contentPresenter.Opacity = 1.0;
                progressGrid.Opacity = 1.0;
            }
        }

        private void loadResultItemModels(ResultModel resultModel)
        {
            if (LoadResultItemModels != null)
            {
                LoadResultItemModels(resultModel);
            }
        }

        private void render(bool sizeChanged = false)
        {
            if (Render != null)
            {
                Render(sizeChanged);
            }
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
            InputFieldViewModel model = (sender as InputFieldView).DataContext as InputFieldViewModel;
            var visModel = model.VisualizationViewModel;
            if (DataContext == visModel)
            {
                visModel.ActiveStopwatch.Restart();

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
        
        void FilterModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            QueryModel queryModel = (DataContext as VisualizationViewModel).QueryModel;
            var tbSelection = ((TextBlock)GetTemplateChild("tbSelection"));
            if (queryModel.FilterModels.Count > 0 &&
                queryModel.GetUsageInputOperationModel(InputUsage.X).Count > 0 &&
                queryModel.GetUsageInputOperationModel(InputUsage.Y).Count > 0)
            {
                var xAom = queryModel.GetUsageInputOperationModel(InputUsage.X).First();
                var yAom = queryModel.GetUsageInputOperationModel(InputUsage.Y).First();


                if (queryModel.FilterModels.SelectMany(fm => fm.ValueComparisons).Count(vc => Equals(vc.InputOperationModel, xAom)) > 0)
                {
                    tbSelection.Text = xAom.InputModel.Name + ": " +
                                       queryModel.FilterModels.SelectMany(fm => fm.ValueComparisons).Where(vc => Equals(vc.InputOperationModel, xAom))
                                           .Min(vc => vc.Value).ToString();
                    tbSelection.Text += " - " + queryModel.FilterModels.SelectMany(fm => fm.ValueComparisons).Where(vc => Equals(vc.InputOperationModel, xAom)).Max(vc => vc.Value);
                }
                if (!xAom.Equals(yAom) &&
                    queryModel.FilterModels.SelectMany(fm => fm.ValueComparisons).Count(vc => Equals(vc.InputOperationModel, yAom)) > 0)
                {
                    tbSelection.Text += ", " + yAom.InputModel.Name + ": " +
                                        queryModel.FilterModels.SelectMany(fm => fm.ValueComparisons).Where(vc => Equals(vc.InputOperationModel, yAom))
                                            .Min(vc => vc.Value).ToString();
                    tbSelection.Text += " - " + queryModel.FilterModels.SelectMany(fm => fm.ValueComparisons).Where(vc => Equals(vc.InputOperationModel, yAom)).Max(vc => vc.Value);
                }

                if (queryModel.FilterModels.Any(fm => fm.Value.HasValue))
                {
                    tbSelection.Text += ", avg value: " + queryModel.FilterModels.Where(fm => fm.Value.HasValue).Average(fm => fm.Value.Value).ToString("F1");
                    tbSelection.Text += ", sum value: " + queryModel.FilterModels.Where(fm => fm.Value.HasValue).Sum(fm => fm.Value.Value).ToString("F1");
                }
                //tbSelection.Text = "" + queryModel.FilterModels.Count;
            }
            else
            {
                tbSelection.Text = "";
            }
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                return this.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            }
        }

        public void InputFieldViewModelMoved(InputFieldViewModel sender, InputFieldViewModelEventArgs e, bool overElement)
        {
            var xInputFieldView = (InputFieldView) GetTemplateChild("xInputFieldView");
            var yInputFieldView = (InputFieldView) GetTemplateChild("yInputFieldView");

            var xBounds = xInputFieldView.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (xBounds.Intersects(e.Bounds.GetPolygon()))
            {
                if (xInputFieldView.DataContext != null && !(xInputFieldView.DataContext as InputFieldViewModel).IsHighlighted)
                {
                    (xInputFieldView.DataContext as InputFieldViewModel).IsHighlighted = true;
                }
            }
            else
            {
                if (xInputFieldView.DataContext != null && (xInputFieldView.DataContext as InputFieldViewModel).IsHighlighted)
                {
                    (xInputFieldView.DataContext as InputFieldViewModel).IsHighlighted = false;
                }
            }

            var yBounds = yInputFieldView.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (yBounds.Intersects(e.Bounds.GetPolygon()) && !xBounds.Intersects(e.Bounds.GetPolygon()))
            {
                if (yInputFieldView.DataContext != null && !(yInputFieldView.DataContext as InputFieldViewModel).IsHighlighted)
                {
                    (yInputFieldView.DataContext as InputFieldViewModel).IsHighlighted = true;
                }
            }
            else
            {
                if (yInputFieldView.DataContext != null && (yInputFieldView.DataContext as InputFieldViewModel).IsHighlighted)
                {
                    (yInputFieldView.DataContext as InputFieldViewModel).IsHighlighted = false;
                }
            }
        }

        public void InputFieldViewModelDropped(InputFieldViewModel sender, InputFieldViewModelEventArgs e, bool overElement)
        {
            var xInputFieldView = (InputFieldView)GetTemplateChild("xInputFieldView");
            var yInputFieldView = (InputFieldView)GetTemplateChild("yInputFieldView");
            // turn both off 
            if (xInputFieldView.DataContext != null)
            {
                (xInputFieldView.DataContext as InputFieldViewModel).IsHighlighted = false;
            }
            if (yInputFieldView.DataContext != null)
            {
                (yInputFieldView.DataContext as InputFieldViewModel).IsHighlighted = false;
            }

            if (!overElement)
            {
                return;
            }

            QueryModel qModel = (DataContext as VisualizationViewModel).QueryModel;

            // if both are empty before hand add default value
            if (!qModel.GetUsageInputOperationModel(InputUsage.X).Any() && !qModel.GetUsageInputOperationModel(InputUsage.Y).Any())
            {
                InputOperationModel value = new InputOperationModel(e.InputOperationModel.InputModel);
                value.AggregateFunction = AggregateFunction.Count;

                qModel.AddUsageInputOperationModel(InputUsage.DefaultValue, value);
            }

            var xBounds = xInputFieldView.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (xBounds.Intersects(e.Bounds.GetPolygon()))
            {
                if (qModel.GetUsageInputOperationModel(InputUsage.X).Any())
                {
                    qModel.RemoveUsageInputOperationModel(InputUsage.X, qModel.GetUsageInputOperationModel(InputUsage.X).First());
                }
                qModel.AddUsageInputOperationModel(InputUsage.X, e.InputOperationModel);
            }

            var yBounds = yInputFieldView.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (yBounds.Intersects(e.Bounds.GetPolygon()) && !xBounds.Intersects(e.Bounds.GetPolygon()))
            {
                if (qModel.GetUsageInputOperationModel(InputUsage.Y).Any())
                {
                    qModel.RemoveUsageInputOperationModel(InputUsage.Y, qModel.GetUsageInputOperationModel(InputUsage.Y).First());
                }
                qModel.AddUsageInputOperationModel(InputUsage.Y, e.InputOperationModel);
            }
        }
    }
}
