using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.data.sim
{
    [JsonObject(MemberSerialization.OptOut)]
    public class SimAttributeModel : AttributeModel
    {
        public SimAttributeModel(string name, string attributeDataType, string attributeVisualizationType)
        {
            _name = name;
            _attributeDataType = attributeDataType;
            _attributeVisualizationType = attributeVisualizationType;
        }

        private string _name = "";
        public override string Name
        {
            get
            {
                return _name;
            }
        }

        private string _attributeVisualizationType = "";
        public override string AttributeVisualizationType
        {
            get
            {
                return _attributeVisualizationType;
            }
        }

        private string _attributeDataType = "";
        public override string AttributeDataType
        {
            get
            {
                return _attributeDataType;
            }
        }
    }
}
