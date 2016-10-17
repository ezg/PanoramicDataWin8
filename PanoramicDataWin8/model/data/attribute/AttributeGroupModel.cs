using System.Collections.Generic;
using System.Linq;

namespace PanoramicDataWin8.model.data.attribute
{
    public abstract class AttributeGroupModel : AttributeModel
    {
        private List<AttributeModel> _inputModels = new List<AttributeModel>();

        public List<AttributeModel> InputModels
        {
            get { return _inputModels; }
            set { SetProperty(ref _inputModels, value); }
        }

        public override bool Equals(object obj)
        {
            if (obj is AttributeGroupModel)
            {
                var am = obj as AttributeGroupModel;
                return
                    am.OriginModel.Equals(OriginModel) &&
                    am.RawName.Equals(RawName) &&
                    InputModels.SequenceEqual(am.InputModels);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var code = 0;
            code ^= OriginModel.GetHashCode();
            code ^= RawName.GetHashCode();
            return InputModels.Aggregate(code, (current, inputModel) => current ^ inputModel.GetHashCode());
        }
    }
}