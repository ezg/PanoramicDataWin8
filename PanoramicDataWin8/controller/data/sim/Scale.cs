using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.controller.data.sim
{
    public abstract class Scale
    {
        private double _minValue = 0;
        public double MinValue
        {
            get
            {
                return _minValue;
            }
            set
            {
                _minValue = value;
            }
        }

        private double _maxValue = 0;
        public double MaxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
                _maxValue = value;
            }
        }

        private double _dataMinValue = 0;
        public double DataMinValue
        {
            get
            {
                return _dataMinValue;
            }
            set
            {
                _dataMinValue = value;
            }
        }

        private double _dataMaxValue = 0;
        public double DataMaxValue
        {
            get
            {
                return _dataMaxValue;
            }
            set
            {
                _dataMaxValue = value;
            }
        }

        private double _targetBinNumber = 0;
        public double TargetBinNumber
        {
            get
            {
                return _targetBinNumber;
            }
            set
            {
                _targetBinNumber = value;
            }
        }

        public abstract List<double> GetScale();

        public abstract Scale GetUpdatedScale(double dataMin, double dataMax);

        public abstract int GetIndex(double value);

        public abstract double AddStep(double value);

        public virtual string GetLabel(double value)
        {
            return value.ToString();
        }
    }

    public class NominalScale : Scale
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

        public NominalScale(double dataMinValue, double dataMaxValue, double targetBinNumber)
        {
            DataMinValue = dataMinValue;
            DataMaxValue = dataMaxValue;
            TargetBinNumber = targetBinNumber;

        }

        public static NominalScale Initialize(double dataMinValue, double dataMaxValue, double targetBinNumber)
        {
            NominalScale scale = new NominalScale(dataMinValue, dataMaxValue, targetBinNumber);
            scale.MinValue = dataMinValue;
            scale.MaxValue = dataMaxValue + 1;
            scale.Step = 1;
            return scale;
        }

        public override List<double> GetScale()
        {
            List<double> scale = new List<double>();
            for (double v = MinValue; v < MaxValue; v += Step)
            {
                scale.Add(v);
            }
            return scale;
        }
        public override Scale GetUpdatedScale(double dataMin, double dataMax)
        {
            return new NominalScale(dataMin, dataMax, TargetBinNumber)
            {
                MinValue = Math.Min(dataMin, this.MinValue),
                MaxValue = Math.Max(dataMax + 1, this.MaxValue),
                DataMinValue = Math.Min(dataMin, this.DataMinValue),
                DataMaxValue = Math.Min(dataMax, this.DataMaxValue),
                Step = 1
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
    }

    public class DateTimeScale : Scale
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

        public DateTimeScale(double dataMinValue, double dataMaxValue, double targetBinNumber)
        {
            DataMinValue = dataMinValue;
            DataMaxValue = dataMaxValue;
            TargetBinNumber = targetBinNumber;

        }

        public static DateTimeScale Initialize(double dataMinValue, double dataMaxValue, double targetBinNumber)
        {
            DateTimeScale scale = new DateTimeScale(dataMinValue, dataMaxValue, targetBinNumber);
            DateTimeStep dateTimeStep = null;
            long[] extent = DateTimeUtil.GetDataTimeExtent(dataMinValue, dataMaxValue, targetBinNumber, out dateTimeStep);

            scale.MinValue = (double)extent[0];
            scale.MaxValue = (double)extent[1];
            scale.Step = dateTimeStep;
            return scale;
        }

        public override List<double> GetScale()
        {
            List<double> scale = new List<double>();
            for (double v = MinValue; v < MaxValue; v = DateTimeUtil.AddToDateTime(v, Step).Ticks)
            {
                scale.Add(v);
            }
            return scale;
        }

        public override Scale GetUpdatedScale(double dataMin, double dataMax)
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
            
            return new DateTimeScale(dataMin, dataMax, TargetBinNumber)
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
            List<double> scale = GetScale();

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

    public class QuantitativeScale : Scale
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

        public QuantitativeScale(double dataMinValue, double dataMaxValue, double targetBinNumber)
        {
            DataMinValue = dataMinValue;
            DataMaxValue = dataMaxValue;
            TargetBinNumber = targetBinNumber;

        }

        public static QuantitativeScale Initialize(double dataMinValue, double dataMaxValue, double targetBinNumber)
        {
            QuantitativeScale scale = new QuantitativeScale(dataMinValue, dataMaxValue, targetBinNumber);
            double[] extent = scale.getExtent(scale.DataMinValue, scale.DataMaxValue, scale.TargetBinNumber);
            scale.MinValue = extent[0];
            scale.MaxValue = extent[1];
            scale.Step = extent[2];
            return scale;
        }

        public override List<double> GetScale()
        {
            List<double> scale = new List<double>();
            for (double v = MinValue; v < MaxValue; v += Step)
            {
                scale.Add(v);
            }
            return scale;
        }
        public override Scale GetUpdatedScale(double dataMin, double dataMax)
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

            int multiplier = (int)(GetScale().Count / TargetBinNumber);
            double newStep = Step;
            if (multiplier > 1)
            {
                newStep = Step * (double)multiplier;
            }

            return new QuantitativeScale(dataMin, dataMax, TargetBinNumber)
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
    }
}
