using PanoramicData.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI;

namespace PanoramicData.view.inq
{
    public class InkStroke : IScribbable
    {
        public Color Color = Colors.Black;
        private ObservableCollection<Point> _points = null;

        public InkStroke(IList<Point> points)
        {
            _points = new ObservableCollection<Point>(points);
            updateStroke();
        }

        public InkStroke(IList<Vec> points)
        {
            _points = new ObservableCollection<Point>();
            foreach (Vec p in points)
            {
                _points.Add(new Point(p.X, p.Y));
            }
        }

        public InkStroke()
        {
            _points = new ObservableCollection<Point>();
        }

        public static implicit operator InkStrokeElement(InkStroke s) { return new InkStrokeElement(s); }

        public static implicit operator InkStroke(InkStrokeElement se) { return se.InkStroke; }

        public ObservableCollection<Point> Points
        {
            get { return _points; }
        }

        public void Add(Point point)
        {
            _points.Add(point);
        }

        // TODO: Cache bounding rect
        public Rect BoundingRect
        {
            get
            {
                double minX = double.MaxValue;
                double maxX = double.MinValue;
                double minY = double.MaxValue;
                double maxY = double.MinValue;
                foreach (Point point in _points)
                {
                    if (point.X < minX)
                        minX = point.X;
                    if (point.X > maxX)
                        maxX = point.X;
                    if (point.Y < minY)
                        minY = point.Y;
                    if (point.Y > maxY)
                        maxY = point.Y;
                }
                return new Rect(minX, minY, maxX - minX, maxY - minY);
            }
        }

        private void updateStroke()
        {
        }

        public InkStroke GetTranslated(Point offset)
        {
            InkStroke s = Clone();
            s.Translate(offset);
            return s;
        }

        public void ClipTo(Rect rect)
        {
            List<Point> newList = _points.Where(rect.Contains).ToList();
            _points = new ObservableCollection<Point>(newList);
            updateStroke();
        }


        public void Translate(Point offset)
        {
            for (int i = 0; i < _points.Count; ++i)
            {
                _points[i] = new Point(offset.X + _points[i].X, offset.Y + _points[i].Y);
            }
            updateStroke();
        }

        public void Scale(double scale)
        {
            for (int i = 0; i < _points.Count; ++i)
            {
                _points[i] = new Point(_points[i].X * scale, _points[i].Y * scale);
            }
            updateStroke();
        }

        public InkStroke GetResampled(int samples)
        {
            if (samples >= Points.Distinct().Count())
            {
                return this.Clone();
            }
            InkStroke s = Clone();
            s.Resample(samples);
            return s;
        }

        public void Resample(int samples)
        {
            double S = Length() / (samples - 1) - 0.001;
            double D = 0.0, d; // D is the distance accumulator of consecutive points, when D < S

            IList<Point> points = Clone().Points;
            var newPoints = new List<Point>();
            newPoints.Add(points[0]);
            int i, c = 0;

            for (i = 1; i < points.Count; i++)
            {
                d = MathUtil.Distance(points[i - 1], points[i]);

                if (D + d >= S)
                {
                    c = c + 1;
                    var q = new Point();
                    q.X = points[i - 1].X + ((S - D) / d) * (points[i].X - points[i - 1].X);
                    q.Y = points[i - 1].Y + ((S - D) / d) * (points[i].Y - points[i - 1].Y);
                    newPoints.Add(q);
                    points.Insert(i, q);

                    D = 0.0;
                }
                else
                    D += d;
            }
            _points = new ObservableCollection<Point>(newPoints);
        }

        public float GetAveragePointDistance()
        {
            return Length() / _points.Count;
        }

        public float Length()
        {
            float d = 0;
            for (int i = 1; i < _points.Count; i++)
            {
                d += MathUtil.Distance(_points[i - 1], _points[i]);
            }
            return d;
        }

        public InkStroke Clone()
        {
            var s = new InkStroke();
            foreach (Point point in _points)
            {
                s.Points.Add(new Point(point.X, point.Y));
            }

            return s;
        }

        public GeoAPI.Geometries.IPolygon GetPolygon()
        {
            GeoAPI.Geometries.Coordinate[] coords;

            if (this.Points.Count >= 3)
            {
                coords = new GeoAPI.Geometries.Coordinate[this.Points.Count + 1];
                int i = 0;
                foreach (Point pt in this.Points)
                {
                    coords[i] = new GeoAPI.Geometries.Coordinate(pt.X, pt.Y);
                    i++;
                }
                coords[i] = new GeoAPI.Geometries.Coordinate(this.Points[0].X, this.Points[0].Y);
            }
            else
            {
                coords = new GeoAPI.Geometries.Coordinate[4];
                coords[0] = new GeoAPI.Geometries.Coordinate(this.Points[0].X, this.Points[0].Y);
                coords[1] = new GeoAPI.Geometries.Coordinate(this.Points[0].X + 1, this.Points[0].Y);
                coords[2] = new GeoAPI.Geometries.Coordinate(this.Points[0].X + 1, this.Points[0].Y + 1);
                coords[3] = new GeoAPI.Geometries.Coordinate(this.Points[0].X, this.Points[0].Y);
            }


            return new NetTopologySuite.Geometries.Polygon(new NetTopologySuite.Geometries.LinearRing(coords));
        }

        public GeoAPI.Geometries.ILineString GetLineString()
        {
            GeoAPI.Geometries.Coordinate[] coords;
            if (this.Points.Count > 1)
            {
                coords = new GeoAPI.Geometries.Coordinate[this.Points.Count];
                int i = 0;
                foreach (Point pt in this.Points)
                {
                    coords[i] = new GeoAPI.Geometries.Coordinate(pt.X, pt.Y);
                    i++;
                }
            }
            else
            {
                coords = new GeoAPI.Geometries.Coordinate[2];
                coords[0] = new GeoAPI.Geometries.Coordinate(this.Points[0].X, this.Points[0].Y);
                coords[1] = new GeoAPI.Geometries.Coordinate(this.Points[0].X, this.Points[0].Y + 1);
            }
            return new NetTopologySuite.Geometries.LineString(coords);
        }

        public GeoAPI.Geometries.IGeometry Geometry
        {
            get
            {
                return this.GetLineString();
            }
        }

        public List<IScribbable> Children
        {
            get
            {
                return new List<IScribbable>();
            }
        }
    }
}
