using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;
using IDEA_common.aggregates;
using IDEA_common.operations.histogram;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.model.data
{
    [JsonObject(MemberSerialization.OptOut)]
    public class HistogramOperationModel : AttributeUsageOperationModel, IBrushableOperationModel,
        IBrusherOperationModel, IFilterConsumerOperationModel,
        IStatisticallyComparableOperationModel
    {
        private readonly BrushableOperationModelImpl _brushableOperationModelImpl;
        private readonly FilterConsumerOperationModelImpl _filterConsumerOperationModelImpl;
        private readonly FilterProviderOperationModelImpl _filterProviderOperationModelImpl;
        private readonly StatisticallyComparableOperationModelImpl _statisticallyComparableOperationModelImpl;

        private VisualizationType _visualizationType;

        public HistogramOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
            _filterProviderOperationModelImpl = new FilterProviderOperationModelImpl(this);
            _filterConsumerOperationModelImpl = new FilterConsumerOperationModelImpl(this);
            _brushableOperationModelImpl = new BrushableOperationModelImpl(this);
            _statisticallyComparableOperationModelImpl = new StatisticallyComparableOperationModelImpl(this);
        }

        public VisualizationType VisualizationType
        {
            get { return _visualizationType; }
            set { SetProperty(ref _visualizationType, value); }
        }

        public ObservableCollection<IBrushableOperationModel> BrushOperationModels
        {
            get { return _brushableOperationModelImpl.BrushOperationModels; }
            set { _brushableOperationModelImpl.BrushOperationModels = value; }
        }

        public List<Color> BrushColors { get; set; } = new List<Color>();

        public ObservableCollection<FilterModel> FilterModels
        {
            get { return _filterProviderOperationModelImpl.FilterModels; }
        }

        public void ClearFilterModels()
        {
            _filterProviderOperationModelImpl.ClearFilterModels();
        }

        public void AddFilterModels(List<FilterModel> filterModels)
        {
            _filterProviderOperationModelImpl.AddFilterModels(filterModels);
        }

        public void AddFilterModel(FilterModel filterModel)
        {
            _filterProviderOperationModelImpl.AddFilterModel(filterModel);
        }

        public void RemoveFilterModel(FilterModel filterModel)
        {
            _filterProviderOperationModelImpl.RemoveFilterModel(filterModel);
        }

        public void RemoveFilterModels(List<FilterModel> filterModels)
        {
            _filterProviderOperationModelImpl.RemoveFilterModels(filterModels);
        }

        public FilteringOperation FilteringOperation
        {
            get { return _filterConsumerOperationModelImpl.FilteringOperation; }
            set { _filterConsumerOperationModelImpl.FilteringOperation = value; }
        }

        public ObservableCollection<FilterLinkModel> LinkModels
        {
            get { return _filterConsumerOperationModelImpl.LinkModels; }
            set { _filterConsumerOperationModelImpl.LinkModels = value; }
        }

        public bool IncludeDistribution
        {
            get { return _statisticallyComparableOperationModelImpl.IncludeDistribution; }
            set { _statisticallyComparableOperationModelImpl.IncludeDistribution = value; }
        }
    }

    public enum VisualizationType
    {
        table,
        plot,
        map,
        line,
        county
    }

    public static class QueryModelHelper
    {
        public static AggregateParameters CreateAggregateParameters(AttributeTransformationModel iom)
        {
            if (iom.AggregateFunction == AggregateFunction.Count)
                return new CountAggregateParameters {Dimension = iom.AttributeModel.Index};
            if (iom.AggregateFunction == AggregateFunction.Avg)
                return new AverageAggregateParameters {Dimension = iom.AttributeModel.Index};
            if (iom.AggregateFunction == AggregateFunction.Max)
                return new MaxAggregateParameters {Dimension = iom.AttributeModel.Index};
            if (iom.AggregateFunction == AggregateFunction.Min)
                return new MinAggregateParameters {Dimension = iom.AttributeModel.Index};
            if (iom.AggregateFunction == AggregateFunction.Sum)
                return new SumAggregateParameters {Dimension = iom.AttributeModel.Index};
            if (iom.AggregateFunction == AggregateFunction.Count)
                return new CountAggregateParameters {Dimension = iom.AttributeModel.Index};
            return null;
        }

        public static AggregateKey CreateAggregateKey(AttributeTransformationModel iom, HistogramResult histogramResult,
            int brushIndex)
        {
            return new AggregateKey
            {
                AggregateParameterIndex = histogramResult.GetAggregateParametersIndex(CreateAggregateParameters(iom)),
                BrushIndex = brushIndex
            };
        }

        public static AggregateKey CreateAggregateKey(AttributeTransformationModel iom,
            SingleDimensionAggregateParameters aggParameters, HistogramResult histogramResult, int brushIndex)
        {
            aggParameters.Dimension = iom.AttributeModel.Index;
            return new AggregateKey
            {
                AggregateParameterIndex = histogramResult.GetAggregateParametersIndex(aggParameters),
                BrushIndex = brushIndex
            };
        }
    }
}