using System.Collections.Generic;
using System.Windows;
using Windows.Foundation;

namespace GraphSharp.Algorithms.Layout.Compound
{
    public interface ICompoundLayoutIterationEventArgs<TVertex> 
        : ILayoutIterationEventArgs<TVertex>
        where TVertex : class
    {
        IDictionary<TVertex, Size> InnerCanvasSizes { get; }
    }
}
