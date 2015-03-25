using PanoramicDataWin8.model.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.controller.data.sim
{
    public class BinStructure
    {
        public BinStructure()
        {
            Bins = new List<List<Bin>>();
            BinMinX = Double.MaxValue;
            BinMinY = Double.MaxValue;
            BinMaxX = Double.MinValue;
            BinMaxY = Double.MinValue;

            DataMinX = Double.MaxValue;
            DataMinY = Double.MaxValue;
            DataMaxX = Double.MinValue;
            DataMaxY = Double.MinValue;
        }

        public double BinMinX { get; set; }
        public double BinMinY { get; set; }
        public double BinMaxX { get; set; }
        public double BinMaxY { get; set; }

        public double DataMinX { get; set; }
        public double DataMinY { get; set; }
        public double DataMaxX { get; set; }
        public double DataMaxY { get; set; }

        public double BinSizeX { get; set; }
        public double BinSizeY { get; set; }

        public List<List<Bin>> Bins { get; set; }
    }
}
