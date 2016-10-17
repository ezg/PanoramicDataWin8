using System.Collections.Generic;
using Newtonsoft.Json;
using PanoramicDataWin8.controller.data;

namespace PanoramicDataWin8.model.data.idea
{
    [JsonObject(MemberSerialization.OptOut)]
    public class IDEASchemaModel : SchemaModel
    {
        private IDEAOriginModel _rootOriginModel;

        public IDEAOriginModel RootOriginModel
        {
            get { return _rootOriginModel; }
            set { SetProperty(ref _rootOriginModel, value); }
        }

        [JsonIgnore]
        public override QueryExecuter QueryExecuter { get; set; } = null;

        public override List<OriginModel> OriginModels
        {
            get
            {
                var originModels = new List<OriginModel>();
                originModels.Add(RootOriginModel);
                return originModels;
            }
        }
    }
}