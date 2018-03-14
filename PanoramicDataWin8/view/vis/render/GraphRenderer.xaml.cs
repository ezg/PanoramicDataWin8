using Frontenac.Blueprints;
using GeoAPI.Geometries;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.inq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using InkStroke = PanoramicDataWin8.view.inq.InkStroke;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class GraphRenderer : Renderer, IScribbable
    {
        bool _sortTop = false;
        bool _sortLeft = false;
        public GraphRenderer()
        {
            this.InitializeComponent();
            this.Loaded += GraphRenderer_Loaded;
        }

        public GraphOperationViewModel GraphOperationViewModel => DataContext as GraphOperationViewModel;

        void GraphRenderer_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= GraphRenderer_SizeChanged;
            SizeChanged += GraphRenderer_SizeChanged;
        }
        
        private void xTop_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            _sortTop = !_sortTop;
            GraphRenderer_SizeChanged(null, null);
        }

        private void xLeft_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            _sortLeft = !_sortLeft;
            GraphRenderer_SizeChanged(null, null);
        }

        IAsyncAction lastTask = null;
        bool abort = false;
        private void GraphRenderer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var g = GraphOperationViewModel.GraphOperationModel.Graph;
            var sortedVerts = g.GetVertices().ToList();
            if (_sortTop)
            {
                sortedVerts.Sort(new Comparison<IVertex>((kp1, kp2) => kp1.GetEdges(Direction.Both).Count() - kp2.GetEdges(Direction.Both).Count()));
                sortedVerts.Reverse();
            }
            var topVerts = sortedVerts;
            var sortedLeftVerts = g.GetVertices().Where((v,i) => i< 100).ToList();
            if (_sortLeft)
            {
                sortedLeftVerts.Sort(new Comparison<IVertex>((kp1, kp2) => kp1.GetEdges(Direction.Both).Count() - kp2.GetEdges(Direction.Both).Count()));
                sortedLeftVerts.Reverse();
            }
            var leftVerts = sortedLeftVerts;
            var topIds    = topVerts.Select((v) => v.Id).ToList();
            var leftIds   = leftVerts.Select((v) => v.Id).ToList();
            var cellhgt   = xMatrix.ActualHeight / leftVerts.Count();
            var cellwid   = xMatrix.ActualWidth  / topVerts.Count();
            var glyphsize = Math.Max(1, cellwid);
            var glyphhgt  = Math.Max(1, cellhgt);
            abort = true;
            if (lastTask == null)
            {
                for (int c = (int)(xMatrix.ActualHeight / 55); c < xLeft.Children.Count; c++)
                    xLeft.Children[c].Visibility  = Visibility.Collapsed;
                for (int c = (int)(xMatrix.ActualWidth / 55); c < xTop.Children.Count; c++)
                    xTop.Children[c].Visibility = Visibility.Collapsed;
                abort = false;
                lastTask = Windows.System.Threading.ThreadPool.RunAsync(async (workItem) =>
                {
                    async Task<bool> layoutLabels(bool xaxis, double dim, Grid grid, List<IVertex> verts, List<object> ids)
                    {
                        double last = double.MinValue;
                        int count = 0;
                        foreach (var v in verts)
                        {
                            if (abort)
                            {
                                abort    = false;
                                lastTask = null;
                                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, new Windows.UI.Core.DispatchedHandler(() => GraphRenderer_SizeChanged(null, null)));
                                return true;
                            }
                            var val = ids.IndexOf(v.Id) * dim;
                            if (val > last)
                            {
                                last = val + 55;
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, new Windows.UI.Core.DispatchedHandler(() =>
                                {
                                    if (val + 25 < (xaxis ? xMatrix.ActualWidth : xMatrix.ActualHeight))
                                    {
                                        var t = grid.Children.ElementAtOrDefault(count) as Grid;
                                        if (count >= grid.Children.Count)
                                        {
                                            var vb = new Viewbox() { Child = new TextBlock(), Width=50, Height=20 };
                                            t = new Grid() { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
                                            t.Children.Add(vb);
                                            grid.Children.Add(t);  
                                        }
                                        t.RenderTransform = new CompositeTransform()
                                        {
                                            Rotation = xaxis ? 90 : 0,
                                            TranslateX = xaxis ? val + 30 : 0,
                                            TranslateY = xaxis ? 0 : val
                                        };
                                        t.Visibility = Visibility.Visible;
                                        ((t.Children.First() as Viewbox).Child as TextBlock).Text = v.GetProperty("label")?.ToString() ?? v.Id.ToString();
                                    }
                                }));
                                count++;
                            }
                        }
                        return false;
                    }
                    async Task<bool> layoutDots()
                    {
                        int ecount = 0;
                        foreach (var edge in g.GetEdges())
                        {
                            if (abort)
                            {
                                abort = false;
                                lastTask = null;
                                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, new Windows.UI.Core.DispatchedHandler(() => GraphRenderer_SizeChanged(null, null)));
                                return true;
                            }
                            var lid = leftIds.IndexOf(edge.GetVertex(Frontenac.Blueprints.Direction.In).Id);
                            var tid = topIds.IndexOf(edge.GetVertex(Frontenac.Blueprints.Direction.Out).Id);
                            if (lid != -1 && tid != -1)
                            {
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, new Windows.UI.Core.DispatchedHandler(() =>
                                {
                                    var dot = xMatrix.Children.ElementAtOrDefault(ecount) as Ellipse;
                                    if (ecount >= xMatrix.Children.Count)
                                        xMatrix.Children.Add(dot = new Ellipse() { Width = glyphsize, Height = glyphhgt, Fill = new SolidColorBrush(Colors.Black), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top });
                                    dot.Visibility = Visibility.Visible;
                                    dot.RenderTransform = new TranslateTransform()
                                    {
                                        X = tid * cellwid,
                                        Y = lid * cellhgt
                                    };
                                }));
                                ecount++;
                            }
                        }
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, new Windows.UI.Core.DispatchedHandler(() =>
                        {
                            for (int ee = ecount; ee < xMatrix.Children.Count; ee++)
                                xMatrix.Children[ee].Visibility = Visibility.Collapsed;
                        }));
                        return false;
                    }
                    var breaking = await layoutLabels(false, cellhgt, xLeft, leftVerts, leftIds);
                    if (!breaking)
                    {
                        breaking = await layoutLabels(true, cellwid, xTop, topVerts, topIds);
                        if (!breaking)
                        {
                            breaking = await layoutDots();
                            if (!breaking)
                            {
                                abort = false;
                                lastTask = null;
                            }
                        }
                    }
                });
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
                var model = this.DataContext as GraphOperationViewModel;

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
            return false;
        }
    }
}
