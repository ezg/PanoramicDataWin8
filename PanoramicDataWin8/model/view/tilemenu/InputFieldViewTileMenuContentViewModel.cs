namespace PanoramicDataWin8.model.view.tilemenu
{
    public class InputFieldViewTileMenuContentViewModel : TileMenuContentViewModel
    {
        private AttributeViewModel _attributeViewModel;

        public AttributeViewModel AttributeViewModel
        {
            get { return _attributeViewModel; }
            set { SetProperty(ref _attributeViewModel, value); }
        }
    }
}