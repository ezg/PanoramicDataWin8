namespace PanoramicDataWin8.model.view.tilemenu
{
    public class InputGroupViewTileMenuContentViewModel : TileMenuContentViewModel
    {
        private InputGroupViewModel _inputGroupViewModel;

        public InputGroupViewModel InputGroupViewModel
        {
            get { return _inputGroupViewModel; }
            set { SetProperty(ref _inputGroupViewModel, value); }
        }
    }
}