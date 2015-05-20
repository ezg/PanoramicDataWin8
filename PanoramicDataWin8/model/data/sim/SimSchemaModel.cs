using System.Collections.Generic;
using Newtonsoft.Json;
using PanoramicDataWin8.controller.data;
using PanoramicDataWin8.controller.data.virt;

namespace PanoramicDataWin8.model.data.sim
{
    [JsonObject(MemberSerialization.OptOut)]
    public class SimSchemaModel : SchemaModel
    {
        private QueryExecuter _queryExecuter = null;

        public SimSchemaModel() 
        {
        }


        public SimOriginModel RootOriginModel { get; set; }

        [JsonIgnore]
        public override QueryExecuter QueryExecuter
        {
            get
            {
                return _queryExecuter;
            }
            set
            {
                _queryExecuter = value;
            }
        }

        public override List<OriginModel> OriginModels
        {
            get
            {
                List<OriginModel> originModels = new List<OriginModel>();
                originModels.Add(RootOriginModel);
                return originModels;
            }
        }

        public override Dictionary<CalculatedInputModel, string> CalculatedInputFieldModels
        {
            get
            {
                return new Dictionary<CalculatedInputModel, string>();
            }
        }

        public override Dictionary<NamedInputModel, string> NamedInputFieldModels
        {
            get
            {
                return new Dictionary<NamedInputModel, string>();
            }
        }
    }
}
