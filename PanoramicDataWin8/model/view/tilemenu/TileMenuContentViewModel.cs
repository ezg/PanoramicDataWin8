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

    public class InputFieldViewTileMenuContentViewModel : TileMenuContentViewModel
    {
        private AttributeTransformationViewModel _attributeTransformationViewModel = null;
        public AttributeTransformationViewModel AttributeTransformationViewModel
        {
            get
            {
                return _attributeTransformationViewModel;
            }
            set
            {
                this.SetProperty(ref _attributeTransformationViewModel, value);
            }
        }
    }

    public class InputGroupViewTileMenuContentViewModel : TileMenuContentViewModel
    {
        private InputGroupViewModel _inputGroupViewModel = null;
        public InputGroupViewModel InputGroupViewModel
        {
            get
            {
                return _inputGroupViewModel;
            }
            set
            {
                this.SetProperty(ref _inputGroupViewModel, value);
            }
        }
    }

    public class OperationTypeGroupTileMenuContentViewModel : TileMenuContentViewModel
    {
        private OperationTypeGroupModel _operationTypeGroupModel = null;
        public OperationTypeGroupModel OperationTypeGroupModel
        {
            get
            {
                return _operationTypeGroupModel;
            }
            set
            {
                this.SetProperty(ref _operationTypeGroupModel, value);
            }
        }
    }


    public class OperationTypeTileMenuContentViewModel : TileMenuContentViewModel
    {
        private OperationTypeModel _operationTypeModel = null;
        public OperationTypeModel OperationTypeModel
        {
            get
            {
                return _operationTypeModel;
            }
            set
            {
                this.SetProperty(ref _operationTypeModel, value);
            }
        }
    }
}
