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

        private int _rowNumber = -1;
        public int RowNumber
        {
            get
            {
                return _rowNumber;
            }
            set
            {
                this.SetProperty(ref _rowNumber, value);
            }
        }
    }
}
