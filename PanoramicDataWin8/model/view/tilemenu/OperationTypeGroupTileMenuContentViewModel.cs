using PanoramicDataWin8.model.data;

namespace PanoramicDataWin8.model.view.tilemenu
{
    public class OperationTypeGroupTileMenuContentViewModel : TileMenuContentViewModel
    {
        private OperationTypeGroupModel _operationTypeGroupModel;

        public OperationTypeGroupModel OperationTypeGroupModel
        {
            get { return _operationTypeGroupModel; }
            set { SetProperty(ref _operationTypeGroupModel, value); }
        }
    }
}