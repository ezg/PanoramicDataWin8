using PanoramicDataWin8.model.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.controller.data.sim
{
    public class BinStructure
    {
        public BinStructure()
        {
            Bins = new List<List<Bin>>();
        }

        public double XNullCount { get; set; }
        public double YNullCount { get; set; }
        public double XAndYNullCount { get; set; } 
        public List<List<Bin>> Bins { get; set; }
        public Scale XScale { get; set; }
        public Scale YScale { get; set; }
    }
}
