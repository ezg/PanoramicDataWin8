using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Interpolation;

namespace PanoramicDataWin8.model.data.common
{
    public class NominalBinRange : BinRange
    {
        List<string> days = new List<string>() {"Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"};
        List<string> daysUpper = new List<string>() { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" };
        List<string> months = new List<string>() { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        private Dictionary<double, string> _labelsValue = new Dictionary<double, string>();

        public Dictionary<double, string> LabelsValue
        {
            get { return _labelsValue; }
            set { _labelsValue = value; }
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

        public NominalBinRange()
        {
            
        }

        public NominalBinRange(double dataMinValue, double dataMaxValue, double targetBinNumber)
        {
            DataMinValue = dataMinValue;
            DataMaxValue = dataMaxValue;
            TargetBinNumber = targetBinNumber;

        }

        public static NominalBinRange Initialize(double dataMinValue, double dataMaxValue, double targetBinNumber)
        {
            NominalBinRange scale = new NominalBinRange(dataMinValue, dataMaxValue, targetBinNumber);
            scale.MinValue = dataMinValue;
            scale.MaxValue = dataMaxValue;
            scale.Step = 1;
            return scale;
        }

        public override List<double> GetBins()
        {
            List<double> scale = new List<double>();
            for (double v = MinValue; v <= MaxValue; v += Step)
            {
                scale.Add(v);
            }
            //scale = _labelsValue.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
            
            return scale;
        }

        public override List<BinLabel> GetLabels()
        {
            List<BinLabel> labels = new List<BinLabel>();
            int count = 0;
            labels = _labelsValue.OrderBy(kvp => this.getOrderingIndex(kvp.Value)).Select(kvp => new BinLabel()
            {
                Label = kvp.Value,
                MinValue = count++,
                MaxValue = count,
                Value = kvp.Key

            }).ToList();

            return labels;
        }

        private string getOrderingIndex(string v)
        {
            //v = new String(v.Select((ch, index) => (index == 0) ? ch : Char.ToLower(ch)).ToArray());

            if (days.Contains(v))
            {
                return days.IndexOf(v).ToString("D8");
            }
            if (daysUpper.Contains(v))
            {
                return daysUpper.IndexOf(v).ToString("D8");
            }
            if (months.Contains(v))
            {
                return months.IndexOf(v).ToString("D8");
            }
            return v;
        }

        public override BinRange GetUpdatedBinRange(double dataMin, double dataMax)
        {
            return new NominalBinRange(dataMin, dataMax, TargetBinNumber)
            {
                MinValue = Math.Min(dataMin, this.MinValue),
                MaxValue = Math.Max(dataMax, this.MaxValue),
                DataMinValue = Math.Min(dataMin, this.DataMinValue),
                DataMaxValue = Math.Min(dataMax, this.DataMaxValue),
                Step = 1
            };
        }

        public override int GetIndex(double value)
        {
            return (int)Math.Floor((value - this.MinValue) / this.Step);
        }

        public override int GetDisplayIndex(double value)
        {
            var ordered = _labelsValue.OrderBy(kvp => this.getOrderingIndex(kvp.Value)).Select(kvp => kvp.Key).ToList().IndexOf(value);
            var index = _labelsValue.Keys.ToList().IndexOf((int)Math.Floor((value - this.MinValue) / this.Step));
            if (ordered == -1)
            {
                index = _labelsValue.Keys.ToList().IndexOf((int)Math.Floor(((value - 1) - this.MinValue) / this.Step)) + 1;
                ordered =(int) this.MaxValue + 1;
            }
            return ordered;
        }

        public override double AddStep(double value)
        {
            return value + Step;
        }

        public void SetLabels(Dictionary<string, double> labels)
        {
            _labelsValue = labels.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        }

        public override string GetLabel(double value)
        {
            return _labelsValue[value];
        }
    }

}
