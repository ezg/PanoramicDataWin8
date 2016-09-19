using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PanoramicDataWin8.utils;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Text;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.view.inq;
using LinkType = PanoramicDataWin8.model.data.LinkType;

namespace PanoramicDataWin8.view.vis
{
    public class FilterLinkView : UserControl, IScribbable
    {
        private List<IDisposable> _sourceDisposables = new List<IDisposable>();
        private double _attachmentRectHalfSize = 15;
        private Dictionary<OperationViewModel, IGeometry> _visualizationViewModelGeometries = new Dictionary<OperationViewModel, IGeometry>();
        private IGeometry _linkViewGeometry = null;
        private Dictionary<OperationViewModel, IGeometry> _visualizationViewModelCenterGeometries = new Dictionary<OperationViewModel, IGeometry>();
        private Dictionary<OperationViewModel, IGeometry> _visualizationViewModelIconGeometries = new Dictionary<OperationViewModel, IGeometry>();
        //private Dictionary<FilteringType, Vec> _attachmentCenters = new Dictionary<FilteringType, Vec>(); 

        private SolidColorBrush _darkBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717"));
        private SolidColorBrush _highlightBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5"));
        private SolidColorBrush _highlightFaintBrush = new SolidColorBrush(Helpers.GetColorFromString("#3329aad5"));
        private SolidColorBrush _lightBrush = new SolidColorBrush(Helpers.GetColorFromString("#e6e6e6"));
        private SolidColorBrush _backgroundBrush = new SolidColorBrush(Helpers.GetColorFromString("#ffffff"));

        public FilterLinkView()
        {
            this.DataContextChanged += LinkView_DataContextChanged;
            this.PointerPressed += LinkView_PointerPressed;
        }

        void LinkView_DataContextChanged(FrameworkElement sender, Windows.UI.Xaml.DataContextChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                (e.NewValue as FilterLinkViewModel).FilterLinkModels.CollectionChanged += LinkModels_CollectionChanged;
                (e.NewValue as FilterLinkViewModel).FromOperationViewModels.CollectionChanged += FromVisualizationViewModels_CollectionChanged;
                (e.NewValue as FilterLinkViewModel).ToOperationViewModel.PropertyChanged += ToVisualizationViewModel_PropertyChanged;
                (e.NewValue as FilterLinkViewModel).ToOperationViewModel.OperationModel.PropertyChanged += QueryModel_PropertyChanged;
                updateRendering();
            }
        }

        void QueryModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateRendering();
        }

        void FromVisualizationViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    (item as HistogramOperationViewModel).PropertyChanged -= FromVisualizationViewModel_PropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    (item as HistogramOperationViewModel).PropertyChanged += FromVisualizationViewModel_PropertyChanged;
                }
            }
            updateRendering();
        }

        void ToVisualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateRendering();
        }

        void FromVisualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateRendering();
        }

        void LinkModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    (item as FilterLinkModel).PropertyChanged -= LinkView_PropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    (item as FilterLinkModel).PropertyChanged += LinkView_PropertyChanged;
                }
            }
            updateRendering();
        }

        void LinkView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateRendering();
        }

        private void updateRendering()
        {
            Canvas c = new Canvas();
            FilterLinkViewModel filterLinkViewModel = (DataContext as FilterLinkViewModel);
            if (filterLinkViewModel.FromOperationViewModels.Count > 0)
            {
                _visualizationViewModelGeometries.Clear();
                _visualizationViewModelCenterGeometries.Clear();
                _visualizationViewModelIconGeometries.Clear();

                Vec attachmentCenter = updateAttachmentCenter(LinkType.Filter, c);
                drawLinesFromModelsToAttachmentCenter(LinkType.Filter, attachmentCenter, c);
                if (filterLinkViewModel.FilterLinkModels.Any(lm => lm.LinkType == LinkType.Filter))
                {
                    drawFilterAttachment(attachmentCenter, c);
                }

                attachmentCenter = updateAttachmentCenter(LinkType.Brush, c);
                drawLinesFromModelsToAttachmentCenter(LinkType.Brush, attachmentCenter, c);
                if (filterLinkViewModel.FilterLinkModels.Any(lm => lm.LinkType == LinkType.Brush))
                {
                    drawBrushAttachment(attachmentCenter, c);
                }
            }
            Content = c;
        }

        private Vec updateAttachmentCenter(LinkType linkType, Canvas canvas)
        {
            FilterLinkViewModel filterLinkViewModel = (DataContext as FilterLinkViewModel);

            Rct destinationRct = new Rct(filterLinkViewModel.ToOperationViewModel.Position,
                new Vec(filterLinkViewModel.ToOperationViewModel.Size.X, filterLinkViewModel.ToOperationViewModel.Size.Y));
            var destinationGeom = destinationRct.GetLineString();

            List<Pt> midPoints = new List<Pt>();
            int sourceCount = 0;
            foreach (var from in filterLinkViewModel.FromOperationViewModels.Where(vvm => vvm.OperationModel is IFilterProviderOperationModel).Where(
                vvm => filterLinkViewModel.FilterLinkModels.Where(lvm => lvm.LinkType == linkType).Select(lvm => lvm.FromOperationModel).Contains(vvm.OperationModel as IFilterProviderOperationModel)))
            {
                sourceCount++;
                var fromCenterToCenter = new Point[] { 
                    filterLinkViewModel.ToOperationViewModel.Position + filterLinkViewModel.ToOperationViewModel.Size / 2.0, 
                    from.Position + from.Size / 2.0 }.GetLineString();
                var sourceRct = new Rct(from.Position,
                    new Vec(from.Size.X, from.Size.Y)).GetLineString();
                var interPtSource = sourceRct.Intersection(fromCenterToCenter);
                var interPtDestination = destinationGeom.Intersection(fromCenterToCenter);

                Vec midPoint = new Vec();
                if (interPtDestination.IsEmpty || interPtSource.IsEmpty)
                {
                    midPoint = ((filterLinkViewModel.ToOperationViewModel.Position + filterLinkViewModel.ToOperationViewModel.Size / 2.0) +
                        (from.Position + from.Size / 2.0)).GetVec() / 2.0;
                }
                else
                {
                    midPoint = (new Vec(interPtSource.Centroid.X, interPtSource.Centroid.Y) +
                                new Vec(interPtDestination.Centroid.X, interPtDestination.Centroid.Y)) / 2.0;
                }
                midPoints.Add(new Pt(midPoint.X, midPoint.Y));
            }

            if (sourceCount == 0)
            {
                if (linkType == LinkType.Brush)
                {
                    return new Vec(
                        destinationRct.Left + _attachmentRectHalfSize,
                        destinationRct.Bottom + _attachmentRectHalfSize - 2);
                }
                else if (linkType == LinkType.Filter)
                {
                    return new Vec(
                        destinationRct.Right - _attachmentRectHalfSize,
                        destinationRct.Bottom + _attachmentRectHalfSize - 2);
                }
            }

            Vec tempAttachment = midPoints.Aggregate((p1, p2) => p1 + p2).GetVec() / (double)midPoints.Count;
            Vec destinationVec = new Vec(
                (filterLinkViewModel.ToOperationViewModel.Position + filterLinkViewModel.ToOperationViewModel.Size / 2.0).X,
                (filterLinkViewModel.ToOperationViewModel.Position + filterLinkViewModel.ToOperationViewModel.Size / 2.0).Y);
            var inter = destinationGeom.Intersection(new Point[] { tempAttachment.GetCoord().GetPt(), destinationVec.GetCoord().GetPt() }.GetLineString());
            Vec attachmentCenter = new Vec();

            if (inter.IsEmpty)
            {
                Vec dirVec = tempAttachment - destinationVec;
                dirVec = dirVec.Normal() * 40000;
                dirVec += tempAttachment;
                inter = destinationGeom.Intersection(new Point[] { dirVec.GetCoord().GetPt(), destinationVec.GetCoord().GetPt() }.GetLineString());
                attachmentCenter = new Vec(inter.Centroid.X, inter.Centroid.Y);
            }
            else
            {
                attachmentCenter = new Vec(inter.Centroid.X, inter.Centroid.Y);
            }

            // left
            if (attachmentCenter.X <= destinationRct.Left)
            {
                attachmentCenter = new Vec(
                    attachmentCenter.X - _attachmentRectHalfSize + 2,
                    Math.Min(Math.Max(attachmentCenter.Y, destinationRct.Top + _attachmentRectHalfSize), destinationRct.Bottom - _attachmentRectHalfSize));
            }
            // right
            else if (attachmentCenter.X >= destinationRct.Right)
            {
                attachmentCenter = new Vec(
                    attachmentCenter.X + _attachmentRectHalfSize - 2,
                    Math.Min(Math.Max(attachmentCenter.Y, destinationRct.Top + _attachmentRectHalfSize), destinationRct.Bottom - _attachmentRectHalfSize));
            }
            // top
            else if (attachmentCenter.Y <= destinationRct.Top)
            {
                attachmentCenter = new Vec(
                    Math.Min(Math.Max(attachmentCenter.X, destinationRct.Left + _attachmentRectHalfSize), destinationRct.Right - _attachmentRectHalfSize),
                    attachmentCenter.Y - _attachmentRectHalfSize + 2);
            }
            // bottom
            else if (attachmentCenter.Y >= destinationRct.Bottom)
            {
                attachmentCenter = new Vec(
                    Math.Min(Math.Max(attachmentCenter.X, destinationRct.Left + _attachmentRectHalfSize), destinationRct.Right - _attachmentRectHalfSize),
                    attachmentCenter.Y + _attachmentRectHalfSize - 2);
            }

            return attachmentCenter;
        }

        private void drawLinesFromModelsToAttachmentCenter(LinkType linkType, Vec attachmentCenter, Canvas c)
        {
            FilterLinkViewModel filterLinkViewModel = (DataContext as FilterLinkViewModel);
            foreach (var incomingModel in filterLinkViewModel.FromOperationViewModels.Where(vvm => vvm.OperationModel is IFilterProviderOperationModel).Where(
                vvm => filterLinkViewModel.FilterLinkModels.Where(lvm => lvm.LinkType == linkType).Select(lvm => lvm.FromOperationModel).Contains(vvm.OperationModel as IFilterProviderOperationModel)))
            {
                Vec incomingCenter = new Vec(
                    (incomingModel.Position + incomingModel.Size / 2.0).X,
                    (incomingModel.Position + incomingModel.Size / 2.0).Y);
                var incomingRct = new Rct(incomingModel.Position,
                    new Vec(incomingModel.Size.X, incomingModel.Size.Y));

                var inter =
                    incomingRct.GetLineString()
                        .Intersection(
                            new Point[] { attachmentCenter.GetCoord().GetPt(), incomingCenter.GetCoord().GetPt() }
                                .GetLineString());
                Vec incomingStart = new Vec();

                if (inter.IsEmpty)
                {
                    incomingStart = incomingCenter;
                }
                else
                {
                    incomingStart = new Vec(inter.Centroid.X, inter.Centroid.Y);
                }
                Vec distanceVec = (attachmentCenter - incomingStart);
                if (distanceVec.Length > 0)
                {
                    Vec cutOff = distanceVec.Normalized() * (distanceVec.Length - _attachmentRectHalfSize);

                    Line l1 = new Line();
                    l1.X1 = incomingStart.X;
                    l1.Y1 = incomingStart.Y;
                    l1.X2 = incomingStart.X + cutOff.X;
                    l1.Y2 = incomingStart.Y + cutOff.Y;
                    if (filterLinkViewModel.FilterLinkModels.Where(lm => lm.IsInverted).Select(lm => lm.FromOperationModel).Contains(incomingModel.OperationModel as IFilterProviderOperationModel))
                    {
                        l1.StrokeDashArray = new DoubleCollection() { 2 };
                    }
                    l1.Stroke = _lightBrush;
                    l1.StrokeThickness = 2;
                    c.Children.Add(l1);


                    Vec n = distanceVec.Perp().Normalized();
                    Vec trianglePos = (distanceVec.Normalized() * (distanceVec.Length * 0.3)) + incomingStart;

                    Polygon poly = new Polygon();
                    poly.Points.Add(new Point(trianglePos.X + n.X * 8, trianglePos.Y + n.Y * 8));
                    poly.Points.Add(new Point(trianglePos.X - n.X * 8, trianglePos.Y - n.Y * 8));
                    poly.Points.Add(new Point(trianglePos.X + distanceVec.Normalized().X * 20,
                        trianglePos.Y + distanceVec.Normalized().Y * 20));
                    poly.Points.Add(new Point(trianglePos.X + n.X * 8, trianglePos.Y + n.Y * 8));
                    poly.Fill = _lightBrush;
                    c.Children.Add(poly);

                    _visualizationViewModelCenterGeometries.Add(incomingModel,
                        poly.Points.GetPolygon().Buffer(3));
                    _visualizationViewModelGeometries.Add(incomingModel,
                        new Point[] { incomingStart.GetCoord().GetPt(), (incomingStart + cutOff).GetCoord().GetPt() }
                            .GetLineString());



                    /*Vec iconPos = (distanceVec.Normalized() * (distanceVec.Length * 0.2)) + incomingStart;
                    Canvas brushCanvas = new Canvas();
                    Ellipse e = new Ellipse();
                    e.Width = 30;
                    e.Height = 30;
                    e.Fill = _lightBrush;
                    e.Stroke = _lightBrush;
                    e.StrokeThickness = 2;

                    if (linkType == LinkType.Brush)
                    {
                        Canvas pathCanvas = new Canvas();
                        var p1 = new Path();
                        p1.Fill = _lightBrush;
                        var b = new Binding
                        {
                            Source = "m 0,0 c 0.426,0 0.772,-0.346 0.772,-0.773 0,-0.426 -0.346,-0.772 -0.772,-0.772 -0.427,0 -0.773,0.346 -0.773,0.772 C -0.773,-0.346 -0.427,0 0,0 m -9.319,11.674 c 0,0 7.188,0.868 7.188,-7.187 l 0,-5.26 c 0,-1.618 1.175,-1.888 2.131,-1.888 0,0 1.914,-0.245 1.871,1.87 l 0,5.246 c 0,0 0.214,7.219 7.446,7.219 l 0,2.21 -18.636,0 0,-2.21 z"
                        };
                        BindingOperations.SetBinding(p1, Path.DataProperty, b);
                        var tg = new TransformGroup();
                        tg.Children.Add(new ScaleTransform() { ScaleX = 1, ScaleY = -1 });
                        tg.Children.Add(new TranslateTransform() { X = 9.3, Y = 26 });
                        p1.RenderTransform = tg;
                        pathCanvas.Children.Add(p1);

                        var p2 = new Path();
                        p2.Fill = _lightBrush;
                        b = new Binding
                        {
                            Source = "m 0,0 0,-0.491 0,-4.316 18.636,0 0,3.58 0,1.227 0,5.333 L 0,5.333 0,0 z"
                        };
                        BindingOperations.SetBinding(p2, Path.DataProperty, b);
                        tg = new TransformGroup();
                        tg.Children.Add(new ScaleTransform() { ScaleX = 1, ScaleY = -1 });
                        tg.Children.Add(new TranslateTransform() { X = 0, Y = 6 });
                        p2.RenderTransform = tg;
                        pathCanvas.Children.Add(p2);

                        tg = new TransformGroup();
                        tg.Children.Add(new ScaleTransform() { ScaleX = 0.7, ScaleY = 0.7 });
                        tg.Children.Add(new TranslateTransform() { X = 9, Y = 6 });
                        pathCanvas.RenderTransform = tg;

                        brushCanvas.Children.Add(e);
                        brushCanvas.Children.Add(pathCanvas);

                    }
                    else if (linkType == LinkType.Filter)
                    {
                        Polygon filterIcon = new Polygon();
                        filterIcon.Points = new PointCollection();
                        filterIcon.Points.Add(new Point(0, 0));
                        filterIcon.Points.Add(new Point(15, 0));
                        filterIcon.Points.Add(new Point(10, 6));
                        filterIcon.Points.Add(new Point(10, 14));
                        filterIcon.Points.Add(new Point(5, 12));
                        filterIcon.Points.Add(new Point(5, 6));
                        filterIcon.Fill = _highlightBrush;
                        filterIcon.Width = _attachmentRectHalfSize * 2;
                        filterIcon.Height = _attachmentRectHalfSize * 2;
                        Mat mat = Mat.Translate(6, 8) * Mat.Scale(1.2, 1.2);
                        filterIcon.RenderTransform = new MatrixTransform() { Matrix = mat };

                        brushCanvas.Children.Add(e);
                        brushCanvas.Children.Add(filterIcon);
                    }
                    brushCanvas.RenderTransform = new TranslateTransform() { X = iconPos.X - 15, Y = iconPos.Y - 15 };
                    c.Children.Add(brushCanvas);

                    Rect rr = new Rct(iconPos.X - 15, iconPos.Y - 15, iconPos.X + 15, iconPos.Y + 15);
                    _visualizationViewModelIconGeometries.Add(incomingModel,
                        rr.GetPolygon());*/
                }

            }
        }

        private void drawFilterAttachment(Vec attachmentCenter, Canvas c)
        {
            FilterLinkViewModel filterLinkViewModel = (DataContext as FilterLinkViewModel);
            int sourceCount = filterLinkViewModel.FilterLinkModels.Count(lvm => lvm.LinkType == LinkType.Filter);

            Rectangle c1 = new Rectangle();
            c1.Width = _attachmentRectHalfSize * 2;
            c1.Height = _attachmentRectHalfSize * 2;

            c1.RenderTransform = new TranslateTransform()
            {
                X = attachmentCenter.X - _attachmentRectHalfSize,
                Y = attachmentCenter.Y - _attachmentRectHalfSize
            };
            c.Children.Add(c1);

            c1.Fill = _lightBrush;
            c1.Stroke = _lightBrush;
            c1.StrokeThickness = 2;

            Polygon filterIcon = new Polygon();
            filterIcon.Points = new PointCollection();
            filterIcon.Points.Add(new Point(0, 0));
            filterIcon.Points.Add(new Point(15, 0));
            filterIcon.Points.Add(new Point(10, 6));
            filterIcon.Points.Add(new Point(10, 14));
            filterIcon.Points.Add(new Point(5, 12));
            filterIcon.Points.Add(new Point(5, 6));
            filterIcon.Fill = sourceCount > 1 ? _highlightFaintBrush : _highlightBrush;
            filterIcon.Width = _attachmentRectHalfSize * 2;
            filterIcon.Height = _attachmentRectHalfSize * 2;
            Mat mat = Mat.Translate(attachmentCenter.X - _attachmentRectHalfSize + 6,
                        attachmentCenter.Y - _attachmentRectHalfSize + 7 + (sourceCount > 1 ? 0 : 0)) * 
                      Mat.Scale(1.2, 1.2);
            filterIcon.RenderTransform = new MatrixTransform() { Matrix = mat };
            c.Children.Add(filterIcon);

            if (sourceCount > 1)
            {
                TextBlock label = new TextBlock();
                if (((IFilterConsumerOperationModel) filterLinkViewModel.ToOperationViewModel.OperationModel).FilteringOperation == FilteringOperation.AND)
                {
                    label.Text = "AND";
                }
                else if (((IFilterConsumerOperationModel)filterLinkViewModel.ToOperationViewModel.OperationModel).FilteringOperation == FilteringOperation.OR)
                {
                    label.Text = "OR";
                }
                label.TextAlignment = TextAlignment.Center;
                label.FontSize = 9;
                label.FontWeight = FontWeights.Bold;
                label.Width = _attachmentRectHalfSize * 2;
                label.Foreground = _highlightBrush;
                label.Height = _attachmentRectHalfSize * 2;
                c.UseLayoutRounding = false;
                label.RenderTransform = new TranslateTransform()
                {
                    X = attachmentCenter.X - _attachmentRectHalfSize,
                    Y = attachmentCenter.Y - _attachmentRectHalfSize + 10
                };
                c.Children.Add(label);
            }

            Vec t = (attachmentCenter - new Vec(_attachmentRectHalfSize, _attachmentRectHalfSize));
            Rct r = new Rct(new Pt(t.X, t.Y),
                new Vec(_attachmentRectHalfSize * 2, _attachmentRectHalfSize * 2));
            _linkViewGeometry = r.GetPolygon().Buffer(3);
        }

        private void drawBrushAttachment(Vec attachmentCenter, Canvas c)
        {
            FilterLinkViewModel filterLinkViewModel = (DataContext as FilterLinkViewModel);
            int sourceCount = filterLinkViewModel.FilterLinkModels.Count(lvm => lvm.LinkType == LinkType.Brush);

            Rectangle c1 = new Rectangle();
            c1.Width = _attachmentRectHalfSize * 2;
            c1.Height = _attachmentRectHalfSize * 2;

            c1.RenderTransform = new TranslateTransform()
            {
                X = attachmentCenter.X - _attachmentRectHalfSize,
                Y = attachmentCenter.Y - _attachmentRectHalfSize
            };
            c.Children.Add(c1);

            c1.Fill = _lightBrush;
            c1.Stroke = _lightBrush;
            c1.StrokeThickness = 2;

            Canvas brushCanvas = new Canvas();
            var p1 = new Path();
            p1.Fill = _lightBrush;
            var b = new Binding
            {
                Source = "m 0,0 c 0.426,0 0.772,-0.346 0.772,-0.773 0,-0.426 -0.346,-0.772 -0.772,-0.772 -0.427,0 -0.773,0.346 -0.773,0.772 C -0.773,-0.346 -0.427,0 0,0 m -9.319,11.674 c 0,0 7.188,0.868 7.188,-7.187 l 0,-5.26 c 0,-1.618 1.175,-1.888 2.131,-1.888 0,0 1.914,-0.245 1.871,1.87 l 0,5.246 c 0,0 0.214,7.219 7.446,7.219 l 0,2.21 -18.636,0 0,-2.21 z"
            };
            BindingOperations.SetBinding(p1, Path.DataProperty, b);
            var tg = new TransformGroup();
            tg.Children.Add(new ScaleTransform() { ScaleX = 1, ScaleY = -1 });
            tg.Children.Add(new TranslateTransform() { X = 9.3, Y = 26 });
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
            tg.Children.Add(new ScaleTransform() { ScaleX = 1, ScaleY = -1 });
            tg.Children.Add(new TranslateTransform() { X = 0, Y = 6 });
            p2.RenderTransform = tg;
            brushCanvas.Children.Add(p2);

            //BrushIcon brushIcon = new BrushIcon();
            //brushIcon.SetBrush(sourceCount > 1 && false ? Destination.FaintBrush : Destination.Brush);
            Matrix mat =  Mat.Translate(
                          attachmentCenter.X - _attachmentRectHalfSize + 7,
                          attachmentCenter.Y - _attachmentRectHalfSize + 7) * Mat.Scale(0.80, 0.80) * Mat.Rotate(new Deg(45), new Pt(10, 10));
            brushCanvas.RenderTransform = new MatrixTransform() { Matrix = mat };
            c.Children.Add(brushCanvas);

            Vec t = (attachmentCenter - new Vec(_attachmentRectHalfSize, _attachmentRectHalfSize));
            Rct r = new Rct(new Pt(t.X, t.Y),
                new Vec(_attachmentRectHalfSize * 2, _attachmentRectHalfSize * 2));
            _linkViewGeometry = r.GetPolygon().Buffer(3);
        }

        void LinkView_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            IPoint p = e.GetCurrentPoint(MainViewController.Instance.InkableScene).Position.GetVec().GetCoord().GetPoint();
            FilterLinkViewModel filterLinkViewModel = (DataContext as FilterLinkViewModel);

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
                FilteringOperation op = ((IFilterConsumerOperationModel) filterLinkViewModel.ToOperationViewModel.OperationModel).FilteringOperation;
                ((IFilterConsumerOperationModel)filterLinkViewModel.ToOperationViewModel.OperationModel).FilteringOperation = op == FilteringOperation.AND ? FilteringOperation.OR : FilteringOperation.AND;
                e.Handled = true;
                foreach (var linkModel in filterLinkViewModel.FilterLinkModels)
                {
                    linkModel.ToOperationModel.FireOperationModelUpdated(new FilterOperationModelUpdatedEventArgs(FilterOperationModelUpdatedEventType.Links));
                }
            }
        }
        public GeoAPI.Geometries.IGeometry Geometry
        {
            get
            {
                FilterLinkViewModel filterLinkViewModel = (DataContext as FilterLinkViewModel);
                if (filterLinkViewModel.FilterLinkModels.Count > 0)
                {
                    IGeometry unionGeometry = _linkViewGeometry;

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
                else
                {
                    return new NetTopologySuite.Geometries.Point(-40000, -40000);
                }
            }
        }

        public List<IScribbable> Children
        {
            get
            {
                return new List<IScribbable>();
            }
        }

        public bool Consume(InkStroke inkStroke)
        {
            FilterLinkViewModel filterLinkViewModel = (DataContext as FilterLinkViewModel);
            List<FilterLinkModel> models = new List<FilterLinkModel>();
            foreach (var model in _visualizationViewModelGeometries.Keys.ToArray())
            {
                if (_visualizationViewModelGeometries[model].Buffer(3).Intersects(inkStroke.GetLineString()))
                {
                    var linkModel = (filterLinkViewModel.FilterLinkModels.First(lm => lm.FromOperationModel == model.OperationModel));
                    linkModel.IsInverted = !linkModel.IsInverted;
                    linkModel.ToOperationModel.FireOperationModelUpdated(new FilterOperationModelUpdatedEventArgs(FilterOperationModelUpdatedEventType.Links));
                }
            }
            return true;
        }

        public bool IsDeletable
        {
            get { return true; }
        }

        public List<FilterLinkModel> GetLinkModelsToRemove(IGeometry scribble)
        {
            FilterLinkViewModel filterLinkViewModel = (DataContext as FilterLinkViewModel);
            List<FilterLinkModel> models = new List<FilterLinkModel>();
            if (scribble.Intersects(_linkViewGeometry.Buffer(3)))
            {
                models = filterLinkViewModel.FilterLinkModels.ToList();
            }
            else
            {
                foreach (var model in _visualizationViewModelGeometries.Keys)
                {
                    if (_visualizationViewModelGeometries[model].Buffer(3).Intersects(scribble))
                    {
                        models.Add(filterLinkViewModel.FilterLinkModels.First(lm => lm.FromOperationModel == model.OperationModel));
                    }
                }
            }
            return models;
        }
    }
}
