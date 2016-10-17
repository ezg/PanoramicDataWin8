using System.Collections.ObjectModel;

namespace PanoramicDataWin8.model.data.operation
{
    public class StatisticalComparisonOperationModel : OperationModel
    {
        private StatistalComparisonType _statistalComparisonType = StatistalComparisonType.histogram;

        private ObservableCollection<IStatisticallyComparableOperationModel> _statisticallyComparableOperationModels =
            new ObservableCollection<IStatisticallyComparableOperationModel>();


        public StatisticalComparisonOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
        }

        public StatistalComparisonType StatistalComparisonType
        {
            get { return _statistalComparisonType; }
            set
            {
                SetProperty(ref _statistalComparisonType, value);
                foreach (var statisticallyComparableOperationModel in _statisticallyComparableOperationModels)
                    statisticallyComparableOperationModel.IncludeDistribution = _statistalComparisonType == StatistalComparisonType.distribution;
            }
        }

        public ObservableCollection<IStatisticallyComparableOperationModel> StatisticallyComparableOperationModels
        {
            get { return _statisticallyComparableOperationModels; }
            set { SetProperty(ref _statisticallyComparableOperationModels, value); }
        }

        public void RemoveStatisticallyComparableOperationModel(IStatisticallyComparableOperationModel model)
        {
            model.IncludeDistribution = false;
            _statisticallyComparableOperationModels.Remove(model);
        }

        public void AddStatisticallyComparableOperationModel(IStatisticallyComparableOperationModel model)
        {
            if (_statistalComparisonType == StatistalComparisonType.distribution)
                model.IncludeDistribution = true;
            _statisticallyComparableOperationModels.Add(model);
        }
    }
}