using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class IncludeExludeMenuItemViewModel : MenuItemComponentViewModel
    {
        private  bool _isInclude = false;
        private AttributeModel _attributeModel = null;

        public bool IsInclude
        {
            get { return _isInclude; }
            set { SetProperty(ref _isInclude, value); }
        }

        public AttributeModel AttributeModel
        {
            get { return _attributeModel; }
            set { SetProperty(ref _attributeModel, value); }
        }
    }
}