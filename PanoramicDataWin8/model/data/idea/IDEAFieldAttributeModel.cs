using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.data.idea
{
    [JsonObject(MemberSerialization.OptOut)]
    public class IDEAFieldAttributeModel : AttributeFieldModel
    {
        private string _displayName = "";

        private int _index = -1;

        private string _rawName = "";

        public IDEAFieldAttributeModel(string rawName, string displayName, int index, string inputDataType, string inputVisualizationType)
        {
            _rawName = rawName;
            _displayName = displayName;
            InputDataType = inputDataType;
            InputVisualizationType = inputVisualizationType;
            _index = index;
        }

        public override string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
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