using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.data.progressive
{
    public class ProgressiveInputGroupModel : InputGroupModel
    {
        public ProgressiveInputGroupModel()
        {

        }

        public ProgressiveInputGroupModel(string name)
        {
            _name = name;
        }

        private string _name = "";
        public override string Name
        {
            get
            {
                return _name;
            }
            set
            {
                this.SetProperty(ref _name, value);
            }
        }
    }
}
