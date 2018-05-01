using System.Linq;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GraphSharp.Controls;

namespace GraphSharp.Sample
{
    /// <summary>
    /// Main window of the Proof of Concept application for the GraphLayout control.
    /// </summary>
    public partial class MainWindow
    {
        static public MainWindow instance = null;
        public MainWindow()
        {
            InitializeComponent();
            instance = this;

            var graph = new PocGraph();

            for (int i = 0; i < 8; i++)
            {
                var v = new PocVertex(i.ToString());
                graph.AddVertex(v);
            }

            graph.AddEdge(new PocEdge("0to1", graph.Vertices.ElementAt(0), graph.Vertices.ElementAt(1)));
            graph.AddEdge(new PocEdge("1to2", graph.Vertices.ElementAt(1), graph.Vertices.ElementAt(2)));
            graph.AddEdge(new PocEdge("2to3", graph.Vertices.ElementAt(2), graph.Vertices.ElementAt(3)));
            graph.AddEdge(new PocEdge("2to4", graph.Vertices.ElementAt(2), graph.Vertices.ElementAt(4)));
            graph.AddEdge(new PocEdge("0to5", graph.Vertices.ElementAt(0), graph.Vertices.ElementAt(5)));
            graph.AddEdge(new PocEdge("1to7", graph.Vertices.ElementAt(1), graph.Vertices.ElementAt(7)));
            graph.AddEdge(new PocEdge("4to6", graph.Vertices.ElementAt(4), graph.Vertices.ElementAt(6)));
            graph.AddEdge(new PocEdge("7to4", graph.Vertices.ElementAt(7), graph.Vertices.ElementAt(4)));

            DataContext = new GraphLayoutViewModel() { Graph = graph, LayoutAlgorithmType = "Tree" };

            var disp = new DispatcherTimer();
            disp.Interval = new System.TimeSpan(0, 0, 5);
            disp.Tick += Disp_Tick1;
            disp.Start();
        }

        private void Disp_Tick1(object sender, object e)
        {
            var g = graphLayout.Graph;
            graphLayout.LayoutAlgorithmType = "EfficientSugiyama";

            //var graph = new PocGraph();

            //for (int i = 0; i < 5; i++)
            //{
            //    var v = new PocVertex(i.ToString());
            //    graph.AddVertex(v);
            //}

            //graph.AddEdge(new PocEdge("0to1", graph.Vertices.ElementAt(0), graph.Vertices.ElementAt(1)));
            //graph.AddEdge(new PocEdge("1to2", graph.Vertices.ElementAt(1), graph.Vertices.ElementAt(2)));
            //graph.AddEdge(new PocEdge("2to3", graph.Vertices.ElementAt(2), graph.Vertices.ElementAt(3)));
            //graph.AddEdge(new PocEdge("2to4", graph.Vertices.ElementAt(2), graph.Vertices.ElementAt(4)));
            //DataContext = new GraphLayoutViewModel() { Graph = graph, LayoutAlgorithmType = "Tree" };
        }
    }
}