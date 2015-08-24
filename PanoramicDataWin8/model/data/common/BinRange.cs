using System.Collections.Generic;

namespace PanoramicDataWin8.model.data.common
{
    public abstract class BinRange
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

        public abstract List<double> GetBins();

        public abstract BinRange GetUpdatedBinRange(double dataMin, double dataMax);

        public abstract int GetIndex(double value);

        public virtual int GetDisplayIndex(double value)
        {
            return GetIndex(value);
        }

        public abstract double AddStep(double value);

        public virtual string GetLabel(double value)
        {
            return value.ToString();
        }

        public virtual List<BinLabel> GetLabels()
        {
            List<BinLabel> labels = new List<BinLabel>();
            foreach (var bin in GetBins())
            {
                labels.Add(new BinLabel()
                {
                    Value = bin,
                    MinValue = bin,
                    MaxValue = AddStep(bin),
                    Label = GetLabel(bin)
                });
            }

            return labels;
        }
    }

    public class BinLabel
    {
        public double Value { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public string Label { get; set; }
    }
}
