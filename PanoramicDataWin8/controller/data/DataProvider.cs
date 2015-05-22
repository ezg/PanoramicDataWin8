using System.Collections.Generic;
using System.Threading.Tasks;
using PanoramicDataWin8.model.data;

namespace PanoramicDataWin8.controller.data
{
    public abstract class DataProvider
    {
        public bool IsInitialized { get; set; }
        public int NrSamplesToCheck { get; set; }
        public QueryModel QueryModelClone { get; set; }
        public abstract Task StartSampling();
        public abstract Task<List<DataRow>> GetSampleDataRows(int sampleSize);
        public abstract double Progress();
        public abstract int GetNrTotalSamples();
    }
}
