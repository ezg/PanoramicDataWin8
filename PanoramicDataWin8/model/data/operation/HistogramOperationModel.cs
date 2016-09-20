using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;
using IDEA_common.aggregates;
using IDEA_common.operations.histogram;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;

namespace PanoramicDataWin8.model.data
{
    [JsonObject(MemberSerialization.OptOut)]
    public class HistogramOperationModel : AttributeUsageOperationModel, IBrushableOperationModel, IBrusherOperationModel, IFilterConsumerOperationModel,
        IStatisticallyComparableOperationModel
    {
        private readonly BrushableOperationModelImpl _brushableOperationModelImpl;
        private readonly FilterProviderOperationModelOperationModel _filterProviderOperationModelOperationModel;
        private readonly FilterConsumerOperationModelOperationModel _filterConsumerOperationModelOperationModel;

        private VisualizationType _visualizationType;

        public HistogramOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
            _filterProviderOperationModelOperationModel = new FilterProviderOperationModelOperationModel(this);
            _filterConsumerOperationModelOperationModel = new FilterConsumerOperationModelOperationModel(this);
            _brushableOperationModelImpl = new BrushableOperationModelImpl(this);
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

        public FilteringOperation FilteringOperation
        {
            get { return _filterConsumerOperationModelOperationModel.FilteringOperation; }
            set { _filterConsumerOperationModelOperationModel.FilteringOperation = value; }
        }

        public ObservableCollection<FilterLinkModel> LinkModels
        {
            get { return _filterConsumerOperationModelOperationModel.LinkModels; }
            set { _filterConsumerOperationModelOperationModel.LinkModels = value; }
        }

        public ObservableCollection<FilterModel> FilterModels
        {
            get { return _filterProviderOperationModelOperationModel.FilterModels; }
        }

        public void ClearFilterModels()
        {
            _filterProviderOperationModelOperationModel.ClearFilterModels();
        }

        public void AddFilterModels(List<FilterModel> filterModels)
        {
            _filterProviderOperationModelOperationModel.AddFilterModels(filterModels);
        }

        public void AddFilterModel(FilterModel filterModel)
        {
            _filterProviderOperationModelOperationModel.AddFilterModel(filterModel);
        }

        public void RemoveFilterModel(FilterModel filterModel)
        {
            _filterProviderOperationModelOperationModel.RemoveFilterModel(filterModel);
        }

        public void RemoveFilterModels(List<FilterModel> filterModels)
        {
            _filterProviderOperationModelOperationModel.RemoveFilterModels(filterModels);
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
            {
                return new CountAggregateParameters {Dimension = iom.AttributeModel.Index};
            }
            if (iom.AggregateFunction == AggregateFunction.Avg)
            {
                return new AverageAggregateParameters {Dimension = iom.AttributeModel.Index};
            }
            if (iom.AggregateFunction == AggregateFunction.Max)
            {
                return new MaxAggregateParameters {Dimension = iom.AttributeModel.Index};
            }
            if (iom.AggregateFunction == AggregateFunction.Min)
            {
                return new MinAggregateParameters {Dimension = iom.AttributeModel.Index};
            }
            if (iom.AggregateFunction == AggregateFunction.Sum)
            {
                return new SumAggregateParameters {Dimension = iom.AttributeModel.Index};
            }
            if (iom.AggregateFunction == AggregateFunction.Count)
            {
                return new CountAggregateParameters {Dimension = iom.AttributeModel.Index};
            }
            return null;
        }

        public static AggregateKey CreateAggregateKey(AttributeTransformationModel iom, HistogramResult histogramResult, int brushIndex)
        {
            return new AggregateKey
            {
                AggregateParameterIndex = histogramResult.GetAggregateParametersIndex(CreateAggregateParameters(iom)),
                BrushIndex = brushIndex
            };
        }

        public static AggregateKey CreateAggregateKey(AttributeTransformationModel iom, SingleDimensionAggregateParameters aggParameters, HistogramResult histogramResult, int brushIndex)
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