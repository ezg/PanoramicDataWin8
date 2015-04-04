using PanoramicDataWin8.model.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.controller.data.sim
{
    public class DataBinStructure
    {
        public DataBinStructure()
        {
            Bins = new List<List<Bin>>();
        }

        public double XNullCount { get; set; }
        public double YNullCount { get; set; }
        public double XAndYNullCount { get; set; } 
        public List<List<Bin>> Bins { get; set; }
        public BinRange XBinRange { get; set; }
        public BinRange YBinRange { get; set; }
    }
}
