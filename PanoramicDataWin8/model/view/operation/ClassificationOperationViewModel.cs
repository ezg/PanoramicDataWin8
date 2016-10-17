using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.model.view.operation
{
    public class ClassificationOperationViewModel : OperationViewModel
    {
        public ClassificationOperationViewModel(ClassificationOperationModel classificationOperationModel) :base(classificationOperationModel)
        {
        }

        public ClassificationOperationModel ClassificationOperationModel => (ClassificationOperationModel) OperationModel;
    }
}
