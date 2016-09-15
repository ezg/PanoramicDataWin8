using System.Collections.Generic;
using System.Linq;

namespace PanoramicDataWin8.model.data
{
    public abstract class AttributeGroupModel : AttributeModel
    {
        private List<AttributeModel> _inputModels = new List<AttributeModel>();
        public List<AttributeModel> InputModels
        {
            get { return _inputModels; }
            set { this.SetProperty(ref _inputModels, value); }
        }
        public override bool Equals(object obj)
        {
            if (obj is AttributeGroupModel)
            {
                var am = obj as AttributeGroupModel;
                return
                    am.OriginModel.Equals(this.OriginModel) &&
                    am.RawName.Equals(this.RawName) &&
                    this.InputModels.SequenceEqual(am.InputModels);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int code = 0;
            code ^= this.OriginModel.GetHashCode();
            code ^= this.RawName.GetHashCode();
            return this.InputModels.Aggregate(code, (current, inputModel) => current ^ inputModel.GetHashCode());
        }
    }
}