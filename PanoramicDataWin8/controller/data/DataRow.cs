using System.Collections.Generic;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.result;

namespace PanoramicDataWin8.controller.data
{
    public class DataRow : ResultItemModel
    {
        private Dictionary<AttributeFieldModel, object> _entries = null;
        public Dictionary<AttributeFieldModel, object> Entries
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

        private Dictionary<AttributeTransformationModel, double?> _visualizationValues = new Dictionary<AttributeTransformationModel, double?>();
        public Dictionary<AttributeTransformationModel, double?> VisualizationValues
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