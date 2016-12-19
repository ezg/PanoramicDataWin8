using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IDEA_common.operations.risk;
using PanoramicDataWin8.model.view.operation;

namespace PanoramicDataWin8.model.data.operation
{
    public class StatisticalComparisonDecisionOperationModel : OperationModel
    {
        private List<ComparisonId> _comparisonIds = new List<ComparisonId>();
        private ModelId _modelId;
        private RiskControlType _riskControlType = RiskControlType.PCER;

        public StatisticalComparisonDecisionOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
        }

        public ModelId ModelId
        {
            get { return _modelId; }
            set { SetProperty(ref _modelId, value); }
        }


        public List<ComparisonId> ComparisonIds
        {
            get { return _comparisonIds; }
            set { SetProperty(ref _comparisonIds, value); }
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
            _statisticallyComparableOperationModels.CollectionChanged += _statisticallyComparableOperationModels_CollectionChanged;
        }

        private void _statisticallyComparableOperationModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var statComp in e.OldItems.OfType<IStatisticallyComparableOperationModel>())
                {
                    statComp.OperationModelUpdated -= StatComp_OperationModelUpdated;
                }
            }
            if (e.NewItems != null)
            {
                foreach (var statComp in e.NewItems.OfType<IStatisticallyComparableOperationModel>())
                {
                    statComp.OperationModelUpdated += StatComp_OperationModelUpdated;
                }
            }
        }

        private void StatComp_OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            FireOperationModelUpdated(e);
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
            _statisticallyComparableOperationModels.Remove(model);
            model.IncludeDistribution = false;
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