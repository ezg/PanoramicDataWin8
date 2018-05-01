﻿using System.Collections.Generic;
using QuickGraph;
using System.Windows;
using Windows.UI.Xaml;

namespace GraphSharp.Algorithms.Layout.Compound
{
    public interface ICompoundLayoutContext<TVertex, TEdge, TGraph> : ILayoutContext<TVertex, TEdge, TGraph>
        where TEdge : IEdge<TVertex>
		where TGraph : IBidirectionalGraph<TVertex, TEdge>
    {
        IDictionary<TVertex, Thickness> VertexBorders { get; }
        IDictionary<TVertex, CompoundVertexInnerLayoutType> LayoutTypes { get; }
    }
}
