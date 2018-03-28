﻿using System.Collections.Generic;
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

        public StatisticalComparisonDecisionOperationModel(OriginModel schemaModel) : base(schemaModel)
        {
        }

        public override void Dispose()
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
        private int _comparisonOrder = -1;
        private ModelId _modelId;
        private Decision _decision;
        private StatistalComparisonType _statistalComparisonType = StatistalComparisonType.histogram;
        private TestType _testType = TestType.chi2;


        private ObservableCollection<IStatisticallyComparableOperationModel> _statisticallyComparableOperationModels =
            new ObservableCollection<IStatisticallyComparableOperationModel>();


        public StatisticalComparisonOperationModel(OriginModel schemaModel) : base(schemaModel)
        {
            _statisticallyComparableOperationModels.CollectionChanged += _statisticallyComparableOperationModels_CollectionChanged;
        }

        public override void Dispose()
        {
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


        public Decision Decision
        {
            get { return _decision; }
            set { SetProperty(ref _decision, value); }
        }


        public int ComparisonOrder
        {
            get { return _comparisonOrder; }
            set { SetProperty(ref _comparisonOrder, value); }
        }

        public ModelId ModelId
        {
            get { return _modelId; }
            set { SetProperty(ref _modelId, value); }
        }

        public TestType TestType
        {
            get { return _testType; }
            set { SetProperty(ref _testType, value); }
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