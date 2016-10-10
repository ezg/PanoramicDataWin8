namespace PanoramicDataWin8.model.data.operation
{
    public class ClassificationOperationModel : OperationModel
    {
        private double _minimumSupport = 0.1;

        private OperationTypeModel _operationTypeModel;

        public ClassificationOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
        }

        public OperationTypeModel OperationTypeModel
        {
            get { return _operationTypeModel; }
            set { SetProperty(ref _operationTypeModel, value); }
        }

        public double MinimumSupport
        {
            get { return _minimumSupport; }
            set { SetProperty(ref _minimumSupport, value); }
        }
    }
}