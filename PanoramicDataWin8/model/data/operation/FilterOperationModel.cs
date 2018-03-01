using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;

namespace PanoramicDataWin8.model.data.operation
{
    public class FilterOperationModel : OperationModel, IBrusherOperationModel, IFilterConsumerOperationModel
    {
        private readonly BrushableOperationModelImpl _brushableOperationModelImpl;
        private readonly FilterProviderOperationModelImpl _filterProviderOperationModelImpl;
        private readonly FilterConsumerOperationModelImpl _filterConsumerOperationModelImpl;

        public FilterOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
            _filterProviderOperationModelImpl = new FilterProviderOperationModelImpl(this);
            _filterConsumerOperationModelImpl = new FilterConsumerOperationModelImpl(this);
            _brushableOperationModelImpl = new BrushableOperationModelImpl(this);
        }

        public override void Dispose()
        {
        }

        public ObservableCollection<FilterModel> FilterModels
        {
            get { return _filterProviderOperationModelImpl.FilterModels; }
        }


        public override bool ResetFilterModelWhenInputLinksChange { get { return false; } }

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

        public void ClearFilterModels()
        {
            _filterProviderOperationModelImpl.ClearFilterModels();
        }
        
        public ObservableCollection<FilterLinkModel> ProviderLinkModels
        {
            get { return _filterProviderOperationModelImpl.ProviderLinkModels; }
            set { _filterProviderOperationModelImpl.ProviderLinkModels = value; }
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
    }
}