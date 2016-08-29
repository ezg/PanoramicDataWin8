using System.Collections.Generic;

namespace PanoramicDataWin8.model.data.result
{
    public class VisualizationItemResultModel : ResultItemModel
    {
        public VisualizationItemResultModel()
        {
        }

        private Dictionary<InputOperationModel, ResultItemValueModel> _values = new Dictionary<InputOperationModel, ResultItemValueModel>();
        public Dictionary<InputOperationModel, ResultItemValueModel> Values
        {
            get
            {
                return _values;
            }
            set
            {
                this.SetProperty(ref _values, value);
            }
        }

        private Dictionary<InputOperationModel, double> _margins = new Dictionary<InputOperationModel, double>();
        public Dictionary<InputOperationModel, double> Margins
        {
            get
            {
                return _margins;
            }
            set
            {
                this.SetProperty(ref _margins, value);
            }
        }

        private Dictionary<InputOperationModel, double> _marginsAbsolute = new Dictionary<InputOperationModel, double>();
        public Dictionary<InputOperationModel, double> MarginsAbsolute
        {
            get
            {
                return _marginsAbsolute;
            }
            set
            {
                this.SetProperty(ref _marginsAbsolute, value);
            }
        }


        public void AddValue(InputOperationModel aom, ResultItemValueModel value)
        {
            if (!_values.ContainsKey(aom))
            {
                _values.Add(aom, value);
            }
            else
            {
                _values[aom] = value;
            }
        }

        public void AddMargin(InputOperationModel aom, double margin)
        {
            if (!_margins.ContainsKey(aom))
            {
                _margins.Add(aom, margin);
            }
            else
            {
                _margins[aom] = margin;
            }
        }

        public void AddMarginAbsolute(InputOperationModel aom, double marginAbsolute)
        {
            if (!_marginsAbsolute.ContainsKey(aom))
            {
                _marginsAbsolute.Add(aom, marginAbsolute);
            }
            else
            {
                _marginsAbsolute[aom] = marginAbsolute;
            }
        }


        private bool _isSelected = false;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                this.SetProperty(ref _isSelected, value);
            }
        }

        private int _count = 0;
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                this.SetProperty(ref _count, value);
            }
        }

        private int _brushCount = 0;
        public int BrushCount
        {
            get
            {
                return _brushCount;
            }
            set
            {
                this.SetProperty(ref _brushCount, value);
            }
        }
    }
}
