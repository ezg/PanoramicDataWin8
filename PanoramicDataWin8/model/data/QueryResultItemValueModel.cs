using PanoramicData.model.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.data
{
    public class QueryResultItemValueModel
    {
        public object Value { get; set; }
        public string StringValue { get; set; }
        public string ShortStringValue { get; set; }
        
        public QueryResultItemValueModel()
        {
        }

        public override int GetHashCode()
        {
            int code = Value.GetHashCode();
            return code;
        }
        public override bool Equals(object obj)
        {
            if (obj is QueryResultItemValueModel)
            {
                var pv = obj as QueryResultItemValueModel;
                if (pv.Value.Equals(Value))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
