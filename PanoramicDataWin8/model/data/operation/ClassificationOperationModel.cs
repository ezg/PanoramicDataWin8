namespace PanoramicDataWin8.model.data.operation
{
    public class ClassificationOperationModel : AttributeUsageOperationModel
    {
        private double _minimumSupport = 0.1;

        private TaskModel taskModel;

        public ClassificationOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
        }

        public TaskModel TaskModel
        {
            get { return taskModel; }
            set { SetProperty(ref taskModel, value); }
        }

        public double MinimumSupport
        {
            get { return _minimumSupport; }
            set { SetProperty(ref _minimumSupport, value); }
        }
    }
}