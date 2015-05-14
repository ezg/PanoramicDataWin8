using System.Collections.Generic;
using System.Linq;

namespace PanoramicDataWin8.model.data.common
{
    public class BinStructure
    {
        public BinStructure()
        {
            Bins = new Dictionary<BinIndex, Bin>();
        }

        public Dictionary<AttributeOperationModel, double> AggregatedMaxValues = new Dictionary<AttributeOperationModel, double>();
        public Dictionary<AttributeOperationModel, double> AggregatedMinValues = new Dictionary<AttributeOperationModel, double>();

        public double NullCount { get; set; } 
        public Dictionary<BinIndex, Bin> Bins { get; set; }
        public List<BinRange> BinRanges { get; set; }

        public void Map(BinStructure binStructure)
        {
            this.NullCount += binStructure.NullCount;
            foreach (var oldBinIndex in binStructure.Bins.Keys)
            {
                Bin oldBin = binStructure.Bins[oldBinIndex];
                BinIndex newBinIndex = new BinIndex();

                for(int d = 0; d< this.BinRanges.Count; d++)
                {
                    newBinIndex.Indices.Add(this.BinRanges[d].GetIndex(oldBin.Spans[d].Min));
                }

                Bin newBin = this.Bins[newBinIndex];

                if (newBin.ContainsBin(oldBin))
                {
                    newBin.Map(oldBin);
                }
            }
        }
    }

    public class BinIndex
    {
        public List<int> Indices { get; set; }

        public BinIndex()
        {
            Indices = new List<int>();
        }

        public BinIndex(params int[] indices)
        {
            Indices = new List<int>(indices);
        }

        public override bool Equals(object obj)
        {
            if (obj is BinIndex)
            {
                var go = obj as BinIndex;
                if (Indices.Count > 0)
                {
                    return go.Indices.SequenceEqual(this.Indices);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        public override int GetHashCode()
        {
            if (Indices.Count > 0)
            {
                int code = 0;
                foreach (var v in Indices)
                {
                    code ^= v.GetHashCode();
                }
                return code;
            }
            else
            {
                return 0;
            }
        }
    }

}
