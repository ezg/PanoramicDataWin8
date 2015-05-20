using System.Collections.Generic;
using PanoramicDataWin8.model.data;

namespace PanoramicDataWin8.controller.data
{
    public class DataRow
    {
        private  Dictionary<InputFieldModel, object> _entries = null;
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

        private Dictionary<InputFieldModel, double?> _visualizationValues = new Dictionary<InputFieldModel, double?>();
        public Dictionary<InputFieldModel, double?> VisualizationValues
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