using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.data.common
{
    public class NominalBinRange : BinRange
    {
        private List<string> _labels = new List<string>();
        public List<string> Labels
        {
            get
            {
                return _labels;
            }
            set
            {
                _labels = value;
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
            scale.MaxValue = dataMaxValue + 1;
            scale.Step = 1;
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
            return new NominalBinRange(dataMin, dataMax, TargetBinNumber)
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

        public override string GetLabel(double value)
        {
            int index = GetIndex(value);
            return Labels[index];
        }
    }

}
