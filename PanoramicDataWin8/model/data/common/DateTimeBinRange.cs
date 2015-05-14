using System;
using System.Collections.Generic;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.common
{
    public class DateTimeBinRange : BinRange
    {
        private DateTimeStep _step = null;
        public DateTimeStep Step
        {
            get
            {
                return _step;
            }
            set
            {
                _step = value;
            }
        }

        public DateTimeBinRange(double dataMinValue, double dataMaxValue, double targetBinNumber)
        {
            DataMinValue = dataMinValue;
            DataMaxValue = dataMaxValue;
            TargetBinNumber = targetBinNumber;

        }

        public static DateTimeBinRange Initialize(double dataMinValue, double dataMaxValue, double targetBinNumber)
        {
            DateTimeBinRange scale = new DateTimeBinRange(dataMinValue, dataMaxValue, targetBinNumber);
            DateTimeStep dateTimeStep = null;
            long[] extent = DateTimeUtil.GetDataTimeExtent(dataMinValue, dataMaxValue, targetBinNumber, out dateTimeStep);

            scale.MinValue = (double)extent[0];
            scale.MaxValue = (double)extent[1];
            scale.Step = dateTimeStep;
            return scale;
        }

        public override List<double> GetBins()
        {
            List<double> scale = new List<double>();
            for (double v = MinValue; v < MaxValue; v = DateTimeUtil.AddToDateTime(v, Step).Ticks)
            {
                scale.Add(v);
            }
            return scale;
        }

        public override BinRange GetUpdatedBinRange(double dataMin, double dataMax)
        {
            double newMin = MinValue;
            double newMax = MaxValue;

            if (dataMin < MinValue)
            {
                while (dataMin < newMin)
                {
                    DateTimeUtil.RemoveFromDateTime(newMin, Step);
                }
            }
            if (dataMax >= MaxValue)
            {
                while (dataMax >= newMax)
                {
                    DateTimeUtil.AddToDateTime(newMax, Step);
                }
            }

            return new DateTimeBinRange(dataMin, dataMax, TargetBinNumber)
            {
                MinValue = newMin,
                MaxValue = newMax,
                DataMinValue = Math.Min(dataMin, this.DataMinValue),
                DataMaxValue = Math.Min(dataMax, this.DataMaxValue),
                Step = this.Step
            };
        }

        public override int GetIndex(double value)
        {
            List<double> scale = GetBins();

            int i = 0;
            foreach (double min in scale)
            {
                double max = this.AddStep(min);

                if (value >= min && value < max)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        public override double AddStep(double value)
        {
            return DateTimeUtil.AddToDateTime(value, Step).Ticks;
        }

        public override string GetLabel(double value)
        {
            return DateTimeUtil.GetLabel(value, Step);
        }
    }
}
