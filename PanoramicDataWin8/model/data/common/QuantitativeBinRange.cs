using System;
using System.Collections.Generic;

namespace PanoramicDataWin8.model.data.common
{
    public class QuantitativeBinRange : BinRange
    {
        private bool _isIntegerRange = false;
        public bool IsIntegerRange
        {
            get
            {
                return _isIntegerRange;
            }
            set
            {
                _isIntegerRange = value;
            }
        }

        private double _step = 0;
        public double Step
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

        private QuantitativeBinRange(double dataMinValue, double dataMaxValue, double targetBinNumber, bool isIntegerRange)
        {
            DataMinValue = dataMinValue;
            DataMaxValue = dataMaxValue;
            TargetBinNumber = targetBinNumber;
            IsIntegerRange = isIntegerRange;
        }

        public static QuantitativeBinRange Initialize(double dataMinValue, double dataMaxValue, double targetBinNumber, bool isIntegerRange)
        {
            if (dataMinValue == 0 && dataMaxValue == 0)
            {
               //dataMaxValue = 1;
            }
            QuantitativeBinRange scale = new QuantitativeBinRange(dataMinValue, dataMaxValue, targetBinNumber, isIntegerRange);
            double[] extent = scale.getExtent(scale.DataMinValue, scale.DataMaxValue, scale.TargetBinNumber);
            scale.MinValue = extent[0];
            scale.MaxValue = extent[1];
            scale.Step = extent[2];
            return scale;
        }

        public override List<double> GetBins()
        {
            List<double> scale = new List<double>();
            for (double v = MinValue; v < MaxValue; v += Step)
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
                    newMin -= Step;
                }
            }
            if (dataMax >= MaxValue)
            {
                while (dataMax >= newMax)
                {
                    newMax += Step;
                }
            }

            int multiplier = (int)(GetBins().Count / TargetBinNumber);
            double newStep = Step;
            if (multiplier > 1)
            {
                //newStep = Step * (double)multiplier;
            }

            return new QuantitativeBinRange(dataMin, dataMax, TargetBinNumber, this.IsIntegerRange)
            {
                MinValue = newMin,
                MaxValue = newMax,
                DataMinValue = Math.Min(dataMin, this.DataMinValue),
                DataMaxValue = Math.Min(dataMax, this.DataMaxValue),
                Step = newStep
            };
        }

        public override int GetIndex(double value)
        {
            return (int)Math.Floor(Math.Round((value - this.MinValue) / this.Step, 14));
        }
        public override double AddStep(double value)
        {
            return value + Step;
        }

        private double[] getExtent(double dataMin, double dataMax, double m)
        {
            if (dataMin == dataMax)
            {
                //dataMin -= 0.1;
                dataMax += 0.1;
            }
            double span = dataMax - dataMin;

            double step = Math.Pow(10, Math.Floor(Math.Log10(span / m)));
            double err = m / span * step;

            if (err <= .15)
                step *= 10;
            else if (err <= .35)
                step *= 5;
            else if (err <= .75)
                step *= 2;

            if (IsIntegerRange)
            {
                step = Math.Ceiling(step);
            }
            double[] ret = new double[3];
            ret[0] = (double)(Math.Floor(Math.Round(dataMin, 14) / step) * step);
            ret[1] = (double)(Math.Floor(Math.Round(dataMax, 14) / step) * step + step);
            ret[2] = (double)step;

            return ret;
        }

        public override string GetLabel(double value)
        {
            return (Math.Floor(value / _step) * _step).ToString();
        }
    }
}
