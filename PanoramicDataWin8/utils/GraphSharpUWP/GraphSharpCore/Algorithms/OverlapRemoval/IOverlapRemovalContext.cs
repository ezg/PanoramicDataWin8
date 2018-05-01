using System.Collections.Generic;
using System.Windows;
using Windows.Foundation;

namespace GraphSharp.Algorithms.OverlapRemoval
{
	public interface IOverlapRemovalContext<TVertex>
	{
		IDictionary<TVertex, Rect> Rectangles { get; }
	}
}