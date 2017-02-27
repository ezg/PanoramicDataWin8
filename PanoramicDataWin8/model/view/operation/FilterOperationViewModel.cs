using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.model.view.operation
{
    public class FilterOperationViewModel : OperationViewModel
    {
        public FilterOperationViewModel(FilterOperationModel filterOperationModel) : base(filterOperationModel)
        {
        }

        public FilterOperationModel FilterOperationModel => (FilterOperationModel)OperationModel;
    }
}