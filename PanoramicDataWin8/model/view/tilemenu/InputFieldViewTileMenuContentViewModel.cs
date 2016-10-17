namespace PanoramicDataWin8.model.view.tilemenu
{
    public class InputFieldViewTileMenuContentViewModel : TileMenuContentViewModel
    {
        private AttributeTransformationViewModel _attributeTransformationViewModel;

        public AttributeTransformationViewModel AttributeTransformationViewModel
        {
            get { return _attributeTransformationViewModel; }
            set { SetProperty(ref _attributeTransformationViewModel, value); }
        }
    }
}