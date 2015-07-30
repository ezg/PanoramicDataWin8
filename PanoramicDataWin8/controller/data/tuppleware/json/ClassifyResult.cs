using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.controller.data.tuppleware.json
{
    public class ClassifyResult
    {
        public double fp { get; set; }
        public double f1 { get; set; }
        public double auc { get; set; }
        public double recall { get; set; }
        public double precision { get; set; }
        public double tp { get; set; }
        public double tn { get; set; }
        public List<double> tpr { get; set; }
        public List<double> fpr { get; set; }
        public double fn { get; set; }
    }
}
