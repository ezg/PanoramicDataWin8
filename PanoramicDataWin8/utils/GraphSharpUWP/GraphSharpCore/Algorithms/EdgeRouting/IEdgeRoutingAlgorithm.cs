using System.Collections.Generic;
using QuickGraph.Algorithms;
using QuickGraph;
using System.Windows;
using Windows.Foundation;

namespace GraphSharp.Algorithms.EdgeRouting
{
	public interface IEdgeRoutingAlgorithm<TVertex, TEdge, TGraph> : IAlgorithm<TGraph>
		where TEdge : IEdge<TVertex>
		where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
	{
		IDictionary<TEdge, Point[]> EdgeRoutes { get; }
	}
}