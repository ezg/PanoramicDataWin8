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

    public class TaskGroupViewTileMenuContentViewModel : TileMenuContentViewModel
    {
        private TaskGroupModel _taskGroupModel = null;
        public TaskGroupModel TaskGroupModel
        {
            get
            {
                return _taskGroupModel;
            }
            set
            {
                this.SetProperty(ref _taskGroupModel, value);
            }
        }
    }


    public class TaskViewTileMenuContentViewModel : TileMenuContentViewModel
    {
        private TaskModel _taskModel = null;
        public TaskModel TaskModel
        {
            get
            {
                return _taskModel;
            }
            set
            {
                this.SetProperty(ref _taskModel, value);
            }
        }
    }



    public class VisualizationTypeViewTileMenuContentViewModel : TileMenuContentViewModel
    {
        private VisualizationTypeViewModel _visualizationTypeViewModel = null;
        public VisualizationTypeViewModel VisualizationTypeViewModel
        {
            get
            {
                return _visualizationTypeViewModel;
            }
            set
            {
                this.SetProperty(ref _visualizationTypeViewModel, value);
            }
        }
    }
}
