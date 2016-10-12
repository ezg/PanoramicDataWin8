using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Media;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
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
