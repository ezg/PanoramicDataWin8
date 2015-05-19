using System.Collections.Generic;
using System.Linq;

namespace PanoramicDataWin8.model.data
{
    public abstract class InputGroupModel : InputModel
    {
        private List<InputModel> _inputModels = new List<InputModel>();
        public List<InputModel> InputModels
        {
            get { return _inputModels; }
            set { this.SetProperty(ref _inputModels, value); }
        }
        public override bool Equals(object obj)
        {
            if (obj is InputGroupModel)
            {
                var am = obj as InputGroupModel;
                return
                    am.OriginModel.Equals(this.OriginModel) &&
                    am.Name.Equals(this.Name) &&
                    this.InputModels.SequenceEqual(am.InputModels);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int code = 0;
            code ^= this.OriginModel.GetHashCode();
            code ^= this.Name.GetHashCode();
            return this.InputModels.Aggregate(code, (current, inputModel) => current ^ inputModel.GetHashCode());
        }
    }
}