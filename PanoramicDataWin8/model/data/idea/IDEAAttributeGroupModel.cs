using System;
using System.Collections.Generic;
using IDEA_common.catalog;
using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.data.idea
{
    public class IDEAAttributeGroupModel : AttributeGroupModel
    {
        private string _displayName = "";

        private string _rawName = "";

        public IDEAAttributeGroupModel()
        {
        }

        public IDEAAttributeGroupModel(string rawName, string displayName)
        {
            _rawName = rawName;
            _displayName = displayName;
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

        public override List<VisualizationHint> VisualizationHints { get; set; }
    }
}