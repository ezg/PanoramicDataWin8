﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    ///     For those graph engines that do not support the low-level querying of the vertices or edges, then DefaultQuery can be used.
    ///     DefaultQuery assumes, at minimum, that Graph.getVertices() and Graph.getEdges() is implemented by the respective Graph.
    /// </summary>
    public class DefaultGraphQuery : DefaultQuery
    {
        private readonly IGraph _graph;

        public DefaultGraphQuery(IGraph graph)
        {
            Contract.Requires(graph != null);

            _graph = graph;
        }

        public override IEnumerable<IEdge> Edges()
        {
            return new DefaultGraphQueryIterable<IEdge>(this, GetElementIterable<IEdge>(typeof (IEdge)));
        }

        public override IEnumerable<IVertex> Vertices()
        {
            return new DefaultGraphQueryIterable<IVertex>(this, GetElementIterable<IVertex>(typeof (IVertex)));
        }

        private IEnumerable<T> GetElementIterable<T>(Type elementClass) where T : IElement
        {
            Contract.Requires(elementClass != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            if (_graph is IKeyIndexableGraph)
            {
                var keys = (_graph as IKeyIndexableGraph).GetIndexedKeys(elementClass).ToArray();
                foreach (
                    var hasContainer in
                        HasContainers.Where(
                            hasContainer =>
                            hasContainer.Compare == Compare.Equal && hasContainer.Value != null &&
                            keys.Contains(hasContainer.Key)))
                {
                    Debug.WriteLine("Possible error GetElementIterable");
                    //if (typeof (IVertex).IsAssignableFrom(elementClass))
                    //    return (IEnumerable<T>) _graph.GetVertices(hasContainer.Key, hasContainer.Value);
                    return (IEnumerable<T>) _graph.GetEdges(hasContainer.Key, hasContainer.Value);
                }
            }

            foreach (var hasContainer in HasContainers.Where(hasContainer => hasContainer.Compare == Compare.Equal))
            {
                Debug.WriteLine("Possible error GetElementIterable2");
                //if (typeof (IVertex).IsAssignableFrom(elementClass))
                //    return (IEnumerable<T>) _graph.GetVertices(hasContainer.Key, hasContainer.Value);
                return (IEnumerable<T>) _graph.GetEdges(hasContainer.Key, hasContainer.Value);
            }

            Debug.WriteLine("Possible error GetElementIterable3");
            //return typeof(IVertex).IsAssignableFrom(elementClass)
            //           ? (IEnumerable<T>)_graph.GetVertices()
            //           : (IEnumerable<T>)_graph.GetEdges();
            return (IEnumerable<T>)_graph.GetEdges();
        }

        private class DefaultGraphQueryIterable<T> : IEnumerable<T> where T : IElement
        {
            private readonly DefaultGraphQuery _defaultQuery;
            private readonly IEnumerable<T> _iterable;
            private long _count;
            private T _nextElement;

            public DefaultGraphQueryIterable(DefaultGraphQuery defaultQuery, IEnumerable<T> iterable)
            {
                Contract.Requires(defaultQuery != null);
                Contract.Requires(iterable != null);

                _defaultQuery = defaultQuery;
                _iterable = iterable;
            }

            public IEnumerator<T> GetEnumerator()
            {
                while (LoadNext()) yield return _nextElement;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private bool LoadNext()
            {
                _nextElement = default(T);
                if (_count >= _defaultQuery.Innerlimit)
                    return false;

                foreach (var element in _iterable)
                {
                    var filter = _defaultQuery.HasContainers.Any(hasContainer => !hasContainer.IsLegal(element));

                    if (filter) continue;
                    _nextElement = element;
                    _count++;
                    return true;
                }
                return false;
            }
        }
    }
}