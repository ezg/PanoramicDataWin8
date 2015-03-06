using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.data
{
    public class NamedAttributeModel : AttributeModel
    {
        public NamedAttributeModel() : base() { }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public override string AttributeVisualizationType
        {
            get { throw new NotImplementedException(); }
        }

        public override string AttributeDataType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
