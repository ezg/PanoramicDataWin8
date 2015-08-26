using System.Collections.Generic;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;

namespace PanoramicDataWin8.controller.data
{
    public class DataRow : ResultItemModel
    {
        private Dictionary<InputFieldModel, object> _entries = null;
        public Dictionary<InputFieldModel, object> Entries
        {
            get
            {
                return _entries;
            }
            set
            {
                _entries = value;
            }
        }

        private Dictionary<InputOperationModel, double?> _visualizationValues = new Dictionary<InputOperationModel, double?>();
        public Dictionary<InputOperationModel, double?> VisualizationValues
        {
            get
            {
                return _visualizationValues;
            }
            set
            {
                _visualizationValues = value;
            }
        }
    }
}