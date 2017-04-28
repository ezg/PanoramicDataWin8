using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.model.view.operation
{
    public class RecommenderOperationViewModel : OperationViewModel
    {
        public RecommenderOperationViewModel(RecommenderOperationModel recommenderOperationModel) : base(recommenderOperationModel)
        {
        }

        public RecommenderOperationModel RecommenderOperationModel => (RecommenderOperationModel) OperationModel;
    }
}