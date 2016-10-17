namespace PanoramicDataWin8.model.data.operation
{
    public class FilterOperationModelUpdatedEventArgs : OperationModelUpdatedEventArgs
    {
        public FilterOperationModelUpdatedEventArgs(FilterOperationModelUpdatedEventType type)
        {
            FilterOperationModelUpdatedEventType = type;
        }

        public FilterOperationModelUpdatedEventType FilterOperationModelUpdatedEventType { get; set; }
    }
}