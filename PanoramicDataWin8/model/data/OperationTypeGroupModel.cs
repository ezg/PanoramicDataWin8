using System.Collections.ObjectModel;

namespace PanoramicDataWin8.model.data
{
    public class OperationTypeGroupModel : OperationTypeModel
    {
        public ObservableCollection<OperationTypeModel> OperationTypeModels { get; } = new ObservableCollection<OperationTypeModel>();
    }
}