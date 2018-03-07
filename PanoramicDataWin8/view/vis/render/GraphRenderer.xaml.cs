using GeoAPI.Geometries;
using IDEA_common.catalog;
using IDEA_common.operations.recommender;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.inq;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using InkStroke = PanoramicDataWin8.view.inq.InkStroke;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class GraphRenderer : Renderer, IScribbable
    {
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

        private void GraphRenderer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var g = GraphOperationViewModel.GraphOperationModel.Graph;
            var verts = g.GetVertices().ToArray();
            var ids = verts.Select((v) => v.Id).ToList();
            var cellsize = (Parent as FrameworkElement).ActualWidth / verts.Count();
            var cellhgt = (Parent as FrameworkElement).ActualHeight / verts.Count();
            var glyphsize = Math.Max(1, cellsize);
            var glyphhgt = Math.Max(1, cellhgt);
            var ecount = 0;
            var mustBuild = xMatrix.Children.Count == 0;
            Ellipse r;
            foreach (var edge in g.GetEdges())
            {
                if (mustBuild)
                    xMatrix.Children.Add(r = new Ellipse() { Width = glyphsize, Height = glyphhgt, Fill = new SolidColorBrush(Colors.Black), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top });
                else r = xMatrix.Children[ecount++] as Ellipse;
                r.RenderTransform = new TranslateTransform()
                {
                    X = ids.IndexOf(edge.GetVertex(Frontenac.Blueprints.Direction.In).Id) * cellsize,
                    Y = ids.IndexOf(edge.GetVertex(Frontenac.Blueprints.Direction.Out).Id) * cellhgt
                };
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
