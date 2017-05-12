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
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.vis.menu;
using Windows.UI.Input;
using System.Diagnostics;
using GeoAPI.Geometries;
using IDEA_common.operations;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.view.inq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class PlotRenderer : Renderer, IScribbable
    {
        private PlotRendererContentProvider _plotRendererContentProvider = new PlotRendererContentProvider();
        

        public PlotRenderer()
        {
            this.InitializeComponent();

            _plotRendererContentProvider.CompositionScaleX = dxSurface.CompositionScaleX;
            _plotRendererContentProvider.CompositionScaleY = dxSurface.CompositionScaleY;
            dxSurface.ContentProvider = _plotRendererContentProvider;
            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += PlotRenderer_Loaded;
        }

        void PlotRenderer_Loaded(object sender, RoutedEventArgs e)
        {
            _plotRendererContentProvider.CompositionScaleX = dxSurface.CompositionScaleX;
            _plotRendererContentProvider.CompositionScaleY = dxSurface.CompositionScaleY;
        }

        public override void Dispose()
        {
            base.Dispose();
            (DataContext as HistogramOperationViewModel).OperationModel.OperationModelUpdated -= OperationModelUpdated;
            (DataContext as HistogramOperationViewModel).OperationModel.PropertyChanged -= OperationModel_PropertyChanged;
            if (dxSurface != null)
            {
                dxSurface.Dispose();
            }
        }

        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (DataContext as HistogramOperationViewModel).OperationModel.OperationModelUpdated -= OperationModelUpdated;
                (DataContext as HistogramOperationViewModel).OperationModel.OperationModelUpdated += OperationModelUpdated;
                (DataContext as HistogramOperationViewModel).OperationModel.PropertyChanged -= OperationModel_PropertyChanged;
                (DataContext as HistogramOperationViewModel).OperationModel.PropertyChanged += OperationModel_PropertyChanged;

                var result = (DataContext as HistogramOperationViewModel).OperationModel.Result;
                if (result != null)
                {
                    loadResult(result);
                    render();
                }
                else
                {
                    HistogramOperationModel operationModel = (HistogramOperationModel)((OperationViewModel)DataContext).OperationModel;
                    if (!operationModel.GetAttributeUsageTransformationModel(AttributeUsage.X).Any() &&
                        !operationModel.GetAttributeUsageTransformationModel(AttributeUsage.Y).Any())
                    {
                        viewBox.Visibility = Visibility.Visible;
                    }
                }
            }
        }
        void OperationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            HistogramOperationModel operationModel = (HistogramOperationModel)((OperationViewModel)DataContext).OperationModel;
            if (e.PropertyName == operationModel.GetPropertyName(() => operationModel.Result))
            {
                var result = (DataContext as HistogramOperationViewModel).OperationModel.Result;
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
                _plotRendererContentProvider.UpdateFilterModels(new List<FilterModel>());
            }
            if (e is FilterOperationModelUpdatedEventArgs && 
                ((FilterOperationModelUpdatedEventArgs)e).FilterOperationModelUpdatedEventType == FilterOperationModelUpdatedEventType.FilterModels)
            {
                _plotRendererContentProvider.UpdateFilterModels((sender as HistogramOperationModel).FilterModels.ToList());
                render();
            }
            if (e is VisualOperationModelUpdatedEventArgs)
            {
                render();
            }
        }

        void loadResult(IResult result)
        {
            HistogramOperationViewModel model = (DataContext as HistogramOperationViewModel);
            _plotRendererContentProvider.UpdateData(result,
                (HistogramOperationModel) model.OperationModel,
                (HistogramOperationModel) model.OperationModel.ResultCauserClone);
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

        public override bool EndSelection()
        {
            IList<Vec> convexHull = Convexhull.convexhull(_selectionPoints);
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
                    Debug.WriteLine((valueComparison.AttributeTransformationModel.AttributeModel.RawName + " " +
                                     valueComparison.Value));
                }

                HistogramOperationModel histogramOperationModel = (HistogramOperationModel) ((HistogramOperationViewModel) DataContext).OperationModel;

                if (hits.Any(h => histogramOperationModel.FilterModels.Contains(h)))
                {
                    histogramOperationModel.RemoveFilterModels(hits);
                }
                else
                {
                    histogramOperationModel.AddFilterModels(hits);
                }
                return true;
            }

            var bczhits = new List<BczBinMapModel>();
            foreach (var geom in _plotRendererContentProvider.BczHitTargets.Keys)
            {
                if (convexHullPoly.Intersects(geom))
                    bczhits.Add(_plotRendererContentProvider.BczHitTargets[geom]);
            }
            if (bczhits.Count > 0)
            {
                _plotRendererContentProvider.UpdateBinSortings(bczhits);
                render();
                return true;
            }
            return false;
        }


        void render(bool sizeChanged = false)
        {
            viewBox.Visibility = Visibility.Collapsed;
            dxSurface?.Redraw();
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
                HistogramOperationViewModel model = this.DataContext as HistogramOperationViewModel;

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
            GeneralTransform gt = MainViewController.Instance.InkableScene.TransformToVisual(dxSurface);
            List<Windows.Foundation.Point> selectionPoints = inkStroke.Points.Select(p => gt.TransformPoint(p)).
                GetLineString().Buffer(1).Coordinates.Select(c => c.GetWindowsPoint()).ToList();

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
                    Debug.WriteLine((valueComparison.AttributeTransformationModel.AttributeModel.RawName + " " +
                                     valueComparison.Value));
                }

                HistogramOperationModel histogramOperationModel = (HistogramOperationModel)((HistogramOperationViewModel)DataContext).OperationModel;
                var vcs = hits.SelectMany(h => h.ValueComparisons).ToList();

                var xAom = histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.X).First();
                var yAom = histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.Y).First();

                if (hits.Any(h => histogramOperationModel.FilterModels.Contains(h)))
                {
                    histogramOperationModel.RemoveFilterModels(hits);
                }
                else
                {
                    histogramOperationModel.AddFilterModels(hits);
                }
            }
            else
            {
                var bczhits = new List<BczBinMapModel>();
                foreach (var geom in _plotRendererContentProvider.BczHitTargets.Keys)
                {
                    if (convexHullPoly.Intersects(geom))
                        bczhits.Add(_plotRendererContentProvider.BczHitTargets[geom]);
                }
                if (bczhits.Count > 0)
                {
                    _plotRendererContentProvider.UpdateBinSortings(bczhits);
                    render();
                }
            }
            return true;
        }
    }
}