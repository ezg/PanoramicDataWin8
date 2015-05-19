using System;

namespace PanoramicDataWin8.model.data
{
    public class NamedInputModel : InputFieldModel
    {
        public NamedInputModel() : base() { }

        public override string Name
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override string InputVisualizationType
        {
            get { throw new NotImplementedException(); }
        }

        public override string InputDataType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
