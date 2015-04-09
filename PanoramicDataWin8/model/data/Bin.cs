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
            Counts = new Dictionary<GroupingObject, Dictionary<AttributeOperationModel, double>>();
            Values = new Dictionary<GroupingObject, Dictionary<AttributeOperationModel, double?>>();
            NormalizedValues = new Dictionary<GroupingObject, Dictionary<AttributeOperationModel, double?>>();
        }

        public bool ContainsBin(Bin bin)
        {
            return (bin.BinMinX >= this.BinMinX || aboutEqual(bin.BinMinX, this.BinMinX)) &&
                   (bin.BinMaxX <= this.BinMaxX || aboutEqual(bin.BinMaxX, this.BinMaxX)) &&
                   (bin.BinMinY >= this.BinMinY || aboutEqual(bin.BinMinY, this.BinMinY)) &&
                   (bin.BinMaxY <= this.BinMaxY || aboutEqual(bin.BinMaxY, this.BinMaxY));
        }

        public void Update(Bin bin)
        {
            BinMaxX = bin.BinMaxX;
            BinMaxY = bin.BinMaxY;
            BinMinX = bin.BinMinX;
            BinMinY = bin.BinMinY;
            Count = bin.Count;
            NormalizedCount = bin.NormalizedCount;
            NormalizedValues = bin.NormalizedValues;
            Values = bin.Values;
        }

        public void Map(Bin bin)
        {
            BinMaxX = bin.BinMaxX;
            BinMaxY = bin.BinMaxY;
            BinMinX = bin.BinMinX;
            BinMinY = bin.BinMinY;
            Count = bin.Count;
            NormalizedCount = bin.NormalizedCount;
            NormalizedValues = bin.NormalizedValues;
            Values = bin.Values;
        }

        public Bin Clone()
        {
            var bin = new Bin();
            bin.Update(this);
            return bin;
        }

        private bool aboutEqual(double x, double y)
        {
            double epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1E-15;
            return Math.Abs(x - y) <= epsilon;
        }

        public Dictionary<GroupingObject, Dictionary<AttributeOperationModel, double>> Counts { get; set; }
        public Dictionary<GroupingObject, Dictionary<AttributeOperationModel, double?>> Values { get; set; }
        public Dictionary<GroupingObject, Dictionary<AttributeOperationModel, double?>> NormalizedValues { get; set; }

        public double Count { get; set; }
        public double? NormalizedCount { get; set; }
        public double BinMinX { get; set; }
        public double BinMinY { get; set; }
        public double BinMaxX { get; set; }
        public double BinMaxY { get; set; }
        public List<DataRow> Samples { get; set; }
    }
}
