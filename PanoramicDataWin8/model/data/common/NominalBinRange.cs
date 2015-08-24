using System;
using System.Collections.Generic;
using System.Linq;

namespace PanoramicDataWin8.model.data.common
{
    public class NominalBinRange : BinRange
    {
        private Dictionary<double, string> _labelsValue = new Dictionary<double, string>();

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
            labels = _labelsValue.OrderBy(kvp => kvp.Value).Select(kvp => new BinLabel()
            {
                Label = kvp.Value,
                MinValue = count++,
                MaxValue = count,
                Value = kvp.Key

            }).ToList();

            return labels;
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
            var index = _labelsValue.Keys.ToList().IndexOf((int)Math.Floor((value - this.MinValue) / this.Step));
            if (index == -1)
            {
                index = _labelsValue.Keys.ToList().IndexOf((int)Math.Floor(((value - 1) - this.MinValue) / this.Step)) + 1;
            }
            return index;
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
