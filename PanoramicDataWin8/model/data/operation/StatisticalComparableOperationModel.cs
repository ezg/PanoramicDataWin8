using System;
using System.Collections.ObjectModel;
using IDEA_common.operations;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.operation
{
    public interface IStatisticallyComparableOperationModel : IOperationModel
    {
        bool IncludeDistribution { get; set; }
    }

    public class StatisticallyComparableOperationModelImpl : ExtendedBindableBase,
        IStatisticallyComparableOperationModel
    {
        private IOperationModel _host;

        private bool _includeDistribution;

        public StatisticallyComparableOperationModelImpl(IOperationModel host)
        {
            _host = host;
        }

        public event OperationModel.OperationModelUpdatedHandler OperationModelUpdated;

        public void FireOperationModelUpdated(OperationModelUpdatedEventArgs args)
        {
            throw new NotImplementedException();
        }

        public IResult Result { get; set; }
        public IOperationModel ResultCauserClone { get; set; }
        public SchemaModel SchemaModel { get; set; }

        public OperationModel Clone()
        {
            throw new NotImplementedException();
        }

        public bool IncludeDistribution
        {
            get { return _includeDistribution; }
            set
            {
                SetProperty(ref _includeDistribution, value);
                _host.FireOperationModelUpdated(new VisualOperationModelUpdatedEventArgs());
            }
        }
    }

    public class StatisticalComparisonOperationModel : OperationModel
    {
        private ObservableCollection<IStatisticallyComparableOperationModel> _statisticallyComparableOperationModels =
            new ObservableCollection<IStatisticallyComparableOperationModel>();
        private StatistalComparisonType _statistalComparisonType = StatistalComparisonType.histogram;


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

        public ObservableCollection<IStatisticallyComparableOperationModel> StatisticallyComparableOperationModels
        {
            get { return _statisticallyComparableOperationModels; }
            set { SetProperty(ref _statisticallyComparableOperationModels, value); }
        }
    }

    public enum StatistalComparisonType
    {
        histogram, distribution
    }
}