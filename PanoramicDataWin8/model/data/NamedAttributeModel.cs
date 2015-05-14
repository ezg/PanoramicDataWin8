using System;

namespace PanoramicDataWin8.model.data
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
