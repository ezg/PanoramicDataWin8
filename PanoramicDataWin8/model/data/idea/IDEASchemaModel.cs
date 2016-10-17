using System.Collections.Generic;
using Newtonsoft.Json;
using PanoramicDataWin8.controller.data;

namespace PanoramicDataWin8.model.data.idea
{
    [JsonObject(MemberSerialization.OptOut)]
    public class IDEASchemaModel : SchemaModel
    {
        private QueryExecuter _queryExecuter = null;

        public IDEASchemaModel()
        {
        }

        private IDEAOriginModel _rootOriginModel = null;
        public IDEAOriginModel RootOriginModel
        {
            get
            {
                return _rootOriginModel;
            }
            set
            {
                this.SetProperty(ref _rootOriginModel, value);
            }
        }

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
    }
}
