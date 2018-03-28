using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.controller.data.progressive;
using IDEA_common.operations;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;
using IDEA_common.aggregates;

namespace PanoramicDataWin8.model.data.operation
{
    [JsonObject(MemberSerialization.OptOut)]
    public abstract class BaseVisualizationOperationModel : OperationModel, IBrushableOperationModel,
        IBrusherOperationModel, IFilterConsumerOperationModel
    {
        private readonly BrushableOperationModelImpl _brushableOperationModelImpl;
        private readonly FilterConsumerOperationModelImpl _filterConsumerOperationModelImpl;
        private readonly FilterProviderOperationModelImpl _filterProviderOperationModelImpl;


        private VisualizationType _visualizationType;

        public BaseVisualizationOperationModel(OriginModel schemaModel) : base(schemaModel)
        {
            _filterProviderOperationModelImpl = new FilterProviderOperationModelImpl(this);
            _filterConsumerOperationModelImpl = new FilterConsumerOperationModelImpl(this);
            _brushableOperationModelImpl = new BrushableOperationModelImpl(this);
        }
        /// <summary>
        /// Called when one of the code attributes has changed.  If that code attribute is used for
        /// the calculation of this Histogram, then we fire a model changed event so that the histogram
        /// and any dependent brushed views will be updated.
        /// </summary>
        /// <param name="sender"></param>
      

        public int IExecutionId { get; set; } = 0;


        public VisualizationType VisualizationType
        {
            get { return _visualizationType; }
            set { SetProperty(ref _visualizationType, value); }
        }

        public ObservableCollection<IBrusherOperationModel> BrushOperationModels
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

        public ObservableCollection<FilterLinkModel> ConsumerLinkModels
        {
            get { return _filterConsumerOperationModelImpl.ConsumerLinkModels; }
            set { _filterConsumerOperationModelImpl.ConsumerLinkModels = value; }
        }
        public ObservableCollection<FilterLinkModel> ProviderLinkModels
        {
            get { return _filterProviderOperationModelImpl.ProviderLinkModels; }
            set { _filterProviderOperationModelImpl.ProviderLinkModels = value; }
        }

    }
}