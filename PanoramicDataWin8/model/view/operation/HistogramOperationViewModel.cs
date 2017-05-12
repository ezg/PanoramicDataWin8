using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.model.view.operation
{
    public class HistogramOperationViewModel : OperationViewModel
    {
        private RecommenderOperationViewModel _recommenderOperationViewModel;

        public HistogramOperationViewModel(HistogramOperationModel histogramOperationModel) : base(histogramOperationModel)
        {
        }

        public HistogramOperationModel HistogramOperationModel => (HistogramOperationModel) OperationModel;

        public RecommenderOperationViewModel RecommenderOperationViewModel
        {
            get { return _recommenderOperationViewModel; }
            set { SetProperty(ref _recommenderOperationViewModel, value); }
        }
    }
}