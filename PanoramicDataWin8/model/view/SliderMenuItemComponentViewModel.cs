using System;

namespace PanoramicDataWin8.model.view
{
    public class SliderMenuItemComponentViewModel : MenuItemComponentViewModel
    {
        private double _finalValue;

        private string _label = "";

        private double _maxValue;

        private double _minValue;

        private double _value;

        private Func<double, string> _formatter;

        public double FinalValue
        {
            get { return _finalValue; }
            set { SetProperty(ref _finalValue, value); }
        }

        public double Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public Func<double, string> Formatter
        {
            get { return _formatter; }
            set { SetProperty(ref _formatter, value); }
        }

        public double MaxValue
        {
            get { return _maxValue; }
            set { SetProperty(ref _maxValue, value); }
        }

        public double MinValue
        {
            get { return _minValue; }
            set { SetProperty(ref _minValue, value); }
        }

        public string Label
        {
            get { return _label; }
            set { SetProperty(ref _label, value); }
        }
    }
}