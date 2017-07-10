using System.Collections.Generic;
using IDEA_common.catalog;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.data.idea
{
    [JsonObject(MemberSerialization.OptOut)]
    public class IDEAAttributeFieldModel : AttributeFieldModel
    {
        private string _displayName = "";

        private int _index = -1;

        private string _rawName = "";

        private List<VisualizationHint> _visualizationHints = new List<VisualizationHint>();

        public IDEAAttributeFieldModel(string rawName, string displayName, int index, string inputDataType, string inputVisualizationType, List<VisualizationHint> visualizationHints)
        {
            _rawName = rawName;
            _displayName = displayName;
            InputDataType = inputDataType;
            InputVisualizationType = inputVisualizationType;
            _index = index;
            _visualizationHints = visualizationHints;
        }

        public override string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }


        public override List<VisualizationHint> VisualizationHints
        {
            get { return _visualizationHints; }
            set { SetProperty(ref _visualizationHints, value); }
        }

        public override int Index
        {
            get { return _index; }
            set { SetProperty(ref _index, value); }
        }

        public override string RawName
        {
            get { return _rawName; }
            set { SetProperty(ref _rawName, value); }
        }

        public override string InputVisualizationType { get; } = "";

        public override string InputDataType { get; } = "";
    }
}