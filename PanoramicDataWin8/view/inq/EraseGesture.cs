using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using GeoAPI.Geometries;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.view.inq
{
    public class EraseGesture : HitGesture
    {
        private InkableScene _inkableScene = null;

        public EraseGesture(InkableScene inkableScene)
        {
            this._inkableScene = inkableScene;
        }

        public override bool Recognize(InkStroke inkStroke)
        {
            _hitScribbables = new List<IScribbable>();
            if (inkStroke.IsErase)
            {
                inkStroke = inkStroke.GetResampled(20);
                ILineString inkStrokeLine = inkStroke.GetLineString();
                IList<Vec> convexHull = Convexhull.convexhull(inkStroke.Points);
                IGeometry convexHullPoly = convexHull.Select(vec => new Point(vec.X, vec.Y)).ToList().GetPolygon();

                foreach (IScribbable existingInkStroke in _inkableScene.InkStrokes)
                {
                    IGeometry geom = existingInkStroke.Geometry;
                    if (geom != null)
                    {
                        if (inkStrokeLine.Intersects(geom))
                        {
                            _hitScribbables.Add(existingInkStroke);
                        }

                        // Check for small exisiting strokes that are completely covered by the scribble.
                        if (!_hitScribbables.Contains(existingInkStroke) && convexHullPoly.Contains(geom.Buffer(1)))
                        {
                            _hitScribbables.Add(existingInkStroke);
                        }
                    }
                }
                List<IScribbable> allScribbables = new List<IScribbable>();
                IScribbleHelpers.GetScribbablesRecursive(allScribbables, _inkableScene.Elements.OfType<IScribbable>().ToList());
                allScribbables = allScribbables.Where(e => e.IsDeletable).ToList();
                foreach (IScribbable existingScribbable in allScribbables)
                {
                    IGeometry geom = existingScribbable.Geometry;
                    if (geom != null)
                    {
                        /*Polygon p = new Polygon();
                        PointCollection pc = new PointCollection(existingScribbable.Geometry.Coordinates.Select(c => new System.Windows.Point(c.X, c.Y)));
                        p.Points = pc;
                        p.Stroke = Brushes.Blue;
                        p.StrokeThickness = 5;
                        _inkableScene.Add(p);*/

                        if (inkStrokeLine.Intersects(geom))
                        {
                            _hitScribbables.Add(existingScribbable);
                        }
                    }
                }

                if (_hitScribbables.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
