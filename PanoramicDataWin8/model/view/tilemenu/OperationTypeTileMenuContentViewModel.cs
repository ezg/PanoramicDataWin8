using PanoramicDataWin8.model.data;

namespace PanoramicDataWin8.model.view.tilemenu
{
    public class OperationTypeTileMenuContentViewModel : TileMenuContentViewModel
    {
        private OperationTypeModel _operationTypeModel;

        public OperationTypeModel OperationTypeModel
        {
            get { return _operationTypeModel; }
            set { SetProperty(ref _operationTypeModel, value); }
        }
    }
}