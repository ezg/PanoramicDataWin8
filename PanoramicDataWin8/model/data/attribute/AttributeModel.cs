using System.Collections.Generic;
using IDEA_common.catalog;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.attribute
{
    public abstract class AttributeModel : ExtendedBindableBase
    {
        private bool _isDisplayed = true;
        private OriginModel _originModel;

        public OriginModel OriginModel
        {
            get { return _originModel; }
            set { SetProperty(ref _originModel, value); }
        }

        public bool IsDisplayed
        {
            get { return _isDisplayed; }
            set { SetProperty(ref _isDisplayed, value); }
        }

        public abstract string RawName { get; set; }
        public abstract string DisplayName { get; set; }

        public abstract List<VisualizationHint> VisualizationHints { get; set; }

        public abstract int Index { get; set; }
    }
}