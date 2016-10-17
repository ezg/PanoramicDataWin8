using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.model.view.operation
{
    public class ExampleOperationViewModel : OperationViewModel
    {
        public ExampleOperationViewModel(ExampleOperationModel exampleOperationModel) : base(exampleOperationModel)
        {
        }

        public ExampleOperationModel ExampleOperationModel => (ExampleOperationModel) OperationModel;
    }
}