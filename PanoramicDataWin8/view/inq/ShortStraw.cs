using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;

namespace PanoramicDataWin8.view.inq
{
    public class ShortStraw
    {
        // Determines the interspacing pixel distance between resampled points.
        public static double DetermineResampleSpacing(InkStroke stroq)
        {
            Rect bound = stroq.BoundingRect;
            return (Distance(new Point(bound.Left, bound.Top), new Point(bound.Right, bound.Bottom)) / 80.0);
        }

        public static double Distance(Point a, Point b)
        {
            double deltaX = a.X - b.X;
            double deltaY = a.Y - b.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        public static InkStroke ResamplePoints(InkStroke stroq, double S)
        {
            double D = 0.0, d; // D is the distance accumulator of consecutive points, when D < S
            InkStroke resampled = new InkStroke();
            resampled.Points.Add(stroq.Points[0]);

            int i, c = 0;

            for (i = 1; i < stroq.Points.Count; i++)
            {
                d = Distance(stroq.Points[i - 1], stroq.Points[i]);

                if (D + d >= S)
                {
                    c = c + 1;
                    Point q = new Point();
                    q.X = stroq.Points[i - 1].X + ((S - D) / d) * (stroq.Points[i].X - stroq.Points[i - 1].X);
                    q.Y = stroq.Points[i - 1].Y + ((S - D) / d) * (stroq.Points[i].Y - stroq.Points[i - 1].Y);
                    resampled.Points.Add(q);
                    stroq.Points.Insert(i, q);
                    D = 0.0;
                }
                else
                    D += d;
            }
            return resampled;
        }

        // Calculate the path length in the inkStroke from one point a to another point b.
        public static double PathDistance(InkStroke stroq, int a, int b)
        {
            double d = 0.0;
            for (int i = a; i < b; i++)
            {
                d += Distance(stroq.Points[i], stroq.Points[i + 1]);
            }

            return d;
        }

        public static List<int> GetCorners(InkStroke stroq, InkStroke oInkStroke)
        {
            List<int> corners = new List<int>(); // List of index of corners.
            List<double> straws = new List<double>(); // List of length of straws at each inkStroke points.
            List<double> temp = new List<double>();
            double t, localMin;
            int W = 3, i, localMinIndex;

            corners.Add(0);
            for (i = 0; i < W; i++) { straws.Add(0.0); }
            for (i = W; i < stroq.Points.Count - W; i++)
            {
                straws.Add(Distance(stroq.Points[i - W], stroq.Points[i + W]));
                temp.Add(straws[i]);
            }
            for (i = stroq.Points.Count - W; i < stroq.Points.Count; i++) { straws.Add(0.0); }

            // Find the median and threshold.
            if (temp.Count == 0)
            {
                Debug.WriteLine("Too few points. Try to draw more points.");
                return corners;
            }
            temp.Sort();
            t = temp[temp.Count / 2] * 0.95;

            // Find local minimum of potential corners.
            for (i = W; i < stroq.Points.Count - W; i++)
            {
                if (straws[i] < t)
                {
                    localMin = 100000.0;
                    localMinIndex = i;
                    while (i < stroq.Points.Count - W && straws[i] < t)
                    {
                        if (straws[i] < localMin)
                        {
                            localMin = straws[i];
                            localMinIndex = i;
                        }
                        i++;
                    }
                    corners.Add(localMinIndex);
                }
            }
            corners.Add(stroq.Points.Count - 1);

            // Do post process to the potential corners.
            corners = PostProcessCorners(stroq, corners, straws);

            // Remove corners on curve.
            corners = CurveCheck(stroq, corners);

            // Find original corners.
            corners = ResampleToOriginal(stroq, oInkStroke, corners);

            if (corners.Count != 0)
            {
                InkStroke cinkStroke = new InkStroke();
                cinkStroke.Points.Add(stroq.Points[corners[0]]);
                for (i = 1; i < corners.Count; i++)
                {
                    cinkStroke.Points.Add(oInkStroke.Points[corners[i]]);
                }

            }
            else
                Debug.WriteLine("No corners found.");

            return corners;
        }

        // Checks the corner candidates to see if any corners can be removed or added based on higher-level polyline rules.
        public static List<int> PostProcessCorners(InkStroke points, List<int> corners, List<double> straws)
        {
            bool conti;
            int i, j, c1, c2;

            // Check corner pairs, decide if there will be additional corners between them.
            do
            {
                conti = true;
                for (i = 1; i < corners.Count; i++)
                {
                    c1 = corners[i - 1];
                    c2 = corners[i];
                    if (!IsLine(points, c1, c2))
                    {
                        int newCorner = HalfwayCorner(straws, c1, c2);
                        corners.Insert(i, newCorner);
                        conti = false;
                    }
                }
            } while (!conti);

            // Check if there are three consecutive corners colinear,
            // if is, remove the middle corner.
            for (i = 1; i < corners.Count - 1; i++)
            {
                c1 = corners[i - 1];
                c2 = corners[i + 1];
                if (IsLine(points, c1, c2))
                {
                    corners.RemoveAt(i);
                    i--;
                }
            }

            // Check if there are too close corners within a range of index
            // Choose the one with shorter straw as our cornrer.          
            for (i = 0; i < corners.Count - 3; i++)
            {
                for (j = 1; j <= 3; j++)
                {
                    if (corners[i] + j == corners[i + 1])
                    {
                        if (straws[i] < straws[i + 1])
                            corners.RemoveAt(i + 1);
                        else
                            corners.RemoveAt(i);
                        break;
                    }
                }
            }

            return corners;
        }

        public static int HalfwayCorner(List<double> straws, int a, int b)
        {
            int quarter = (b - a) / 2, i, minIndex = 0;
            double minValue = 100000.0;

            for (i = a + quarter; i <= b - quarter; i++)
            {
                if (straws[i] < minValue)
                {
                    minValue = straws[i];
                    minIndex = i;
                }
            }

            return minIndex;
        }

        public static bool IsLine(InkStroke stroq, int a, int b)
        {
            double threshold = 0.93;
            double distance = Distance(stroq.Points[a], stroq.Points[b]);
            double pathDistance = PathDistance(stroq, a, b);

            if (distance / pathDistance < threshold)
                return false;
            else
                return true;
        }

        // Find the corner correspondence in original point set(before resample).
        public static List<int> ResampleToOriginal(InkStroke resampled, InkStroke stroq, List<int> corners)
        {
            int i, j, minIndex = 0;
            double minValue, distance;
            List<int> oCorners = new List<int>();
            for (i = 0; i < corners.Count; i++)
            {
                minValue = 100000.0;
                for (j = 0; j < stroq.Points.Count; j++)
                {
                    distance = Distance(resampled.Points[corners[i]], stroq.Points[j]);
                    if (distance < minValue)
                    {
                        minValue = distance;
                        minIndex = j;
                    }
                }
                oCorners.Add(minIndex);
            }

            //Debug.WriteLine( Math.Acos(0.5));
            return oCorners;
        }

        // Delete false positive corners on curves or arcs.
        public static List<int> CurveCheck(InkStroke stroq, List<int> corners)
        {
            int shiftValue = 15, i, diff1, diff2, defaultValue = 30;
            double alpha = 0.0, beta = 0.0, ta;
            Point A, B, C, D, E; // ACB  = alpha angle, DCE = beta angle.

            for (i = 1; i < corners.Count - 1; i++)
            {
                diff1 = corners[i] - corners[i - 1];
                diff2 = corners[i + 1] - corners[i];
                shiftValue = (diff1 < defaultValue || diff2 < defaultValue) ? Math.Min(diff1, diff2) : defaultValue;

                C = stroq.Points[corners[i]];
                A = stroq.Points[corners[i] - shiftValue];
                B = stroq.Points[corners[i] + shiftValue];
                D = stroq.Points[corners[i] - shiftValue / 3];
                E = stroq.Points[corners[i] + shiftValue / 3];

                alpha = AngleFrom3Points(A, C, B) * 180.0 / Math.PI;
                beta = AngleFrom3Points(D, C, E) * 180.0 / Math.PI;

                ta = 10 + 800 / (alpha + 35); // This a an empirical fomula,

                //Debug.WriteLine("shift = " + shiftValue + " alpha = " + alpha + " beta = " + beta
                //                    + " beta - alpha = " + (beta - alpha) + " ta = " + ta);

                if (beta - alpha > ta) // C is a corner on a curve
                {
                    corners.RemoveAt(i);
                    i--;
                }
            }
            return corners;
        }

        // Return the angle of ABC by Law of Cosines
        public static double AngleFrom3Points(Point A, Point B, Point C)
        {
            double AB, AC, BC;
            AB = Distance(A, B);
            AC = Distance(A, C);
            BC = Distance(B, C);

            return Math.Acos((AB * AB + BC * BC - AC * AC) / (2 * AB * BC));
        }

        public static List<int> IStraw(InkStroke stroq)
        {
            double S = DetermineResampleSpacing(stroq);
            InkStroke resampled = ResamplePoints(stroq, S);
            List<int> corners = GetCorners(resampled, stroq);
            return corners;
        }
    }
}
