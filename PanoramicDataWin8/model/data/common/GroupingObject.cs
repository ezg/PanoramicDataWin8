using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.data.common
{
    public class GroupingObject
    {
        public Dictionary<AttributeOperationModel, object> GroupingValues {get;set;}

        public GroupingObject()
        {
             GroupingValues = new Dictionary<AttributeOperationModel, object>();
        }

        public void Add(AttributeOperationModel aom, object value)
        {
            GroupingValues.Add(aom, value);
        }

        public override bool Equals(object obj)
        {
            if (obj is GroupingObject)
            {
                var go = obj as GroupingObject;
                if (GroupingValues.Count > 0)
                {
                    return go.GroupingValues.SequenceEqual(this.GroupingValues);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        public override int GetHashCode()
        {
            if (GroupingValues.Count > 0)
            {
                int code = 0;
                foreach (var v in GroupingValues.Values)
                {
                    if (v == null)
                    {
                        code ^= "null".GetHashCode();
                    }
                    else
                    {
                        code ^= v.GetHashCode();
                    }
                }
                return code;
            }
            else
            {
                return 0;   
            }
        }
    }
}
