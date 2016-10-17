using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.data.idea
{
    public class IDEAAttributeGroupModel : AttributeGroupModel
    {
        public IDEAAttributeGroupModel()
        {

        }

        public IDEAAttributeGroupModel(string rawName, string displayName)
        {
            _rawName = rawName;
            _displayName = displayName;
        }

        private string _rawName = "";
        public override string RawName
        {
            get
            {
                return _rawName;
            }
            set
            {
                this.SetProperty(ref _rawName, value);
            }
        }

        private string _displayName = "";
        public override string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                this.SetProperty(ref _displayName, value);
            }
        }

        public override int Index { get; set; }
    }
}
