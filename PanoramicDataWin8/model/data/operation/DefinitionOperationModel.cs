using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;

namespace PanoramicDataWin8.model.data.operation
{
    public class DefinitionOperationModel : OperationModel, IBrushableOperationModel
    {
        private readonly BrushableOperationModelImpl _brushableOperationModelImpl;

        public DefinitionOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
            _brushableOperationModelImpl = new BrushableOperationModelImpl(this);
        }
        public List<Color> BrushColors { get; set; } = new List<Color>();

        public ObservableCollection<IBrusherOperationModel> BrushOperationModels
        {
            get { return _brushableOperationModelImpl.BrushOperationModels; }
            set { _brushableOperationModelImpl.BrushOperationModels = value; }
        }
    }
}