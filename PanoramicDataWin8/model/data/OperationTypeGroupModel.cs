using System.Collections.ObjectModel;

namespace PanoramicDataWin8.model.data
{
    public class OperationTypeGroupModel : OperationTypeModel
    {
        private ObservableCollection<OperationTypeModel> _operationTypeModels = new ObservableCollection<OperationTypeModel>();
        public ObservableCollection<OperationTypeModel> OperationTypeModels
        {
            get
            {
                return _operationTypeModels;
            }
        }
    }
}
