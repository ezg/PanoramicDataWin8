namespace PanoramicDataWin8.model.view.tilemenu
{
    public class InputGroupViewTileMenuContentViewModel : TileMenuContentViewModel
    {
        private AttributeGroupViewModel _inputGroupViewModel;

        public AttributeGroupViewModel InputGroupViewModel
        {
            get { return _inputGroupViewModel; }
            set { SetProperty(ref _inputGroupViewModel, value); }
        }
    }
}