using System.Collections.Generic;
using Newtonsoft.Json;
using PanoramicDataWin8.controller.data;

namespace PanoramicDataWin8.model.data.tuppleware
{
    [JsonObject(MemberSerialization.OptOut)]
    public class TuppleWareSchemaModel : SchemaModel
    {
        private QueryExecuter _queryExecuter = null;

        public TuppleWareSchemaModel() 
        {
        }

        private TuppleWareOriginModel _rootOriginModel = null;
        public TuppleWareOriginModel RootOriginModel
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

        public override Dictionary<CalculatedAttributeModel, string> CalculatedAttributeModels
        {
            get
            {
                return new Dictionary<CalculatedAttributeModel, string>();
            }
        }

        public override Dictionary<NamedAttributeModel, string> NamedAttributeModels
        {
            get
            {
                return new Dictionary<NamedAttributeModel, string>();
            }
        }
    }
}
