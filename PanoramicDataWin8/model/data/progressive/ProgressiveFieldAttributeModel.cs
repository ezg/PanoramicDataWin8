using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PanoramicDataWin8.model.data.progressive
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ProgressiveFieldAttributeModel : AttributeFieldModel
    {
        public ProgressiveFieldAttributeModel(string rawName, string displayName, int index, string inputDataType, string inputVisualizationType)
        {
            _rawName = rawName;
            _displayName = displayName;
            _inputDataType = inputDataType;
            _inputVisualizationType = inputVisualizationType;
            _index = index;
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

        private int _index = -1;
        public override int Index
        {
            get
            {
                return _index;
            }
            set
            {
                this.SetProperty(ref _index, value);
            }
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

        private string _inputVisualizationType = "";
        public override string InputVisualizationType
        {
            get
            {
                return _inputVisualizationType;
            }
        }

        private string _inputDataType = "";
        public override string InputDataType
        {
            get
            {
                return _inputDataType;
            }
        }
    }
}
