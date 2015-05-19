using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;

namespace PanoramicDataWin8.model.data
{
    [JsonObject(MemberSerialization.OptOut)]
    public abstract class OriginModel : BindableBase
    {
        private SchemaModel _schemaModel = null;
        [JsonIgnore]
        public SchemaModel SchemaModel
        {
            get
            {
                return _schemaModel;
            }
            set
            {
                this.SetProperty(ref _schemaModel, value);
            }
        }

        public abstract string Name
        {
            get;
        }

        [JsonIgnore]
        public abstract List<InputModel> InputModels
        {
            get;
        }

        public abstract List<OriginModel> OriginModels
        {
            get;
        }

        public override bool Equals(object obj)
        {
            if (obj is OriginModel)
            {
                var am = obj as OriginModel;
                return
                    am.Name.Equals(this.Name);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int code = 0;
            code ^= this.Name.GetHashCode();
            return code;
        }
    }
}
