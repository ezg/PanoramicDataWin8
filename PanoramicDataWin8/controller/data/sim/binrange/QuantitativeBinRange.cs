using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.controller.data.sim
{
    public class QuantitativeBinRange : BinRange
    {
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

        public QuantitativeBinRange(double dataMinValue, double dataMaxValue, double targetBinNumber)
        {
            DataMinValue = dataMinValue;
            DataMaxValue = dataMaxValue;
            TargetBinNumber = targetBinNumber;

        }

        public static QuantitativeBinRange Initialize(double dataMinValue, double dataMaxValue, double targetBinNumber)
        {
            QuantitativeBinRange scale = new QuantitativeBinRange(dataMinValue, dataMaxValue, targetBinNumber);
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
                newStep = Step * (double)multiplier;
            }

            return new QuantitativeBinRange(dataMin, dataMax, TargetBinNumber)
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
            return (int)Math.Floor((value - this.MinValue) / this.Step);
        }
        public override double AddStep(double value)
        {
            return value + Step;
        }

        private double[] getExtent(double dataMin, double dataMax, double m)
        {
            double span = dataMax - dataMin;

            double step = Math.Pow(10, Math.Floor(Math.Log10(span / m)));
            double err = m / span * step;

            if (err <= .15)
                step *= 10;
            else if (err <= .35)
                step *= 5;
            else if (err <= .75)
                step *= 2;

            double[] ret = new double[3];
            ret[0] = (double)(Math.Floor(dataMin / step) * step);
            ret[1] = (double)(Math.Floor(dataMax / step) * step + step);
            ret[2] = (double)step;

            return ret;
        }

        public override string GetLabel(double value)
        {
            return (Math.Floor(value / _step) * _step).ToString();
        }
    }
}
