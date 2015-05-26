using System.Collections.Generic;
using PanoramicDataWin8.model.data;

namespace PanoramicDataWin8.controller.data
{
    public class DataRow
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