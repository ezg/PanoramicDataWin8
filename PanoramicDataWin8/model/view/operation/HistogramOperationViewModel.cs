using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.model.view.operation
{
    public class HistogramOperationViewModel : OperationViewModel
    {
        public HistogramOperationViewModel(HistogramOperationModel histogramOperationModel) :base(histogramOperationModel)
        {
        }

        public HistogramOperationModel HistogramOperationModel => (HistogramOperationModel) OperationModel;
    }

    public class ExampleOperationViewModel : OperationViewModel
    {
        public ExampleOperationViewModel(ExampleOperationModel exampleOperationModel) : base(exampleOperationModel)
        {
        }

        public ExampleOperationModel ExampleOperationModel => (ExampleOperationModel)OperationModel;
    }
}
