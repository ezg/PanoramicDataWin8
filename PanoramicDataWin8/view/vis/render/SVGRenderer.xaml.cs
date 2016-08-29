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
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.view.inq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class SVGRenderer : Renderer, IScribbable
    {
        private SVGRendererContentProvider _svgRendererContentProvider = null;

        private DXSurface dxSurface = null;

        public SVGRenderer(string svgFilePath)
        {
            this.InitializeComponent();

            _svgRendererContentProvider = new SVGRendererContentProvider(svgFilePath);

            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += PlotRenderer_Loaded;
        }

        void PlotRenderer_Loaded(object sender, RoutedEventArgs e)
        {
            xRenderer.Render += render;
            xRenderer.LoadResultItemModels += loadResultItemModels;
            dxSurface = new DXSurface();
            _svgRendererContentProvider.CompositionScaleX = dxSurface.CompositionScaleX;
            _svgRendererContentProvider.CompositionScaleY = dxSurface.CompositionScaleY;
            dxSurface.ContentProvider = _svgRendererContentProvider;
            xRenderer.Content = dxSurface;
        }

        public override void Dispose()
        {
            base.Dispose();
            xRenderer.Dispose();
            (DataContext as VisualizationViewModel).QueryModel.QueryModelUpdated -= QueryModel_QueryModelUpdated;
            if (dxSurface != null)
            {
                dxSurface.Dispose();
            }
        }

        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (DataContext as VisualizationViewModel).QueryModel.QueryModelUpdated += QueryModel_QueryModelUpdated;
                (DataContext as VisualizationViewModel).QueryModel.RequestRender += PlotRenderer_RequestRender;
            }
        }

        private void PlotRenderer_RequestRender(object sender, EventArgs e)
        {
            if (DataContext != null && (DataContext as VisualizationViewModel).QueryModel.ResultModel != null)
            {
                var resultModel = (DataContext as VisualizationViewModel).QueryModel.ResultModel;
                if (resultModel.ResultItemModels.Count > 0 || resultModel.Progress == 1.0 || resultModel.ResultType == ResultType.Complete)
                {
                    render();
                }
            }
        }

        void QueryModel_QueryModelUpdated(object sender, QueryModelUpdatedEventArgs e)
        {
            if (e.QueryModelUpdatedEventType == QueryModelUpdatedEventType.FilterModels || e.QueryModelUpdatedEventType == QueryModelUpdatedEventType.ClearFilterModels)
            {
                _svgRendererContentProvider.UpdateFilterModels((sender as QueryModel).FilterModels.ToList());
                render();
            }
        }

        void loadResultItemModels(ResultModel resultModel)
        {
            VisualizationViewModel model = (DataContext as VisualizationViewModel);
            _svgRendererContentProvider.UpdateData(resultModel,
                model.QueryModel,
                model.QueryModel.Clone(),
                model.QueryModel.GetUsageInputOperationModel(InputUsage.X).FirstOrDefault(),
                model.QueryModel.GetUsageInputOperationModel(InputUsage.Y).FirstOrDefault());
            render();
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
            foreach (var geom in _svgRendererContentProvider.HitTargets.Keys)
            {
                if (convexHullPoly.Intersects(geom))
                {
                    hits.Add(_svgRendererContentProvider.HitTargets[geom]);
                }
            }
            if (hits.Count > 0)
            {
                foreach (var valueComparison in hits[0].ValueComparisons)
                {
                    Debug.WriteLine((valueComparison.InputOperationModel.InputModel.RawName + " " +
                                     valueComparison.Value));
                }

                QueryModel queryModel = (DataContext as VisualizationViewModel).QueryModel;

                if (hits.Any(h => queryModel.FilterModels.Contains(h)))
                {
                    queryModel.RemoveFilterModels(hits);
                }
                else
                {
                    queryModel.AddFilterModels(hits);
                }
            }
        }


        void render(bool sizeChanged = false)
        {
            if (dxSurface != null)
            {
                dxSurface.Redraw();
            }
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
                VisualizationViewModel model = this.DataContext as VisualizationViewModel;

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
            List<Windows.Foundation.Point> selectionPoints = inkStroke.Points.Select(p => gt.TransformPoint(p)).ToList();

            IList<Vec> convexHull = Convexhull.convexhull(selectionPoints);
            IGeometry convexHullPoly = convexHull.Select(vec => new Windows.Foundation.Point(vec.X, vec.Y)).ToList().GetPolygon();

            List<FilterModel> hits = new List<FilterModel>();
            foreach (var geom in _svgRendererContentProvider.HitTargets.Keys)
            {
                if (convexHullPoly.Intersects(geom))
                {
                    hits.Add(_svgRendererContentProvider.HitTargets[geom]);
                }
            }
            if (hits.Count > 0)
            {
                foreach (var valueComparison in hits[0].ValueComparisons)
                {
                    Debug.WriteLine((valueComparison.InputOperationModel.InputModel.RawName + " " +
                                     valueComparison.Value));
                }

                QueryModel queryModel = (DataContext as VisualizationViewModel).QueryModel;
                var vcs = hits.SelectMany(h => h.ValueComparisons).ToList();

                var xAom = queryModel.GetUsageInputOperationModel(InputUsage.X).First();
                var yAom = queryModel.GetUsageInputOperationModel(InputUsage.Y).First();

                if (hits.Any(h => queryModel.FilterModels.Contains(h)))
                {
                    queryModel.RemoveFilterModels(hits);
                }
                else
                {
                    queryModel.AddFilterModels(hits);
                }
            }
            return true;
        }
    }
}