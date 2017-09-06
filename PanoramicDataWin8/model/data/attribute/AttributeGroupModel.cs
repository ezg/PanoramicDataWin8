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

        public AttributeGroupModel()
        {
        }

        public List<AttributeModel> InputModels
        {
            get { return _inputModels; }
            set { SetProperty(ref _inputModels, value); }
        }

        string _rawName, _displayName;
        
        public void SetName(string name)
        {
            RawName = DisplayName = name;
        }
        public override string RawName
        {
            get { return _rawName; }
            set { SetProperty(ref _rawName, value); }
        }
        public override string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }
        public override DataType DataType { get; set; }
        public override AttributeFuncModel FuncModel { get; set; }
        public override List<VisualizationHint> VisualizationHints { get; set; }
    }
}