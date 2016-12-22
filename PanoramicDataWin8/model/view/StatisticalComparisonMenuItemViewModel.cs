using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.model.view
{
    public class StatisticalComparisonMenuItemViewModel : MenuItemComponentViewModel
    {
        private StatisticalComparisonOperationModel _statisticalComparisonOperationModel;
        
        public StatisticalComparisonOperationModel StatisticalComparisonOperationModel
        {
            get { return _statisticalComparisonOperationModel; }
            set { SetProperty(ref _statisticalComparisonOperationModel, value); }
        }
    }
}