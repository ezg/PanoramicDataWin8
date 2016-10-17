using System.Collections.ObjectModel;

namespace PanoramicDataWin8.model.data.operation
{
    public interface IFilterConsumerOperationModel : IOperationModel
    {
        FilteringOperation FilteringOperation { get; set; }
        ObservableCollection<FilterLinkModel> LinkModels { get; }
    }
}