using GraphSharp.Algorithms.Layout;
using GraphSharp.Controls;
using GraphSharpSampleCore;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GraphSharp.Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
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
            DataContext = new PocGraphViewModel() { Graph = graph, LayoutAlgorithmType = "Tree" };
            InitializeComponent();
        }
    }
}
