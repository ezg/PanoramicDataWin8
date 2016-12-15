using System.Collections.ObjectModel;
using IDEA_common.operations.risk;

namespace PanoramicDataWin8.model.data.operation
{
    public class StatisticalComparisonDecisionOperationModel : OperationModel
    {
        private ComparisonId _comparisonId;
        private ModelId _modelId;
        private StatisticalComparisonOperationModel _parent;
        private RiskControlType _riskControlType = RiskControlType.PCER;

        public StatisticalComparisonDecisionOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
        }

        public StatisticalComparisonOperationModel Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value); }
        }

        public ModelId ModelId
        {
            get { return _modelId; }
            set { SetProperty(ref _modelId, value); }
        }


        public ComparisonId ComparisonId
        {
            get { return _comparisonId; }
            set { SetProperty(ref _comparisonId, value); }
        }


        public RiskControlType RiskControlType
        {
            get { return _riskControlType; }
            set { SetProperty(ref _riskControlType, value); }
        }
    }

    public class StatisticalComparisonOperationModel : OperationModel
    {
        private int _comparisionOrder = -1;
        private ModelId _modelId;
        private StatistalComparisonType _statistalComparisonType = StatistalComparisonType.histogram;
        private StatisticalComparisonDecisionOperationModel _statisticalComparisonDecisionOperationModel;


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
                {
                    statisticallyComparableOperationModel.IncludeDistribution = _statistalComparisonType == StatistalComparisonType.distribution;
                }
            }
        }

        public StatisticalComparisonDecisionOperationModel StatisticalComparisonDecisionOperationModel
        {
            get { return _statisticalComparisonDecisionOperationModel; }
            set { SetProperty(ref _statisticalComparisonDecisionOperationModel, value); }
        }

        public int ComparisonOrder
        {
            get { return _comparisionOrder; }
            set { SetProperty(ref _comparisionOrder, value); }
        }

        public ModelId ModelId
        {
            get { return _modelId; }
            set { SetProperty(ref _modelId, value); }
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
            {
                model.IncludeDistribution = true;
            }
            _statisticallyComparableOperationModels.Add(model);
        }
    }
}