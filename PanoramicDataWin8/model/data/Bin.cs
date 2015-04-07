using PanoramicData.model.data;
using PanoramicDataWin8.controller.data.sim;
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
            Samples = new List<DataRow>();
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

        public double Count { get; set; }
        public double Value { get; set; }
        public double NormalizedValue { get; set; }
        public double BinMinX { get; set; }
        public double BinMinY { get; set; }
        public double BinMaxX { get; set; }
        public double BinMaxY { get; set; }
        public string LabelX { get; set; }
        public string LabelY { get; set; }

        public List<DataRow> Samples { get; set; }
    }
}
