using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.data.progressive
{
    public class ProgressiveAttributeGroupModel : AttributeGroupModel
    {
        public ProgressiveAttributeGroupModel()
        {

        }

        public ProgressiveAttributeGroupModel(string rawName, string displayName)
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
