using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PanoramicDataWin8.utils
{
    public static class CommonExtensions
    {
        public static Vec GetVec(this GeoAPI.Geometries.Coordinate c)
        {
            return new Vec(c.X, c.Y);
        }

        public static Point GetWindowsPoint(this GeoAPI.Geometries.Coordinate c)
        {
            return new Point(c.X, c.Y);
        }

        public static Point GetWindowsPoint(this GeoAPI.Geometries.IPoint c)
        {
            return new Point(c.X, c.Y);
        }

        public static Point GetWindowsPoint(this Vec c)
        {
            return new Point(c.X, c.Y);
        }

        public static Vec GetVec(this Point c)
        {
            return new Vec(c.X, c.Y);
        }

        public static Vec GetVec(this GeoAPI.Geometries.IPoint c)
        {
            return new Vec(c.X, c.Y);
        }

        public static Vec GetVec(this Pt c)
        {
            return new Vec(c.X, c.Y);
        }

        public static Vector2 ToVector2(double d1, double d2)
        {
            return new Vector2((float) d1, (float) d2);
        }

        public static GeoAPI.Geometries.IPoint GetPoint(this Point c)
        {
            return new NetTopologySuite.Geometries.Point(c.X, c.Y);
        }

        public static GeoAPI.Geometries.IPoint GetPoint(this Pt c)
        {
            return new NetTopologySuite.Geometries.Point(c.X, c.Y);
        }

        public static GeoAPI.Geometries.IPoint GetPoint(this GeoAPI.Geometries.Coordinate c)
        {
            return new NetTopologySuite.Geometries.Point(c.X, c.Y);
        }

        public static Pt GetPt(this GeoAPI.Geometries.Coordinate c)
        {
            return new Pt(c.X, c.Y);
        }

        public static GeoAPI.Geometries.Coordinate GetCoord(this Vec c)
        {
            return new GeoAPI.Geometries.Coordinate(c.X, c.Y);
        }

        public static GeoAPI.Geometries.ILineString GetLineString(this IEnumerable<Point> s)
        {
            GeoAPI.Geometries.Coordinate[] coords;
            coords = new GeoAPI.Geometries.Coordinate[s.Count()];
            int i = 0;
            foreach (Point pt in s)
            {
                coords[i] = new GeoAPI.Geometries.Coordinate(pt.X, pt.Y);
                i++;
            }

            return new NetTopologySuite.Geometries.LineString(coords);
        }

        public static IEnumerable<Pt> GetPoints(this Rct r)
        {
            List<Pt> pts = new List<Pt>();

            pts.Add(r.TopLeft);
            pts.Add(r.TopRight);
            pts.Add(r.BottomRight);
            pts.Add(r.BottomLeft);
            pts.Add(r.TopLeft);

            return pts;
        }

        public static GeoAPI.Geometries.ILineString GetLineString(this Rct r)
        {
            GeoAPI.Geometries.Coordinate[] coords;
            coords = new GeoAPI.Geometries.Coordinate[5];
            coords[0] = new GeoAPI.Geometries.Coordinate(r.TopLeft.X, r.TopLeft.Y);
            coords[1] = new GeoAPI.Geometries.Coordinate(r.TopRight.X, r.TopRight.Y);
            coords[2] = new GeoAPI.Geometries.Coordinate(r.BottomRight.X, r.BottomRight.Y);
            coords[3] = new GeoAPI.Geometries.Coordinate(r.BottomLeft.X, r.BottomLeft.Y);
            coords[4] = new GeoAPI.Geometries.Coordinate(r.TopLeft.X, r.TopLeft.Y);
            return new NetTopologySuite.Geometries.LineString(coords);
        }

        public static GeoAPI.Geometries.IPolygon GetPolygon(this IEnumerable<Pt> s)
        {
            GeoAPI.Geometries.Coordinate[] coords;
            coords = new GeoAPI.Geometries.Coordinate[s.Count() + 1];
            int i = 0;
            foreach (Point pt in s)
            {
                coords[i] = new GeoAPI.Geometries.Coordinate(pt.X, pt.Y);
                i++;
            }
            coords[i] = new GeoAPI.Geometries.Coordinate(s.First().X, s.First().Y);

            return new NetTopologySuite.Geometries.Polygon(new NetTopologySuite.Geometries.LinearRing(coords));
        }

        public static GeoAPI.Geometries.IPolygon GetPolygon(this IEnumerable<Point> s)
        {
            GeoAPI.Geometries.Coordinate[] coords;
            if (s.Count() > 3)
            {
                coords = new GeoAPI.Geometries.Coordinate[s.Count() + 1];
                int i = 0;
                foreach (Point pt in s)
                {
                    coords[i] = new GeoAPI.Geometries.Coordinate(pt.X, pt.Y);
                    i++;
                }
                coords[i] = new GeoAPI.Geometries.Coordinate(s.First().X, s.First().Y);
            }
            else
            {
                coords = new GeoAPI.Geometries.Coordinate[5];
                coords[0] = new GeoAPI.Geometries.Coordinate(s.First().X, s.First().Y);
                coords[1] = new GeoAPI.Geometries.Coordinate(s.First().X + 1, s.First().Y);
                coords[2] = new GeoAPI.Geometries.Coordinate(s.First().X + 1, s.First().Y + 1);
                coords[3] = new GeoAPI.Geometries.Coordinate(s.First().X, s.First().Y + 1);
                coords[4] = new GeoAPI.Geometries.Coordinate(s.First().X, s.First().Y);
            }

            return new NetTopologySuite.Geometries.Polygon(new NetTopologySuite.Geometries.LinearRing(coords));
        }

        public static GeoAPI.Geometries.IPolygon GetPolygon(this Rct r)
        {
            return ((Rect)r).GetPolygon();
        }

        public static GeoAPI.Geometries.IPolygon GetPolygon(this Rect r)
        {
            GeoAPI.Geometries.Coordinate[] coords;
            coords = new GeoAPI.Geometries.Coordinate[5];

            coords[0] = new GeoAPI.Geometries.Coordinate(r.Left, r.Top);
            coords[1] = new GeoAPI.Geometries.Coordinate(r.Right, r.Top);
            coords[2] = new GeoAPI.Geometries.Coordinate(r.Right, r.Bottom);
            coords[3] = new GeoAPI.Geometries.Coordinate(r.Left, r.Bottom);
            coords[4] = new GeoAPI.Geometries.Coordinate(r.Left, r.Top);

            return new NetTopologySuite.Geometries.Polygon(new NetTopologySuite.Geometries.LinearRing(coords));
        }

        public static bool ContainsByReference<T>(this List<T> list, T item)
        {
            return list.Any(x => object.ReferenceEquals(x, item));
        }

        public static void RemoveByReference<T>(this List<T> list, T item)
        {
            list.RemoveAll(x => object.ReferenceEquals(x, item));
        }

        public static bool RemoveFirstByReference<T>(this List<T> list, T item)
        {
            var index = -1;
            for (int i = 0; i < list.Count; i++)
                if (object.ReferenceEquals(list[i], item))
                {
                    index = i;
                    break;
                }
            if (index == -1)
                return false;

            list.RemoveAt(index);
            return true;
        }

        public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            var i = 0;
            foreach (var e in ie) action(e, i++);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> coll)
        {
            var c = new ObservableCollection<T>();
            foreach (var e in coll)
                c.Add(e);
            return c;
        }

        public static void Sort<TSource, TKey>(this Collection<TSource> source, Func<TSource, TKey> keySelector)
        {
            List<TSource> sortedList = source.OrderBy(keySelector).ToList();
            source.Clear();
            foreach (var sortedItem in sortedList)
                source.Add(sortedItem);
        }

        public static string TrimTo(this string input, int length)
        {
            return input.Length > length ? input.Substring(0, length) + "..." : input;
        }

        public static double Square(double x)
        {
            return x * x;
        }

        public static double Distance(Point pt1, Point pt2)
        {
            return Math.Sqrt(Square(pt1.X - pt2.X) + Square(pt1.Y - pt2.Y));
        }

        public static void TranslateBy(FrameworkElement elt, double deltaX, double deltaY)
        {
            Matrix cur = (elt.RenderTransform as MatrixTransform).Matrix;
            Matrix newMat = new Matrix(cur.M11, cur.M12, cur.M21, cur.M22, cur.OffsetX + deltaX, cur.OffsetY + deltaY);
            elt.RenderTransform = new MatrixTransform();
            (elt.RenderTransform as MatrixTransform).Matrix = newMat;
        }

        public static void ScaleBy(FrameworkElement elt, double deltaX, double deltaY)
        {
            Matrix cur = (elt.RenderTransform as MatrixTransform).Matrix;
            Matrix newMat = new Matrix(cur.M11 * deltaX, cur.M12, cur.M21, cur.M22 * deltaY, cur.OffsetX, cur.OffsetY);
            elt.RenderTransform = new MatrixTransform();
            (elt.RenderTransform as MatrixTransform).Matrix = newMat;
        }

        public static Size MeasureString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return new Size(0, 0);
            }
            var TextBlock = new TextBlock()
            {
                Text = s
            };
            TextBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            return new Size(TextBlock.DesiredSize.Width, TextBlock.DesiredSize.Height);
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random(0);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public class TranslationFilter : GeoAPI.Geometries.ICoordinateFilter
    {
        private readonly GeoAPI.Geometries.Coordinate _trans;

        public TranslationFilter(GeoAPI.Geometries.Coordinate trans)
        {
            _trans = trans;
        }
        void GeoAPI.Geometries.ICoordinateFilter.Filter(GeoAPI.Geometries.Coordinate coord)
        {
            coord.X += _trans.X;
            coord.Y += _trans.Y;
        }
    }
}
