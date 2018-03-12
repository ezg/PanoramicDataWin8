using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.model.view.operation
{
    public class FilterOperationViewModel : OperationViewModel
    {
        public FilterOperationViewModel(FilterOperationModel filterOperationModel, bool useTypingUI) : base(filterOperationModel)
        {
            UseTypingUI = useTypingUI;
            addAttachmentViewModels();
        }

        public override void Dispose()
        {
            FilterOperationModel.Dispose();
        }

        public bool UseTypingUI { get; set; }
        public FilterOperationModel FilterOperationModel => (FilterOperationModel)OperationModel;
    }
}