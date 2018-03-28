using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IDEA_common.operations;
using PanoramicDataWin8.utils;
using System.Collections.Specialized;

namespace PanoramicDataWin8.model.data.operation
{
    public class FilterProviderOperationModelImpl : ExtendedBindableBase, IFilterProviderOperationModel
    {
        private readonly IOperationModel _host;
        private ObservableCollection<FilterLinkModel> _linkProviderModels = new ObservableCollection<FilterLinkModel>();

        public FilterProviderOperationModelImpl(IOperationModel host)
        {
            _host = host;
        }

        public void Dispose()
        {

        }

        public ObservableCollection<FilterModel> FilterModels { get; } = new ObservableCollection<FilterModel>();
        public ObservableCollection<FilterLinkModel> ProviderLinkModels
        {
            get { return _linkProviderModels; }
            set { SetProperty(ref _linkProviderModels, value); }
        }

        public void ClearFilterModels()
        {
            if (FilterModels.Count > 0)
            {
                foreach (var filterModel in FilterModels.ToArray())
                    FilterModels.Remove(filterModel);
                fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.ClearFilterModels);
            }
        }


        public void AddFilterModels(List<FilterModel> filterModels)
        {
            foreach (var filterModel in filterModels)
                FilterModels.Add(filterModel);
            fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.FilterModels);
        }
        
        public void AddFilterModel(FilterModel filterModel)
        {
            FilterModels.Add(filterModel);
            fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.FilterModels);
        }

        public void RemoveFilterModel(FilterModel filterModel)
        {
            FilterModels.Remove(filterModel);
            fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.FilterModels);
        }

        public void RemoveFilterModels(List<FilterModel> filterModels)
        {
            foreach (var filterItem in filterModels)
                FilterModels.Remove(filterItem);
            if (filterModels.Count > 0)
                fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.FilterModels);
        }

        public event OperationModel.OperationModelUpdatedHandler OperationModelUpdated;

        public void FireOperationModelUpdated(OperationModelUpdatedEventArgs args)
        {
            _host.FireOperationModelUpdated(args);
        }
        public int ExecutionId { get; set; } = 0;
        public IResult Result { get; set; }
        public ResultParameters ResultParameters { get; }
        public IOperationModel ResultCauserClone { get; set; }
        public OriginModel OriginModel { get; set; }

        public OperationModel Clone()
        {
            throw new NotImplementedException();
        }

        private void fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType type)
        {
            _host.FireOperationModelUpdated(new FilterOperationModelUpdatedEventArgs(type));
        }
    }
}