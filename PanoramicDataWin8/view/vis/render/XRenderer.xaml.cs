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
using IDEA_common.operations;
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
    public partial class XRenderer : ContentControl, AttributeTransformationViewModelEventHandler
    {
        public delegate void EventHandler(bool sizeChanged = false);
        public event EventHandler Render;

        public delegate void LoadResultHandler(IResult result);
        public event LoadResultHandler LoadResult;

        private MenuViewModel _menuViewModel = null;
        private MenuView _menuView = null;
        private HistogramOperationViewModel _histogramOperationViewModel = null;
        private HistogramOperationModel _histogramOperationModel = null;

        public XRenderer()
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
            if (_histogramOperationViewModel != null)
            {
                _histogramOperationViewModel.PropertyChanged -= HistogramOperationViewModelPropertyChanged;
                _histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.X).CollectionChanged -= X_CollectionChanged;
                _histogramOperationModel.FilterModels.CollectionChanged -= FilterModels_CollectionChanged;
                _histogramOperationModel.PropertyChanged -= QueryModel_PropertyChanged;
            }
        }

        protected override void OnApplyTemplate()
        {
            ((TextBlock)GetTemplateChild("mainLabel")).Text = ((HistogramOperationModel) ((HistogramOperationViewModel) DataContext).OperationModel).VisualizationType.ToString();
            populateHeaders();
        }

        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            removeEventHandlers();
            if (args.NewValue != null)
            {
                _histogramOperationViewModel = (HistogramOperationViewModel) DataContext;
                _histogramOperationViewModel.PropertyChanged += HistogramOperationViewModelPropertyChanged;

                _histogramOperationModel = (HistogramOperationModel)_histogramOperationViewModel.OperationModel;
                _histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.X).CollectionChanged += X_CollectionChanged;
                _histogramOperationModel.FilterModels.CollectionChanged += FilterModels_CollectionChanged;
                _histogramOperationModel.PropertyChanged += QueryModel_PropertyChanged;
                ApplyTemplate();
            }
        }

        void X_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            populateHeaders();
        }

        private void QueryModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            HistogramOperationModel model = (HistogramOperationModel) ((HistogramOperationViewModel) DataContext).OperationModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Result))
            {
                populateData();
                updateProgressAndNullVisualization();
            }
        }

        void HistogramOperationViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            HistogramOperationViewModel model = (DataContext as HistogramOperationViewModel);
            if (e.PropertyName == model.GetPropertyName(() => model.Size))
            {
                if (((InputFieldView)GetTemplateChild("xInputFieldView")).DataContext != null)
                {
                    (((InputFieldView)GetTemplateChild("xInputFieldView")).DataContext as AttributeTransformationViewModel).Size = new Vec(model.Size.X, 54);
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
            HistogramOperationViewModel visModel = ((HistogramOperationViewModel) DataContext);
            HistogramOperationModel histogramOperationModel = (HistogramOperationModel) visModel.OperationModel;
            if (histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.X).Any())
            {
                var xAom = histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.X).First();
                ((InputFieldView)GetTemplateChild("xInputFieldView")).DataContext = new AttributeTransformationViewModel((DataContext as HistogramOperationViewModel), xAom)
                {
                    IsShadow = false,
                    BorderThicknes = new Thickness(0, 4, 0, 0),
                    Size = new Vec(visModel.Size.X, 54),
                    AttachmentOrientation = AttachmentOrientation.Top,
                    HideAggregationFunction = true
                };
            }
            else
            {
                ((InputFieldView)GetTemplateChild("xInputFieldView")).DataContext = new AttributeTransformationViewModel((DataContext as HistogramOperationViewModel), null)
                {
                    IsDraggableByPen = false,
                    IsDraggable = false,
                    IsShadow = false,
                    BorderThicknes = new Thickness(0, 4, 0, 0),
                    Size = new Vec(visModel.Size.X, 54),
                    AttachmentOrientation = AttachmentOrientation.Top,
                    HideAggregationFunction = true
                };
            }
        }

        private void updateProgressAndNullVisualization()
        {
            IResult resultModel = ((HistogramOperationViewModel) DataContext).OperationModel.Result;

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
            /*if (result.ResultDescriptionModel != null && (result.ResultDescriptionModel as VisualizationResultDescriptionModel).NullCount > 0)
            {
                tbNull.Visibility = Windows.UI.Xaml.Visibility.Visible;
                tbNull.Text = "null values : " + (result.ResultDescriptionModel as VisualizationResultDescriptionModel).NullCount;
            }
            else
            {
                tbNull.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }*/
        }

        private void populateData()
        {
            IResult resultModel = (DataContext as HistogramOperationViewModel).OperationModel.Result;

            var contentPresenter = (ContentPresenter)GetTemplateChild("contentPresenter");
            var mainLabel = (TextBlock)GetTemplateChild("mainLabel");

            contentPresenter.Opacity = 1;
            mainLabel.Opacity = 0;

            if (resultModel != null)
            {
                loadResultItemModels(resultModel);
                render();
            }

            var animationGrid = (Grid)GetTemplateChild("animationGrid");
            var progressGrid = (Grid)GetTemplateChild("progressGrid");
            /*if (MainViewController.Instance.MainModel.Mode == Mode.batch &&
                result.ResultType == ResultType.Clear &&
                ((OperationViewModel)DataContext).OperationModel.GetAttributeUsageTransformationModel(AttributeUsage.X).Any() &&
                ((OperationViewModel)DataContext).OperationModel.GetAttributeUsageTransformationModel(AttributeUsage.Y).Any())
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

        private void loadResultItemModels(IResult resultModel)
        {
            if (LoadResult != null)
            {
                LoadResult(resultModel);
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
            AttributeTransformationViewModel model = (sender as InputFieldView).DataContext as AttributeTransformationViewModel;
            var visModel = model.HistogramOperationViewModel;
            if (DataContext == visModel)
            {
                visModel.ActiveStopwatch.Restart();

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
        
        void FilterModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            HistogramOperationModel histogramOperationModel = (HistogramOperationModel) ((HistogramOperationViewModel) DataContext).OperationModel;
            var tbSelection = ((TextBlock)GetTemplateChild("tbSelection"));
            if (histogramOperationModel.FilterModels.Count > 0 &&
                histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.X).Count > 0 &&
                histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.Y).Count > 0)
            {
                var xAom = histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.X).First();
                var yAom = histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.Y).First();


                if (histogramOperationModel.FilterModels.SelectMany(fm => fm.ValueComparisons).Count(vc => Equals(vc.AttributeTransformationModel, xAom)) > 0)
                {
                    tbSelection.Text = xAom.AttributeModel.RawName + ": " +
                                       histogramOperationModel.FilterModels.SelectMany(fm => fm.ValueComparisons).Where(vc => Equals(vc.AttributeTransformationModel, xAom))
                                           .Min(vc => vc.Value).ToString();
                    tbSelection.Text += " - " + histogramOperationModel.FilterModels.SelectMany(fm => fm.ValueComparisons).Where(vc => Equals(vc.AttributeTransformationModel, xAom)).Max(vc => vc.Value);
                }
                if (!xAom.Equals(yAom) &&
                    histogramOperationModel.FilterModels.SelectMany(fm => fm.ValueComparisons).Count(vc => Equals(vc.AttributeTransformationModel, yAom)) > 0)
                {
                    tbSelection.Text += ", " + yAom.AttributeModel.RawName + ": " +
                                        histogramOperationModel.FilterModels.SelectMany(fm => fm.ValueComparisons).Where(vc => Equals(vc.AttributeTransformationModel, yAom))
                                            .Min(vc => vc.Value).ToString();
                    tbSelection.Text += " - " + histogramOperationModel.FilterModels.SelectMany(fm => fm.ValueComparisons).Where(vc => Equals(vc.AttributeTransformationModel, yAom)).Max(vc => vc.Value);
                }

                if (histogramOperationModel.FilterModels.Any(fm => fm.Value.HasValue))
                {
                    tbSelection.Text += ", avg value: " + histogramOperationModel.FilterModels.Where(fm => fm.Value.HasValue).Average(fm => fm.Value.Value).ToString("F1");
                    tbSelection.Text += ", sum value: " + histogramOperationModel.FilterModels.Where(fm => fm.Value.HasValue).Sum(fm => fm.Value.Value).ToString("F1");
                }
                //tbSelection.Text = "" + histogramOperationModel.FilterModels.Count;
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

        public void AttributeTransformationViewModelMoved(AttributeTransformationViewModel sender, AttributeTransformationViewModelEventArgs e, bool overElement)
        {
            var xInputFieldView = (InputFieldView) GetTemplateChild("xInputFieldView");
            var yInputFieldView = (InputFieldView) GetTemplateChild("yInputFieldView");

            var xBounds = xInputFieldView.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (xBounds.Intersects(e.Bounds.GetPolygon()))
            {
                if (xInputFieldView.DataContext != null && !(xInputFieldView.DataContext as AttributeTransformationViewModel).IsHighlighted)
                {
                    (xInputFieldView.DataContext as AttributeTransformationViewModel).IsHighlighted = true;
                }
            }
            else
            {
                if (xInputFieldView.DataContext != null && (xInputFieldView.DataContext as AttributeTransformationViewModel).IsHighlighted)
                {
                    (xInputFieldView.DataContext as AttributeTransformationViewModel).IsHighlighted = false;
                }
            }
        }

        public void AttributeTransformationViewModelDropped(AttributeTransformationViewModel sender, AttributeTransformationViewModelEventArgs e, bool overElement)
        {
            var xInputFieldView = (InputFieldView)GetTemplateChild("xInputFieldView");
            // turn both off 
            if (xInputFieldView.DataContext != null)
            {
                (xInputFieldView.DataContext as AttributeTransformationViewModel).IsHighlighted = false;
            }

            if (!overElement)
            {
                return;
            }

            HistogramOperationModel qModel = (HistogramOperationModel) ((HistogramOperationViewModel) DataContext).OperationModel;

            // if both are empty before hand add default value
            if (!qModel.GetAttributeUsageTransformationModel(AttributeUsage.X).Any() && !qModel.GetAttributeUsageTransformationModel(AttributeUsage.Y).Any())
            {
                AttributeTransformationModel value = new AttributeTransformationModel(e.AttributeTransformationModel.AttributeModel);
                value.AggregateFunction = AggregateFunction.Count;

                qModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue, value);
            }

            var xBounds = xInputFieldView.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            if (xBounds.Intersects(e.Bounds.GetPolygon()))
            {
                if (qModel.GetAttributeUsageTransformationModel(AttributeUsage.X).Any())
                {
                    qModel.RemoveAttributeUsageTransformationModel(AttributeUsage.X, qModel.GetAttributeUsageTransformationModel(AttributeUsage.X).First());
                }
                AttributeTransformationModel value = new AttributeTransformationModel(e.AttributeTransformationModel.AttributeModel);
                if (((AttributeFieldModel) e.AttributeTransformationModel.AttributeModel).InputDataType == InputDataTypeConstants.FLOAT ||
                    ((AttributeFieldModel) e.AttributeTransformationModel.AttributeModel).InputDataType == InputDataTypeConstants.INT)
                {
                    value.AggregateFunction = AggregateFunction.Avg;
                }
                else
                {
                    value.AggregateFunction = AggregateFunction.Count;
                }
                qModel.AddAttributeUsageTransformationModel(AttributeUsage.X, value);
            }
        }
    }
}
