using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.controller.data.sim
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

        public abstract double AddStep(double value);

        public virtual string GetLabel(double value)
        {
            return value.ToString();
        }
    }
}
