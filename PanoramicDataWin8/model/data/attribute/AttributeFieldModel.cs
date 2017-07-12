﻿using IDEA_common.catalog;

namespace PanoramicDataWin8.model.data.attribute
{
    public abstract class AttributeFieldModel : AttributeModel
    {
        public abstract string InputVisualizationType { get; }

        public override bool Equals(object obj)
        {
            if (obj is AttributeFieldModel)
            {
                var am = obj as AttributeFieldModel;
                return
                    OriginModel != null && 
                    am.OriginModel.Equals(OriginModel) &&
                    am.RawName.Equals(RawName) &&
                    am.InputVisualizationType.Equals(InputVisualizationType) &&
                    am.DataType.Equals(DataType);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var code = 0;
            if (OriginModel != null)
                code ^= OriginModel.GetHashCode();
            code ^= RawName.GetHashCode();
            code ^= InputVisualizationType.GetHashCode();
            code ^= DataType.GetHashCode();
            return code;
        }
    }
}