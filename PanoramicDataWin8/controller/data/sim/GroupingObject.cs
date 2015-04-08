using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.controller.data.sim
{
    public class GroupingObject
    {
        private Dictionary<int, object> _dictionary = new Dictionary<int, object>();

        public GroupingObject()
        {
        }

        public void Add(int index, object value)
        {
            _dictionary.Add(index, value);
        }

        public override bool Equals(object obj)
        {
            if (obj is GroupingObject)
            {
                var go = obj as GroupingObject;
                if (_dictionary.Count > 0)
                {
                    return go._dictionary.SequenceEqual(this._dictionary);
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
            if (_dictionary.Count > 0)
            {
                int code = 0;
                foreach (var v in _dictionary.Values)
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
