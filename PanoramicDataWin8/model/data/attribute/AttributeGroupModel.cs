using System.Collections.Generic;
using System.Linq;

namespace PanoramicDataWin8.model.data.attribute
{
    public class AttributeGroupModel : utils.ExtendedBindableBase
    {
        private List<AttributeModel> _inputModels = new List<AttributeModel>();
        private OriginModel _originModel;

        public AttributeGroupModel(string name)
        {
            DisplayName = name;
        }

        public List<AttributeModel> InputModels
        {
            get { return _inputModels; }
            set { SetProperty(ref _inputModels, value); }
        }

        public string DisplayName { get; set; }

        public OriginModel OriginModel
        {
            get { return _originModel; }
            set { SetProperty(ref _originModel, value); }
        }

        public override bool Equals(object obj)
        {
            if (obj is AttributeGroupModel)
            {
                var am = obj as AttributeGroupModel;
                return
                    am.OriginModel.Equals(OriginModel) &&
                    am.DisplayName.Equals(DisplayName) &&
                    InputModels.SequenceEqual(am.InputModels);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var code = 0;
            code ^= OriginModel.GetHashCode();
            code ^= DisplayName.GetHashCode();
            return InputModels.Aggregate(code, (current, inputModel) => current ^ inputModel.GetHashCode());
        }
    }
}