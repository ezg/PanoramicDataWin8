using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.data
{
    [JsonObject(MemberSerialization.OptOut)]
    public abstract class OriginModel : BindableBase
    {
        private SchemaModel _schemaModel;

        [JsonIgnore]
        public SchemaModel SchemaModel
        {
            get { return _schemaModel; }
            set { SetProperty(ref _schemaModel, value); }
        }

        public abstract string Name { get; }

        [JsonIgnore]
        public abstract List<AttributeModel> InputModels { get; }

        public abstract List<OriginModel> OriginModels { get; }

        public override bool Equals(object obj)
        {
            if (obj is OriginModel)
            {
                var am = obj as OriginModel;
                return
                    am.Name.Equals(Name);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var code = 0;
            code ^= Name.GetHashCode();
            return code;
        }
    }
}