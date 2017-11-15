using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using IDEA_common.aggregates;
using IDEA_common.operations;
using IDEA_common.operations.histogram;
using IDEA_common.operations.recommender;
using IDEA_common.util;
using PanoramicDataWin8.controller.data.progressive;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.common;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.view.vis.render;
using Point = Windows.Foundation.Point;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.menu
{
    public sealed partial class RecommendedHistogramMenuItemView : UserControl
    {
        private PlotRendererContentProvider _plotRendererContentProvider = new PlotRendererContentProvider();
        private RecommendedHistogramMenuItemViewModel _model = null;
        private HistogramOperationViewModel _brusher = null;
        private Color _brushColor = Colors.White;
        private MenuItemView _shadow = null;
        private long _manipulationStartTime = 0;
        private Pt _startDrag = new Point(0, 0);
        private Pt _currentFromInkableScene = new Point(0, 0);

        private PointerManager _mainPointerManager = new PointerManager();
        private Point _mainPointerManagerPreviousPoint = new Point();

        public RecommendedHistogramMenuItemView()
        {
            this.InitializeComponent();
            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += RecommendedHistogram_Loaded;


            _mainPointerManager.Added += mainPointerManager_Added;
            _mainPointerManager.Moved += mainPointerManager_Moved;
            _mainPointerManager.Removed += mainPointerManager_Removed;
            _mainPointerManager.Attach(this);
        }

        void RecommendedHistogram_Loaded(object sender, RoutedEventArgs e)
        {
            _plotRendererContentProvider.CompositionScaleX = dxSurface.CompositionScaleX;
            _plotRendererContentProvider.CompositionScaleY = dxSurface.CompositionScaleY;
            dxSurface.ContentProvider = _plotRendererContentProvider;
        }
        
        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_model != null)
            {
                _model.PropertyChanged -= RecommendedHistogramMenuItemViewModel_PropertyChanged;
            }
            if (args.NewValue != null)
            {
                _model = (DataContext as MenuItemViewModel).MenuItemComponentViewModel as RecommendedHistogramMenuItemViewModel;
                _model.PropertyChanged += RecommendedHistogramMenuItemViewModel_PropertyChanged;
                if (_model.RecommendedHistogram.HistogramResult != null)
                {
                    loadResult(_model.RecommendedHistogram);
                    render();
                }
            }
        }

        void loadResult(RecommendedHistogram recommendedHistogram)
        {
            if (recommendedHistogram.HistogramResult != null)
            {
                var xIom = new AttributeTransformationModel(IDEAHelpers.GetAttributeModelFromAttribute(
                    recommendedHistogram.XAttribute, _model.HistogramOperationViewModel.HistogramOperationModel.SchemaModel.OriginModels.First()));
                var yIom = new AttributeTransformationModel(IDEAHelpers.GetAttributeModelFromAttribute(
                    recommendedHistogram.YAttribute, _model.HistogramOperationViewModel.HistogramOperationModel.SchemaModel.OriginModels.First()))
                {
                    AggregateFunction = AggregateFunction.Count
                };

                txtBlock.Text = xIom.GetLabel;

                var filterModels = IDEAHelpers.GetFilterModelsFromSelections(_model.RecommendedHistogram.Selections,
                        _model.HistogramOperationViewModel.HistogramOperationModel.SchemaModel.OriginModels.First());
                _plotRendererContentProvider.UpdateFilterModels(filterModels);

                _plotRendererContentProvider.UpdateData(recommendedHistogram.HistogramResult, 
                    false, BrushViewModel.ColorScheme1.First().Yield().ToList(), xIom, yIom, yIom, 0);
            }
        }


        private void mainPointerManager_Added(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                GeneralTransform gt = this.TransformToVisual(MainViewController.Instance.InkableScene);
                _mainPointerManagerPreviousPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);
                _manipulationStartTime = DateTime.Now.Ticks;
            }
        }

        void mainPointerManager_Moved(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                GeneralTransform gt = this.TransformToVisual(MainViewController.Instance.InkableScene);
                Point currentPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);

                Vec delta = gt.TransformPoint(e.StartContacts[e.TriggeringPointer.PointerId].Position).GetVec() - currentPoint.GetVec();


                if (_shadow == null && _brusher == null &&
                    _manipulationStartTime + TimeSpan.FromSeconds(0.3).Ticks < DateTime.Now.Ticks)
                {
                    Debug.WriteLine("create Brush " + currentPoint);

                    var attributeModel = IDEAHelpers.GetAttributeModelFromAttribute(_model.RecommendedHistogram.XAttribute, 
                        _model.HistogramOperationViewModel.HistogramOperationModel.SchemaModel.OriginModels.First());


                    _brusher = OperationViewModelFactory.CreateDefaultHistogramOperationViewModel(
                        MainViewController.Instance.MainModel.SchemaModel,
                        attributeModel,
                        new Pt());
                    var filterModels = IDEAHelpers.GetFilterModelsFromSelections(_model.RecommendedHistogram.Selections,
                         _model.HistogramOperationViewModel.HistogramOperationModel.SchemaModel.OriginModels.First());
                    _brusher.HistogramOperationModel.AddFilterModels(filterModels);

                    _brushColor = BrushViewModel.ColorScheme1[BrushableViewController.Instance.GetColorIndex(_model.HistogramOperationViewModel)];
                    var toModel = _model.HistogramOperationViewModel.HistogramOperationModel;
                    toModel.BrushColors.Add(_brushColor);
                    toModel.BrushOperationModels.Add(_brusher.HistogramOperationModel as IBrusherOperationModel);


                }

                if (delta.Length > 50 && _shadow == null)
                {
                    if (_brusher != null)
                    {
                        disposeBrusher();
                    }
                    Debug.WriteLine("create shadow " + currentPoint);
                    createShadow(currentPoint);
                }

                if (_shadow != null)
                {
                    _shadow.RenderTransform = new TranslateTransform()
                    {
                        X = currentPoint.X - _shadow.Width / 2.0,
                        Y = currentPoint.Y - _shadow.Height
                    };
                }

                _mainPointerManagerPreviousPoint = currentPoint;
                e.DoHandle = false;
            }
        }

        void mainPointerManager_Removed(object sender, PointerManagerEvent e)
        {
            if (_brusher != null)
            {
                disposeBrusher();
            }
            if (_shadow != null)
            {
                InkableScene inkableScene = MainViewController.Instance.InkableScene;
                Rct bounds = _shadow.GetBounds(inkableScene);
                _model.FireDroppedEvent(bounds);
                inkableScene.Remove(_shadow);
                _shadow = null;
            }

            _manipulationStartTime = 0;
        }

        private void disposeBrusher()
        {
            var toModel = _model.HistogramOperationViewModel.HistogramOperationModel;
            toModel.BrushColors.Remove(_brushColor);
            toModel.BrushOperationModels.Remove(_brusher.HistogramOperationModel as IBrusherOperationModel);
            _brusher = null;
        }

        public void createShadow(Point fromInkableScene)
        {
            InkableScene inkableScene = MainViewController.Instance.InkableScene;

            if (inkableScene != null && _model.RecommendedHistogram.HistogramResult != null)
            {
                _currentFromInkableScene = fromInkableScene;
                _shadow = new MenuItemView();
                _shadow.DataContext = this.DataContext;

                _shadow.Measure(new Size(double.PositiveInfinity,
                    double.PositiveInfinity));

                _shadow.RenderTransform = new TranslateTransform()
                {
                    X = fromInkableScene.X - _shadow.Width / 2.0,
                    Y = fromInkableScene.Y - _shadow.Height
                };


                inkableScene.Add(_shadow);
                _shadow.SendToFront();

                Rct bounds = _shadow.GetBounds(inkableScene);
                //model.FireMoved(bounds, model.AttributeTransformationModel);
            }
        }


        void render(bool sizeChanged = false)
        {
            dxSurface?.Redraw();
        }

        void RecommendedHistogramMenuItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _model.GetPropertyName(() => _model.RecommendedHistogram))
            {
                loadResult(_model.RecommendedHistogram);
                render();
            }
        }

    }
}
