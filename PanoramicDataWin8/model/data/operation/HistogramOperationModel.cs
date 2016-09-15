using System.Collections.Generic;
using System.Collections.ObjectModel;
using IDEA_common.aggregates;
using IDEA_common.operations.histogram;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;

namespace PanoramicDataWin8.model.data
{
    [JsonObject(MemberSerialization.OptOut)]
    public class HistogramOperationModel : AttributeUsageOperationModel, IBrushableOperationModel, IFilterableOperationModel
    {
        private readonly BrushableOperationModelImpl _brushableOperationModelImpl;
        private readonly FilterableOperationModelImpl _filterableOperationModelImpl;

        private ObservableCollection<ComparisonViewModel> _comparisonViewModels = new ObservableCollection<ComparisonViewModel>();

        private VisualizationType _visualizationType;

        public HistogramOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
            _filterableOperationModelImpl = new FilterableOperationModelImpl(this);
            _brushableOperationModelImpl = new BrushableOperationModelImpl(this);
        }

        public VisualizationType VisualizationType
        {
            get { return _visualizationType; }
            set { SetProperty(ref _visualizationType, value); }
        }

        [JsonIgnore]
        public ObservableCollection<ComparisonViewModel> ComparisonViewModels
        {
            get { return _comparisonViewModels; }
            set { SetProperty(ref _comparisonViewModels, value); }
        }

        public ObservableCollection<OperationModel> BrushOperationModels
        {
            get { return _brushableOperationModelImpl.BrushOperationModels; }
            set { _brushableOperationModelImpl.BrushOperationModels = value; }
        }

        public FilteringOperation FilteringOperation
        {
            get { return _filterableOperationModelImpl.FilteringOperation; }
            set { _filterableOperationModelImpl.FilteringOperation = value; }
        }

        public ObservableCollection<FilterLinkModel> LinkModels
        {
            get { return _filterableOperationModelImpl.LinkModels; }
            set { _filterableOperationModelImpl.LinkModels = value; }
        }

        public ObservableCollection<FilterModel> FilterModels
        {
            get { return _filterableOperationModelImpl.FilterModels; }
        }

        public void ClearFilterModels()
        {
            _filterableOperationModelImpl.ClearFilterModels();
        }

        public void AddFilterModels(List<FilterModel> filterModels)
        {
            _filterableOperationModelImpl.AddFilterModels(filterModels);
        }

        public void AddFilterModel(FilterModel filterModel)
        {
            _filterableOperationModelImpl.AddFilterModel(filterModel);
        }

        public void RemoveFilterModel(FilterModel filterModel)
        {
            _filterableOperationModelImpl.RemoveFilterModel(filterModel);
        }

        public void RemoveFilterModels(List<FilterModel> filterModels)
        {
            _filterableOperationModelImpl.RemoveFilterModels(filterModels);
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