using System.Collections.ObjectModel;

namespace PanoramicDataWin8.model.data.operation
{
    public interface IStatisticallyComparableOperationModel : IOperationModel
    {
    }

    public class StatisticalComparisonOperationModel : OperationModel
    {
        private ObservableCollection<IStatisticallyComparableOperationModel> _statisticallyComparableOperationModels =
            new ObservableCollection<IStatisticallyComparableOperationModel>();

        public StatisticalComparisonOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
        }

        public ObservableCollection<IStatisticallyComparableOperationModel> StatisticallyComparableOperationModels
        {
            get { return _statisticallyComparableOperationModels; }
            set { SetProperty(ref _statisticallyComparableOperationModels, value); }
        }
    }
}