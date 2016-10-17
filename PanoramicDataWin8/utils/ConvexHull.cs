using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;

namespace PanoramicDataWin8.utils
{
    // Finding a convex hull in the plane
    // This program requires .Net version 2.0.
    // Peter Sestoft (sestoft@itu.dk) * Java 2000-10-07, GC# 2001-10-27
    
    // ------------------------------------------------------------

    // Find the convex hull of a XPoint set in the plane

    // An implementation of Graham's (1972) XPoint elimination algorithm,
    // as modified by Andrew (1979) to find lower and upper hull separately.

    // This implementation correctly handles duplicate XPoints, and
    // multiple XPoints with the same x-coordinate.

    // 1. Sort the XPoints lexicographically by increasing (x,y), thus 
    //    finding also a leftmost XPoint L and a rightmost XPoint R.
    // 2. Partition the XPoint set into two lists, upper and lower, according as 
    //    XPoint is above or below the segment LR.  The upper list begins with 
    //    L and ends with R; the lower list begins with R and ends with L.
    // 3. Traverse the XPoint lists clockwise, eliminating all but the extreme
    //    XPoints (thus eliminating also duplicate XPoints).
    // 4. Eliminate L from lower and R from upper, if necessary.
    // 5. Join the XPoint lists (in clockwise order) in an array.

    class Convexhull
    {
        public static IList<Vec> convexhull(IList<Point> pointList)
        {

            var pts = new XPoint[pointList.Count];
            int k = 0;
            foreach (var point in pointList)
            {
                pts[k++] = new XPoint(point.X, point.Y);
            }

            // Sort XPoints lexicographically by increasing (x, y)
            int N = pts.Length;
            Polysort.Quicksort<XPoint>(pts);
            XPoint left = pts[0], right = pts[N - 1];
            // Partition into lower hull and upper hull
            CDLL<XPoint> lower = new CDLL<XPoint>(left), upper = new CDLL<XPoint>(left);
            for (int i = 0; i < N; i++)
            {
                double det = XPoint.Area2(left, right, pts[i]);
                if (det > 0)
                    upper = upper.Append(new CDLL<XPoint>(pts[i]));
                else if (det < 0)
                    lower = lower.Prepend(new CDLL<XPoint>(pts[i]));
            }
            lower = lower.Prepend(new CDLL<XPoint>(right));
            upper = upper.Append(new CDLL<XPoint>(right)).Next;
            // Eliminate XPoints not on the hull
            eliminate(lower);
            eliminate(upper);

            /* phil : Eliminating duplicates leaves holes in the hull
            // Eliminate duplicate endXPoints
            if (lower.Prev.val.Equals(upper.val))
                lower.Prev.Delete();
            if (upper.Prev.val.Equals(lower.val))
                upper.Prev.Delete();
             */
            // Join the lower and upper hull
            XPoint[] res = new XPoint[lower.Size() + upper.Size()];
            lower.CopyInto(res, 0);
            upper.CopyInto(res, lower.Size());

            var result = new List<Vec>();
            foreach (var r in res)
            {
                result.Add(new Vec(r.x, r.y));
            }
            return result;
        }

        // Graham's scan
        private static void eliminate(CDLL<XPoint> start)
        {
            CDLL<XPoint> v = start, w = start.Prev;
            bool fwd = false;
            while (v.Next != start || !fwd)
            {
                if (v.Next == w)
                    fwd = true;
                if (XPoint.Area2(v.val, v.Next.val, v.Next.Next.val) < 0) // right turn
                    v = v.Next;
                else
                {                                       // left turn or straight
                    v.Next.Delete();
                    v = v.Prev;
                }
            }
        }
    }


    // ------------------------------------------------------------

    // XPoints in the plane

    public class XPoint : Ordered<XPoint>
    {
        private static readonly Random rnd = new Random();

        public double x, y;

        public XPoint(double x, double y)
        {
            this.x = x; this.y = y;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        public static XPoint Random(int w, int h)
        {
            return new XPoint(rnd.Next(w), rnd.Next(h));
        }

        public bool Equals(XPoint p2)
        {
            return x == p2.x && y == p2.y;
        }

        public override bool Less(Ordered<XPoint> o2)
        {
            XPoint p2 = (XPoint)o2;
            return x < p2.x || x == p2.x && y < p2.y;
        }

        // Twice the signed area of the triangle (p0, p1, p2)
        public static double Area2(XPoint p0, XPoint p1, XPoint p2)
        {
            return p0.x * (p1.y - p2.y) + p1.x * (p2.y - p0.y) + p2.x * (p0.y - p1.y);
        }
    }

    // ------------------------------------------------------------

    // Circular doubly linked lists of T

    class CDLL<T>
    {
        private CDLL<T> prev, next;     // not null, except in deleted elements
        public T val;

        // A new CDLL node is a one-element circular list
        public CDLL(T val)
        {
            this.val = val; next = prev = this;
        }

        public CDLL<T> Prev
        {
            get { return prev; }
        }

        public CDLL<T> Next
        {
            get { return next; }
        }

        // Delete: adjust the remaining elements, make this one XPoint nowhere
        public void Delete()
        {
            next.prev = prev; prev.next = next;
            next = prev = null;
        }

        public CDLL<T> Prepend(CDLL<T> elt)
        {
            elt.next = this; elt.prev = prev; prev.next = elt; prev = elt;
            return elt;
        }

        public CDLL<T> Append(CDLL<T> elt)
        {
            elt.prev = this; elt.next = next; next.prev = elt; next = elt;
            return elt;
        }

        public int Size()
        {
            int count = 0;
            CDLL<T> node = this;
            do
            {
                count++;
                node = node.next;
            } while (node != this);
            return count;
        }

        public void PrintFwd()
        {
            CDLL<T> node = this;
            do
            {
                node = node.next;
            } while (node != this);
        }

        public void CopyInto(T[] vals, int i)
        {
            CDLL<T> node = this;
            do
            {
                vals[i++] = node.val;	// still, implicit checkcasts at runtime 
                node = node.next;
            } while (node != this);
        }
    }

    // ------------------------------------------------------------

    class Polysort
    {
        private static void swap<T>(T[] arr, int s, int t)
        {
            T tmp = arr[s]; arr[s] = arr[t]; arr[t] = tmp;
        }

        // Typed OO-style quicksort a la Hoare/Wirth

        private static void qsort<T>(Ordered<T>[] arr, int a, int b)
        {
            // sort arr[a..b]
            if (a < b)
            {
                int i = a, j = b;
                Ordered<T> x = arr[(i + j) / 2];
                do
                {
                    while (arr[i].Less(x)) i++;
                    while (x.Less(arr[j])) j--;
                    if (i <= j)
                    {
                        swap<Ordered<T>>(arr, i, j);
                        i++; j--;
                    }
                } while (i <= j);
                qsort<T>(arr, a, j);
                qsort<T>(arr, i, b);
            }
        }

        public static void Quicksort<T>(Ordered<T>[] arr)
        {
            qsort<T>(arr, 0, arr.Length - 1);
        }
    }

    public abstract class Ordered<T>
    {
        public abstract bool Less(Ordered<T> that);
    }
}
