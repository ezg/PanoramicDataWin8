using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.inq;
using LinkType = PanoramicDataWin8.model.data.operation.LinkType;
using Polygon = Windows.UI.Xaml.Shapes.Polygon;

namespace PanoramicDataWin8.view.vis
{
    public class FilterLinkView : UserControl, IScribbable
    {
        private readonly double _attachmentRectHalfSize = 15;
        private SolidColorBrush _backgroundBrush = new SolidColorBrush(Helpers.GetColorFromString("#ffffff"));
        //private Dictionary<FilteringType, Vec> _attachmentCenters = new Dictionary<FilteringType, Vec>(); 

        private SolidColorBrush _darkBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717"));
        private readonly SolidColorBrush _highlightBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5"));

        private readonly SolidColorBrush _highlightFaintBrush =
            new SolidColorBrush(Helpers.GetColorFromString("#3329aad5"));

        private readonly SolidColorBrush _lightBrush = new SolidColorBrush(Helpers.GetColorFromString("#e6e6e6"));
        private IGeometry _linkViewGeometry;
        private List<IDisposable> _sourceDisposables = new List<IDisposable>();

        private readonly Dictionary<OperationViewModel, IGeometry> _visualizationViewModelCenterGeometries =
            new Dictionary<OperationViewModel, IGeometry>();

        private readonly Dictionary<OperationViewModel, IGeometry> _visualizationViewModelGeometries =
            new Dictionary<OperationViewModel, IGeometry>();

        private readonly Dictionary<OperationViewModel, IGeometry> _visualizationViewModelIconGeometries =
            new Dictionary<OperationViewModel, IGeometry>();

        public FilterLinkView()
        {
            DataContextChanged += LinkView_DataContextChanged;
            PointerPressed += LinkView_PointerPressed;
        }

        public IGeometry Geometry
        {
            get
            {
                var filterLinkViewModel = DataContext as FilterLinkViewModel;
                if (filterLinkViewModel.FilterLinkModels.Count > 0)
                {
                    var unionGeometry = _linkViewGeometry;

                    foreach (var model in _visualizationViewModelGeometries.Keys)
                    {
                        unionGeometry = unionGeometry.Union(_visualizationViewModelGeometries[model].Buffer(3));
                        unionGeometry = unionGeometry.Union(_visualizationViewModelCenterGeometries[model]);
                    }

                    /*Polyline pl = new Polyline();
                    pl.Points = new PointCollection(unionGeometry.Coordinates.Select((corrd) => new Point(corrd.X, corrd.Y)).ToArray());
                    pl.Stroke = Brushes.Green;
                    pl.StrokeThickness = 1;
                    MainViewController.Instance.InkableScene.Add(pl);*/

                    return unionGeometry;
                }
                return new Point(-40000, -40000);
            }
        }

        public List<IScribbable> Children
        {
            get { return new List<IScribbable>(); }
        }

        public bool Consume(InkStroke inkStroke)
        {
            var filterLinkViewModel = DataContext as FilterLinkViewModel;
            var models = new List<FilterLinkModel>();
            foreach (var model in _visualizationViewModelGeometries.Keys.ToArray())
                if (_visualizationViewModelGeometries[model].Buffer(3).Intersects(inkStroke.GetLineString()))
                {
                    var linkModel =
                        filterLinkViewModel.FilterLinkModels.First(lm => lm.FromOperationModel == model.OperationModel);
                    linkModel.IsInverted = !linkModel.IsInverted;
                    linkModel.ToOperationModel.FireOperationModelUpdated(
                        new FilterOperationModelUpdatedEventArgs(FilterOperationModelUpdatedEventType.Links));
                }
            return true;
        }

        public bool IsDeletable
        {
            get { return true; }
        }

        private void LinkView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                (e.NewValue as FilterLinkViewModel).FilterLinkModels.CollectionChanged += LinkModels_CollectionChanged;
                (e.NewValue as FilterLinkViewModel).FromOperationViewModels.CollectionChanged +=
                    FromVisualizationViewModels_CollectionChanged;
                (e.NewValue as FilterLinkViewModel).ToOperationViewModel.PropertyChanged +=
                    ToVisualizationViewModel_PropertyChanged;
                (e.NewValue as FilterLinkViewModel).ToOperationViewModel.OperationModel.PropertyChanged +=
                    QueryModel_PropertyChanged;
                updateRendering();
            }
        }

        private void QueryModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            updateRendering();
        }

        private void FromVisualizationViewModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    (item as HistogramOperationViewModel).PropertyChanged -= FromVisualizationViewModel_PropertyChanged;
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    (item as HistogramOperationViewModel).PropertyChanged += FromVisualizationViewModel_PropertyChanged;
            updateRendering();
        }

        private void ToVisualizationViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            updateRendering();
        }

        private void FromVisualizationViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            updateRendering();
        }

        private void LinkModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    (item as FilterLinkModel).PropertyChanged -= LinkView_PropertyChanged;
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    (item as FilterLinkModel).PropertyChanged += LinkView_PropertyChanged;
            updateRendering();
        }

        private void LinkView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            updateRendering();
        }

        private void updateRendering()
        {
            var c = new Canvas();
            var filterLinkViewModel = DataContext as FilterLinkViewModel;
            if (filterLinkViewModel.FromOperationViewModels.Count > 0)
            {
                _visualizationViewModelGeometries.Clear();
                _visualizationViewModelCenterGeometries.Clear();
                _visualizationViewModelIconGeometries.Clear();
                
                var destinationRct = new Rct(filterLinkViewModel.ToOperationViewModel.Position,
                    new Vec(filterLinkViewModel.ToOperationViewModel.Size.X, filterLinkViewModel.ToOperationViewModel.Size.Y));

                var attachmentCenter = updateAttachmentCenter(LinkType.Filter, c);
                var attachmentLocation = getAttachmentLocation(destinationRct, attachmentCenter);

                drawLinesFromModelsToAttachmentCenter(LinkType.Filter, attachmentCenter, c);
                if (filterLinkViewModel.FilterLinkModels.Any(lm => lm.LinkType == LinkType.Filter))
                    drawFilterAttachment(attachmentCenter, c, attachmentLocation);

                attachmentCenter = updateAttachmentCenter(LinkType.Brush, c);
                drawLinesFromModelsToAttachmentCenter(LinkType.Brush, attachmentCenter, c);
                if (filterLinkViewModel.FilterLinkModels.Any(lm => lm.LinkType == LinkType.Brush))
                    drawBrushAttachment(attachmentCenter, c);
            }
            Content = c;
        }

        private AttachmentLocation getAttachmentLocation(Rct destinationRct, Vec attachmentCenter)
        {
            if (attachmentCenter.X <= destinationRct.Left)
                return AttachmentLocation.Left;
            if (attachmentCenter.X >= destinationRct.Right)
                return AttachmentLocation.Right;
            if (attachmentCenter.Y <= destinationRct.Top)
                return AttachmentLocation.Top;
            return AttachmentLocation.Bottom;
        }

        private Vec updateAttachmentCenter(LinkType linkType, Canvas canvas)
        {
            var filterLinkViewModel = DataContext as FilterLinkViewModel;

            var destinationRct = new Rct(filterLinkViewModel.ToOperationViewModel.Position,
                new Vec(filterLinkViewModel.ToOperationViewModel.Size.X, filterLinkViewModel.ToOperationViewModel.Size.Y));
            var destinationGeom = destinationRct.GetLineString();

            var midPoints = new List<Pt>();
            var sourceCount = 0;
            foreach (
                var from in
                filterLinkViewModel.FromOperationViewModels.Where(
                    vvm => vvm.OperationModel is IFilterProviderOperationModel).Where(
                    vvm =>
                        filterLinkViewModel.FilterLinkModels.Where(lvm => lvm.LinkType == linkType)
                            .Select(lvm => lvm.FromOperationModel)
                            .Contains(vvm.OperationModel as IFilterProviderOperationModel)))
            {
                sourceCount++;
                var fromCenterToCenter = new Windows.Foundation.Point[]
                {
                    filterLinkViewModel.ToOperationViewModel.Position +
                    filterLinkViewModel.ToOperationViewModel.Size/2.0,
                    from.Position + from.Size/2.0
                }.GetLineString();
                var sourceRct = new Rct(from.Position,
                    new Vec(from.Size.X, from.Size.Y)).GetLineString();
                var interPtSource = sourceRct.Intersection(fromCenterToCenter);
                var interPtDestination = destinationGeom.Intersection(fromCenterToCenter);

                var midPoint = new Vec();
                if (interPtDestination.IsEmpty || interPtSource.IsEmpty)
                    midPoint = (filterLinkViewModel.ToOperationViewModel.Position +
                                filterLinkViewModel.ToOperationViewModel.Size/2.0 +
                                (from.Position + from.Size/2.0)).GetVec()/2.0;
                else
                    midPoint = (new Vec(interPtSource.Centroid.X, interPtSource.Centroid.Y) +
                                new Vec(interPtDestination.Centroid.X, interPtDestination.Centroid.Y))/2.0;
                midPoints.Add(new Pt(midPoint.X, midPoint.Y));
            }

            if (sourceCount == 0)
                if (linkType == LinkType.Brush)
                    return new Vec(
                        destinationRct.Left + _attachmentRectHalfSize,
                        destinationRct.Bottom + _attachmentRectHalfSize - 2);
                else if (linkType == LinkType.Filter)
                    return new Vec(
                        destinationRct.Right - _attachmentRectHalfSize,
                        destinationRct.Bottom + _attachmentRectHalfSize - 2);

            var tempAttachment = midPoints.Aggregate((p1, p2) => p1 + p2).GetVec()/midPoints.Count;
            var destinationVec = new Vec(
                (filterLinkViewModel.ToOperationViewModel.Position + filterLinkViewModel.ToOperationViewModel.Size/2.0)
                    .X,
                (filterLinkViewModel.ToOperationViewModel.Position + filterLinkViewModel.ToOperationViewModel.Size/2.0)
                    .Y);
            var inter =
                destinationGeom.Intersection(
                    new Windows.Foundation.Point[]
                        {tempAttachment.GetCoord().GetPt(), destinationVec.GetCoord().GetPt()}.GetLineString());
            var attachmentCenter = new Vec();

            if (inter.IsEmpty)
            {
                var dirVec = tempAttachment - destinationVec;
                dirVec = dirVec.Normal()*40000;
                dirVec += tempAttachment;
                inter =
                    destinationGeom.Intersection(
                        new Windows.Foundation.Point[] {dirVec.GetCoord().GetPt(), destinationVec.GetCoord().GetPt()}
                            .GetLineString());
                attachmentCenter = new Vec(inter.Centroid.X, inter.Centroid.Y);
            }
            else
            {
                attachmentCenter = new Vec(inter.Centroid.X, inter.Centroid.Y);
            }

            var attachmentLocation = getAttachmentLocation(destinationRct, attachmentCenter);
            // left
            if (attachmentLocation == AttachmentLocation.Left)
                attachmentCenter = new Vec(
                    attachmentCenter.X - _attachmentRectHalfSize + 2,
                    Math.Min(Math.Max(attachmentCenter.Y, destinationRct.Top + _attachmentRectHalfSize),
                        destinationRct.Bottom - _attachmentRectHalfSize));
            // right
            else if (attachmentLocation == AttachmentLocation.Right)
                attachmentCenter = new Vec(
                    attachmentCenter.X + _attachmentRectHalfSize - 2,
                    Math.Min(Math.Max(attachmentCenter.Y, destinationRct.Top + _attachmentRectHalfSize),
                        destinationRct.Bottom - _attachmentRectHalfSize));
            // top
            else if (attachmentLocation == AttachmentLocation.Top)
                attachmentCenter = new Vec(
                    Math.Min(Math.Max(attachmentCenter.X, destinationRct.Left + _attachmentRectHalfSize),
                        destinationRct.Right - _attachmentRectHalfSize),
                    attachmentCenter.Y - _attachmentRectHalfSize + 2);
            // bottom
            else if (attachmentLocation == AttachmentLocation.Bottom)
                attachmentCenter = new Vec(
                    Math.Min(Math.Max(attachmentCenter.X, destinationRct.Left + _attachmentRectHalfSize),
                        destinationRct.Right - _attachmentRectHalfSize),
                    attachmentCenter.Y + _attachmentRectHalfSize - 2);

            return attachmentCenter;
        }

        private void drawLinesFromModelsToAttachmentCenter(LinkType linkType, Vec attachmentCenter, Canvas c)
        {
            var filterLinkViewModel = DataContext as FilterLinkViewModel;
            foreach (
                var incomingModel in
                filterLinkViewModel.FromOperationViewModels.Where(
                    vvm => vvm.OperationModel is IFilterProviderOperationModel).Where(
                    vvm =>
                        filterLinkViewModel.FilterLinkModels.Where(lvm => lvm.LinkType == linkType)
                            .Select(lvm => lvm.FromOperationModel)
                            .Contains(vvm.OperationModel as IFilterProviderOperationModel)))
            {
                var incomingCenter = new Vec(
                    (incomingModel.Position + incomingModel.Size/2.0).X,
                    (incomingModel.Position + incomingModel.Size/2.0).Y);
                var incomingRct = new Rct(incomingModel.Position,
                    new Vec(incomingModel.Size.X, incomingModel.Size.Y));

                var isInverted =
                    filterLinkViewModel.FilterLinkModels.Where(lm => lm.IsInverted)
                        .Select(lm => lm.FromOperationModel)
                        .Contains(incomingModel.OperationModel as IFilterProviderOperationModel);

                    var inter =
                    incomingRct.GetLineString()
                        .Intersection(
                            new Windows.Foundation.Point[]
                                    {attachmentCenter.GetCoord().GetPt(), incomingCenter.GetCoord().GetPt()}
                                .GetLineString());
                var incomingStart = new Vec();

                if (inter.IsEmpty)
                    incomingStart = incomingCenter;
                else
                    incomingStart = new Vec(inter.Centroid.X, inter.Centroid.Y);
                var distanceVec = attachmentCenter - incomingStart;
                if (distanceVec.Length > 0)
                {
                    var cutOff = distanceVec.Normalized()*(distanceVec.Length - _attachmentRectHalfSize);

                    /* var l1 = new Line();
                     l1.X1 = incomingStart.X;
                     l1.Y1 = incomingStart.Y;
                     l1.X2 = incomingStart.X + cutOff.X;
                     l1.Y2 = incomingStart.Y + cutOff.Y;
                     if (
                         filterLinkViewModel.FilterLinkModels.Where(lm => lm.IsInverted)
                             .Select(lm => lm.FromOperationModel)
                             .Contains(incomingModel.OperationModel as IFilterProviderOperationModel))
                         l1.StrokeDashArray = new DoubleCollection {2};
                     l1.Stroke = new SolidColorBrush(Colors.Red);
                     l1.StrokeThickness = 2;
                     c.Children.Add(l1);*/


                    var n = distanceVec.Perp().Normalized();
                    var trianglePos = distanceVec.Normalized()*(distanceVec.Length*0.3) + incomingStart;


                    var start = incomingStart + distanceVec.Normalized()*4;
                    var end = incomingStart + cutOff;

                    var thinkness = 4;
                    var arrowWidth = 8;
                    var arrowLength = 20;
                    var poly = new Polygon();

                    if (!isInverted)
                    {
                        poly.Points.Add((start - (n*thinkness/2.0)).GetWindowsPoint());
                        poly.Points.Add((start + (n*thinkness/2.0)).GetWindowsPoint());
                    }
                    poly.Points.Add((trianglePos + (n*thinkness/2.0)).GetWindowsPoint());
                    poly.Points.Add((trianglePos + (n*((thinkness/2.0) + arrowWidth))).GetWindowsPoint());
                    poly.Points.Add((trianglePos + (distanceVec.Normalized()*arrowLength) + (n*((thinkness/2.0)))).GetWindowsPoint());
                    if (!isInverted)
                    {
                        poly.Points.Add((end + (n*thinkness/2.0)).GetWindowsPoint());
                        poly.Points.Add((end - (n*thinkness/2.0)).GetWindowsPoint());
                    }
                    poly.Points.Add((trianglePos + (distanceVec.Normalized()*arrowLength) - (n*((thinkness/2.0)))).GetWindowsPoint());
                    poly.Points.Add((trianglePos - (n*((thinkness/2.0) + arrowWidth))).GetWindowsPoint());
                    poly.Points.Add((trianglePos - (n*thinkness/2.0)).GetWindowsPoint());
                    if (!isInverted)
                    {
                        poly.Points.Add((start - (n*thinkness/2.0)).GetWindowsPoint());
                    }

                    /*poly.Points.Add(new Windows.Foundation.Point(trianglePos.X + n.X*8, trianglePos.Y + n.Y*8));
                    poly.Points.Add(new Windows.Foundation.Point(trianglePos.X - n.X*8, trianglePos.Y - n.Y*8));
                    poly.Points.Add(new Windows.Foundation.Point(trianglePos.X + distanceVec.Normalized().X*20,
                        trianglePos.Y + distanceVec.Normalized().Y*20));
                    poly.Points.Add(new Windows.Foundation.Point(trianglePos.X + n.X*8, trianglePos.Y + n.Y*8));*/
                    poly.Fill = _lightBrush;
                    poly.StrokeThickness = 1;
                    //poly.StrokeDashArray = new DoubleCollection { 2 };
                    poly.Stroke = _backgroundBrush;
                    c.Children.Add(poly);

                    if (isInverted)
                    {
                        var steps = Math.Floor((start - trianglePos).Length / 50);
                        var from = start;
                        var to = trianglePos;
                        for (int s = 0; s < steps; s++)
                        {
                            var r = new Polygon();
                            var f = from + distanceVec.Normalized() * s * 50.0;
                            var t = from + distanceVec.Normalized() * (s + 1) * 50.0;

                            poly.Points.Add((from - (n * thinkness / 2.0)).GetWindowsPoint());
                            poly.Points.Add((from + (n * thinkness / 2.0)).GetWindowsPoint());

                            r.Fill = _lightBrush;
                            r.StrokeThickness = 1;
                            r.Stroke = _backgroundBrush;
                            c.Children.Add(r);
                        }
                    }

                    if (!_visualizationViewModelCenterGeometries.ContainsKey(incomingModel))
                    {
                        _visualizationViewModelCenterGeometries.Add(incomingModel,
                            poly.Points.GetPolygon().Buffer(3));
                        _visualizationViewModelGeometries.Add(incomingModel,
                            new Windows.Foundation.Point[]
                                    {incomingStart.GetCoord().GetPt(), (incomingStart + cutOff).GetCoord().GetPt()}
                                .GetLineString());
                    }
                }
            }
        }

        private void drawFilterAttachment(Vec attachmentCenter, Canvas c, AttachmentLocation attachmentLocation)
        {
            var filterLinkViewModel = DataContext as FilterLinkViewModel;
            var sourceCount = filterLinkViewModel.FilterLinkModels.Count(lvm => lvm.LinkType == LinkType.Filter);

            var c1 = new Border();
            c1.Width = _attachmentRectHalfSize*2;
            c1.Height = _attachmentRectHalfSize*2;

            c1.RenderTransform = new TranslateTransform
            {
                X = attachmentCenter.X - _attachmentRectHalfSize,
                Y = attachmentCenter.Y - _attachmentRectHalfSize
            };
            c.Children.Add(c1);

            c1.Background = _lightBrush;
            var outline = _backgroundBrush;

            var thickness = 4;
            var overlap = 2;
            if (attachmentLocation == AttachmentLocation.Left)
            {
                var r1 = new Rectangle();
                r1.Fill = outline;
                r1.Width = _attachmentRectHalfSize * 2 + thickness - overlap;
                r1.Height = thickness;
                r1.RenderTransform = new TranslateTransform
                {
                    X = attachmentCenter.X - _attachmentRectHalfSize - thickness,
                    Y = attachmentCenter.Y + _attachmentRectHalfSize
                };
                c.Children.Add(r1);

                var r2 = new Rectangle();
                r2.Fill = outline;
                r2.Width = _attachmentRectHalfSize * 2 + thickness - overlap;
                r2.Height = thickness;
                r2.RenderTransform = new TranslateTransform
                {
                    X = attachmentCenter.X - _attachmentRectHalfSize - thickness,
                    Y = attachmentCenter.Y - _attachmentRectHalfSize - thickness
                };
                c.Children.Add(r2);

                var r3 = new Rectangle();
                r3.Fill = outline;
                r3.Width = thickness;
                r3.Height = _attachmentRectHalfSize * 2 + thickness - overlap;
                r3.RenderTransform = new TranslateTransform
                {
                    X = attachmentCenter.X - _attachmentRectHalfSize - thickness,
                    Y = attachmentCenter.Y - _attachmentRectHalfSize
                };
                c.Children.Add(r3);
            }
            if (attachmentLocation == AttachmentLocation.Right)
            {

                var r1 = new Rectangle();
                r1.Fill = outline;
                r1.Width = _attachmentRectHalfSize * 2 + thickness - overlap;
                r1.Height = thickness;
                r1.RenderTransform = new TranslateTransform
                {
                    X = attachmentCenter.X - _attachmentRectHalfSize + overlap,
                    Y = attachmentCenter.Y + _attachmentRectHalfSize
                };
                c.Children.Add(r1);

                var r2 = new Rectangle();
                r2.Fill = outline;
                r2.Width = _attachmentRectHalfSize * 2 + thickness - overlap;
                r2.Height = thickness;
                r2.RenderTransform = new TranslateTransform
                {
                    X = attachmentCenter.X - _attachmentRectHalfSize + overlap,
                    Y = attachmentCenter.Y - _attachmentRectHalfSize - thickness
                };
                c.Children.Add(r2);

                var r3 = new Rectangle();
                r3.Fill = outline;
                r3.Width = thickness;
                r3.Height = _attachmentRectHalfSize * 2 + thickness * 2;
                r3.RenderTransform = new TranslateTransform
                {
                    X = attachmentCenter.X + _attachmentRectHalfSize,
                    Y = attachmentCenter.Y - _attachmentRectHalfSize - thickness
                };
                c.Children.Add(r3);
            }
            if (attachmentLocation == AttachmentLocation.Top)
            {
                var r1 = new Rectangle();
                r1.Fill = outline;
                r1.Width = _attachmentRectHalfSize * 2 + thickness * 2;
                r1.Height = thickness;
                r1.RenderTransform = new TranslateTransform
                {
                    X = attachmentCenter.X - _attachmentRectHalfSize - thickness,
                    Y = attachmentCenter.Y - _attachmentRectHalfSize - thickness
                };
                c.Children.Add(r1);

                var r2 = new Rectangle();
                r2.Fill = outline;
                r2.Width = thickness;
                r2.Height = _attachmentRectHalfSize * 2 + thickness - overlap;
                r2.RenderTransform = new TranslateTransform
                {
                    X = attachmentCenter.X + _attachmentRectHalfSize,
                    Y = attachmentCenter.Y - _attachmentRectHalfSize - thickness
                };
                c.Children.Add(r2);

                var r3 = new Rectangle();
                r3.Fill = outline;
                r3.Width = thickness;
                r3.Height = _attachmentRectHalfSize * 2 + thickness - overlap;
                r3.RenderTransform = new TranslateTransform
                {
                    X = attachmentCenter.X - _attachmentRectHalfSize - thickness,
                    Y = attachmentCenter.Y - _attachmentRectHalfSize - thickness
                };
                c.Children.Add(r3);
            }
            if (attachmentLocation == AttachmentLocation.Bottom)
            {
                var r1 = new Rectangle();
                r1.Fill = outline;
                r1.Width = _attachmentRectHalfSize * 2 + thickness * 2;
                r1.Height = thickness;
                r1.RenderTransform = new TranslateTransform
                {
                    X = attachmentCenter.X - _attachmentRectHalfSize - thickness,
                    Y = attachmentCenter.Y + _attachmentRectHalfSize
                };
                c.Children.Add(r1);

                var r2 = new Rectangle();
                r2.Fill = outline;
                r2.Width = thickness;
                r2.Height = _attachmentRectHalfSize * 2 + thickness - overlap;
                r2.RenderTransform = new TranslateTransform
                {
                    X = attachmentCenter.X + _attachmentRectHalfSize,
                    Y = attachmentCenter.Y - _attachmentRectHalfSize
                };
                c.Children.Add(r2);

                var r3 = new Rectangle();
                r3.Fill = outline;
                r3.Width = thickness;
                r3.Height = _attachmentRectHalfSize * 2 + thickness - overlap;
                r3.RenderTransform = new TranslateTransform
                {
                    X = attachmentCenter.X - _attachmentRectHalfSize - thickness,
                    Y = attachmentCenter.Y - _attachmentRectHalfSize
                };
                c.Children.Add(r3);
            }
            

            var filterIcon = new Polygon();
            filterIcon.Points = new PointCollection();
            filterIcon.Points.Add(new Windows.Foundation.Point(0, 0));
            filterIcon.Points.Add(new Windows.Foundation.Point(15, 0));
            filterIcon.Points.Add(new Windows.Foundation.Point(10, 6));
            filterIcon.Points.Add(new Windows.Foundation.Point(10, 14));
            filterIcon.Points.Add(new Windows.Foundation.Point(5, 12));
            filterIcon.Points.Add(new Windows.Foundation.Point(5, 6));
            filterIcon.Fill = sourceCount > 1 ? _highlightFaintBrush : _highlightBrush;
            filterIcon.Width = _attachmentRectHalfSize*2;
            filterIcon.Height = _attachmentRectHalfSize*2;
            var mat = Mat.Translate(attachmentCenter.X - _attachmentRectHalfSize + 6,
                          attachmentCenter.Y - _attachmentRectHalfSize + 7 + (sourceCount > 1 ? 0 : 0))*
                      Mat.Scale(1.2, 1.2);
            filterIcon.RenderTransform = new MatrixTransform {Matrix = mat};
            c.Children.Add(filterIcon);

            if (sourceCount > 1)
            {
                var label = new TextBlock();
                if (((IFilterConsumerOperationModel) filterLinkViewModel.ToOperationViewModel.OperationModel).FilteringOperation == FilteringOperation.AND)
                    label.Text = "AND";
                else if (((IFilterConsumerOperationModel) filterLinkViewModel.ToOperationViewModel.OperationModel).FilteringOperation == FilteringOperation.OR)
                    label.Text = "OR";
                label.TextAlignment = TextAlignment.Center;
                label.FontSize = 9;
                label.FontWeight = FontWeights.Bold;
                label.Width = _attachmentRectHalfSize*2;
                label.Foreground = _highlightBrush;
                label.Height = _attachmentRectHalfSize*2;
                c.UseLayoutRounding = false;
                label.RenderTransform = new TranslateTransform
                {
                    X = attachmentCenter.X - _attachmentRectHalfSize,
                    Y = attachmentCenter.Y - _attachmentRectHalfSize + 10
                };
                c.Children.Add(label);
            }

            var t = attachmentCenter - new Vec(_attachmentRectHalfSize, _attachmentRectHalfSize);
            var r = new Rct(new Pt(t.X, t.Y),
                new Vec(_attachmentRectHalfSize*2, _attachmentRectHalfSize*2));
            _linkViewGeometry = r.GetPolygon().Buffer(3);
        }

        private void drawBrushAttachment(Vec attachmentCenter, Canvas c)
        {
            var filterLinkViewModel = DataContext as FilterLinkViewModel;
            var sourceCount = filterLinkViewModel.FilterLinkModels.Count(lvm => lvm.LinkType == LinkType.Brush);

            var c1 = new Rectangle();
            c1.Width = _attachmentRectHalfSize*2;
            c1.Height = _attachmentRectHalfSize*2;

            c1.RenderTransform = new TranslateTransform
            {
                X = attachmentCenter.X - _attachmentRectHalfSize,
                Y = attachmentCenter.Y - _attachmentRectHalfSize
            };
            c.Children.Add(c1);

            c1.Fill = _lightBrush;
            c1.Stroke = _lightBrush;
            c1.StrokeThickness = 2;

            var brushCanvas = new Canvas();
            var p1 = new Path();
            p1.Fill = _lightBrush;
            var b = new Binding
            {
                Source = "m 0,0 c 0.426,0 0.772,-0.346 0.772,-0.773 0,-0.426 -0.346,-0.772 -0.772,-0.772 -0.427,0 -0.773,0.346 -0.773,0.772 C -0.773,-0.346 -0.427,0 0,0 m -9.319,11.674 c 0,0 7.188,0.868 7.188,-7.187 l 0,-5.26 c 0,-1.618 1.175,-1.888 2.131,-1.888 0,0 1.914,-0.245 1.871,1.87 l 0,5.246 c 0,0 0.214,7.219 7.446,7.219 l 0,2.21 -18.636,0 0,-2.21 z"
            };
            BindingOperations.SetBinding(p1, Path.DataProperty, b);
            var tg = new TransformGroup();
            tg.Children.Add(new ScaleTransform {ScaleX = 1, ScaleY = -1});
            tg.Children.Add(new TranslateTransform {X = 9.3, Y = 26});
            p1.RenderTransform = tg;
            brushCanvas.Children.Add(p1);

            var p2 = new Path();
            p2.Fill = _lightBrush;
            b = new Binding
            {
                Source = "m 0,0 0,-0.491 0,-4.316 18.636,0 0,3.58 0,1.227 0,5.333 L 0,5.333 0,0 z"
            };
            BindingOperations.SetBinding(p2, Path.DataProperty, b);
            tg = new TransformGroup();
            tg.Children.Add(new ScaleTransform {ScaleX = 1, ScaleY = -1});
            tg.Children.Add(new TranslateTransform {X = 0, Y = 6});
            p2.RenderTransform = tg;
            brushCanvas.Children.Add(p2);

            //BrushIcon brushIcon = new BrushIcon();
            //brushIcon.SetBrush(sourceCount > 1 && false ? Destination.FaintBrush : Destination.Brush);
            Matrix mat = Mat.Translate(
                             attachmentCenter.X - _attachmentRectHalfSize + 7,
                             attachmentCenter.Y - _attachmentRectHalfSize + 7)*Mat.Scale(0.80, 0.80)*Mat.Rotate(new Deg(45), new Pt(10, 10));
            brushCanvas.RenderTransform = new MatrixTransform {Matrix = mat};
            c.Children.Add(brushCanvas);

            var t = attachmentCenter - new Vec(_attachmentRectHalfSize, _attachmentRectHalfSize);
            var r = new Rct(new Pt(t.X, t.Y),
                new Vec(_attachmentRectHalfSize*2, _attachmentRectHalfSize*2));
            _linkViewGeometry = r.GetPolygon().Buffer(3);
        }

        private void LinkView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var p = e.GetCurrentPoint(MainViewController.Instance.InkableScene).Position.GetVec().GetCoord().GetPoint();
            var filterLinkViewModel = DataContext as FilterLinkViewModel;

            /*foreach (var visModel in _visualizationViewModelIconGeometries.Keys)
            {
                if (_visualizationViewModelIconGeometries[visModel].Buffer(3).Intersects(p))
                {
                    FilterLinkModel linkModel = linkViewModel.LinkModels.Where(lm => lm.FromOperationModel == visModel.OperationModel).First();
                    linkModel.LinkType = linkModel.LinkType == LinkType.Brush ? LinkType.Filter : LinkType.Brush;

                    e.Handled = true;
                    break;
                }
            }*/


            if (_linkViewGeometry.Intersects(p))
            {
                var op = ((IFilterConsumerOperationModel) filterLinkViewModel.ToOperationViewModel.OperationModel).FilteringOperation;
                ((IFilterConsumerOperationModel) filterLinkViewModel.ToOperationViewModel.OperationModel).FilteringOperation = op == FilteringOperation.AND ? FilteringOperation.OR : FilteringOperation.AND;
                e.Handled = true;
                foreach (var linkModel in filterLinkViewModel.FilterLinkModels)
                    linkModel.ToOperationModel.FireOperationModelUpdated(new FilterOperationModelUpdatedEventArgs(FilterOperationModelUpdatedEventType.Links));
            }
        }

        public List<FilterLinkModel> GetLinkModelsToRemove(IGeometry scribble)
        {
            var filterLinkViewModel = DataContext as FilterLinkViewModel;
            var models = new List<FilterLinkModel>();
            if (scribble.Intersects(_linkViewGeometry.Buffer(3)))
                models = filterLinkViewModel.FilterLinkModels.ToList();
            else
                foreach (var model in _visualizationViewModelGeometries.Keys)
                    if (_visualizationViewModelGeometries[model].Buffer(3).Intersects(scribble))
                        models.Add(filterLinkViewModel.FilterLinkModels.First(lm => lm.FromOperationModel == model.OperationModel));
            return models;
        }
    }

    public enum AttachmentLocation
    {
        Top, Bottom, Left, Right
    }
}