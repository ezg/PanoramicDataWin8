using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PanoramicDataWin8.controller.data;
namespace PanoramicDataWin8.model.data.progressive
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ProgressiveSchemaModel : SchemaModel
    {
        private QueryExecuter _queryExecuter = null;

        public ProgressiveSchemaModel()
        {
        }

        private ProgressiveOriginModel _rootOriginModel = null;
        public ProgressiveOriginModel RootOriginModel
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
