namespace PanoramicDataWin8.model.data.attribute
{
    public abstract class AttributeFieldModel : AttributeModel
    {
        public abstract string InputVisualizationType { get; }

        public abstract string InputDataType { get; }

        public override bool Equals(object obj)
        {
            if (obj is AttributeFieldModel)
            {
                var am = obj as AttributeFieldModel;
                return
                    am.OriginModel.Equals(OriginModel) &&
                    am.RawName.Equals(RawName) &&
                    am.InputVisualizationType.Equals(InputVisualizationType) &&
                    am.InputDataType.Equals(InputDataType);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var code = 0;
            code ^= OriginModel.GetHashCode();
            code ^= RawName.GetHashCode();
            code ^= InputVisualizationType.GetHashCode();
            code ^= InputDataType.GetHashCode();
            return code;
        }
    }
}