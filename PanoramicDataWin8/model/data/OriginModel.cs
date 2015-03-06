using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.data
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
        public abstract List<AttributeModel> AttributeModels
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
