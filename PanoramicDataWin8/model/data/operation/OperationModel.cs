using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDEA_common.operations;
using Newtonsoft.Json;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data
{
    [JsonObject(MemberSerialization.OptOut)]
    public class OperationModel : ExtendedBindableBase
    {

        private static long _nextId = 0;
        public OperationModel(SchemaModel schemaModel)
        {
            _schemaModel = schemaModel;

            _id = _nextId++;
        }

        public OperationModel()
        {
            
        }

        private IResult _result = null;
        [JsonIgnore]
        public IResult Result
        {
            get
            {
                return _result;
            }
            set
            {
                this.SetProperty(ref _result, value);
            }
        }


        private SchemaModel _schemaModel = null;
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

        private long _id = 0;
        public long Id
        {
            get
            {
                return _id;
            }
            set
            {
                this.SetProperty(ref _id, value);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is OperationModel)
            {
                var am = obj as OperationModel;
                return
                    am.Id.Equals(this.Id);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int code = 0;
            code ^= this.Id.GetHashCode();
            return code;
        }

    }
}
