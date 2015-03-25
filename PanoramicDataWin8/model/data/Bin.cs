using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.data
{
    public class Bin
    {
        public Bin()
        {
            IntervalMinX = Double.MaxValue;
            IntervalMinY = Double.MaxValue;
            IntervalMaxX = Double.MinValue;
            IntervalMaxY = Double.MinValue;
        }

        private bool intersects(double r1Left, double r1Right, double r1Top, double r1Bottom, double r2Left, double r2Right, double r2Top, double r2Bottom)
        {
            return !(r2Left > r1Right ||
                     r2Right < r1Left ||
                     r2Top > r1Bottom ||
                     r2Bottom < r1Top);
        }
        public bool ContainsBin(Bin bin)
        {
            return (bin.BinMinX >= this.BinMinX || aboutEqual(bin.BinMinX, this.BinMinX)) &&
                   (bin.BinMaxX <= this.BinMaxX || aboutEqual(bin.BinMaxX, this.BinMaxX)) &&
                   (bin.BinMinY >= this.BinMinY || aboutEqual(bin.BinMinY, this.BinMinY)) &&
                   (bin.BinMaxY <= this.BinMaxY || aboutEqual(bin.BinMaxY, this.BinMaxY));
        }

        private bool aboutEqual(double x, double y)
        {
            double epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1E-15;
            return Math.Abs(x - y) <= epsilon;
        }


        public bool BinIntersects(double x, double y)
        {
            return x >= this.BinMinX && x < this.BinMaxX && y >= this.BinMinY && y < this.BinMaxY;
        }

        public bool BinIntersects(Bin b)
        {
            return intersects(this.BinMinX, this.BinMaxX, this.BinMinY, this.BinMaxY, b.BinMinX, b.BinMaxX, b.BinMinY, b.BinMaxY);
        }

        public bool IntervalIntersects(Bin b)
        {
            return intersects(this.IntervalMinX, this.IntervalMaxX, this.IntervalMinY, this.IntervalMaxY, b.IntervalMinX, b.IntervalMaxX, b.IntervalMinY, b.IntervalMaxY);
        }

        public double BinXOverlapRatio(double left, double right)
        {
            if (left < this.BinMaxX && right >= this.BinMinX)
            {
                double newLeft = Math.Max(left, this.BinMinX);
                double newRight = Math.Min(right, this.BinMaxX);
                if (right - left == 0)
                {
                    return 1.0;
                }
                else
                {
                    return (newRight - newLeft) / (right - left);
                }
            }
            return 0;
        }

        public double BinYOverlapRatio(double top, double bottom)
        {
            if (top < this.BinMaxY && bottom >= this.BinMinY)
            {
                double newTop = Math.Max(top, this.BinMinY);
                double newBottom = Math.Min(bottom, this.BinMaxY);
                if (bottom - top == 0)
                {
                    return 1.0;
                }
                else
                {
                    return (newBottom - newTop) / (bottom - top);
                }
            }
            return 0;
        }

        public double Size { get; set; }
        public double Count { get; set; }
        public double NormalizedCount { get; set; }
        public double BinMinX { get; set; }
        public double BinMinY { get; set; }
        public double BinMaxX { get; set; }
        public double BinMaxY { get; set; }
        public bool HasInterval { get; set; }
        public double IntervalMinX { get; set; }
        public double IntervalMinY { get; set; }
        public double IntervalMaxX { get; set; }
        public double IntervalMaxY { get; set; }

        public string LabelX { get; set; }
        public string LabelY { get; set; }
    }
}
