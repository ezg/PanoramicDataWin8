using System.Collections.Generic;

namespace PanoramicDataWin8.model.data.common
{
    public class AggregateBinRange : BinRange
    {
        public static AggregateBinRange Initialize()
        {
            AggregateBinRange scale = new AggregateBinRange();
            return scale;
        }

        public override List<double> GetBins()
        {
            List<double> scale = new List<double>();
            scale.Add(0);
            return scale;
        }

        public override BinRange GetUpdatedBinRange(double dataMin, double dataMax)
        {
            return new AggregateBinRange();
        }

        public override int GetIndex(double value)
        {
            return 0;
        }

        public override double AddStep(double value)
        {
            return value + 1;
        }
    }
}
