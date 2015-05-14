using PanoramicData.model.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.data.result
{
    public class ResultItemValueModel
    {
        public object Value { get; set; }
        public object NoramlizedValue { get; set; }
        public string StringValue { get; set; }
        public string ShortStringValue { get; set; }
        
        public ResultItemValueModel()
        {
        }

        public ResultItemValueModel(object value, object normalizedValue)
        {
            this.Value = value;
            this.NoramlizedValue = normalizedValue;
            if (value != null)
            {
                this.StringValue = this.ShortStringValue = value.ToString();
            }
        }

        public override int GetHashCode()
        {
            int code = Value.GetHashCode();
            return code;
        }
        public override bool Equals(object obj)
        {
            if (obj is ResultItemValueModel)
            {
                var pv = obj as ResultItemValueModel;
                if (pv.Value.Equals(Value))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
