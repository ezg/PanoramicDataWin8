using PanoramicDataWin8.model.data;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view.tilemenu
{
    public class TileMenuContentViewModel : ExtendedBindableBase
    {
        private string _name = "";
        public string Name
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

    public class AttributeViewTileMenuContentViewModel : TileMenuContentViewModel
    {
        private AttributeViewModel _attributeViewModel = null;
        public AttributeViewModel AttributeViewModel
        {
            get
            {
                return _attributeViewModel;
            }
            set
            {
                this.SetProperty(ref _attributeViewModel, value);
            }
        }
    }
}
