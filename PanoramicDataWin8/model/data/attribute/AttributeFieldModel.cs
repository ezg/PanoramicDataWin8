namespace PanoramicDataWin8.model.data.attribute
{
    public abstract class AttributeFieldModel : AttributeModel
    {
        public abstract string InputVisualizationType
        {
            get;
        }

        public abstract string InputDataType
        {
            get;
        }

        public override bool Equals(object obj)
        {
            if (obj is AttributeFieldModel)
            {
                var am = obj as AttributeFieldModel;
                return
                    am.OriginModel.Equals(this.OriginModel) &&
                    am.RawName.Equals(this.RawName) &&
                    am.InputVisualizationType.Equals(this.InputVisualizationType) &&
                    am.InputDataType.Equals(this.InputDataType);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int code = 0;
            code ^= this.OriginModel.GetHashCode();
            code ^= this.RawName.GetHashCode();
            code ^= this.InputVisualizationType.GetHashCode();
            code ^= this.InputDataType.GetHashCode();
            return code;
        }
    }
}
