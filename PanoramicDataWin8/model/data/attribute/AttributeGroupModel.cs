using System;
using System.Collections.Generic;
using System.Linq;
using IDEA_common.catalog;
using PanoramicDataWin8.model.data.idea;

namespace PanoramicDataWin8.model.data.attribute
{
    public class AttributeGroupModel : AttributeModel
    {
        private List<AttributeModel> _inputModels = new List<AttributeModel>();
        
        public AttributeGroupModel(string rawName, string displayName):
            base(rawName, displayName, null, DataType.Undefined, null, null)
        {
        }

        public List<AttributeModel> InputModels
        {
            get { return _inputModels; }
            set { SetProperty(ref _inputModels, value); }
        }
    }
}