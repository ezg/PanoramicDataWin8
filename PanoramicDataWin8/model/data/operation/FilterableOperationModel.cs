using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.operation
{
    public interface IFilterableOperationModel
    {
        FilteringOperation FilteringOperation { get; set; }
        
        ObservableCollection<FilterLinkModel> LinkModels { get; }
        ObservableCollection<FilterModel> FilterModels { get; }
        void ClearFilterModels();

        void AddFilterModels(List<FilterModel> filterModels);

        void AddFilterModel(FilterModel filterModel);

        void RemoveFilterModel(FilterModel filterModel);

        void RemoveFilterModels(List<FilterModel> filterModels);
    }

    public class FilterableOperationModelImpl : ExtendedBindableBase, IFilterableOperationModel
    {
        private FilteringOperation _filteringOperation = FilteringOperation.AND;
        
        private ObservableCollection<FilterLinkModel> _linkModels = new ObservableCollection<FilterLinkModel>();
        private OperationModel _host;

        public FilterableOperationModelImpl(OperationModel host)
        {
            _host = host;
            _linkModels.CollectionChanged += LinkModels_CollectionChanged;
        }

        public FilteringOperation FilteringOperation
        {
            get { return _filteringOperation; }
            set { SetProperty(ref _filteringOperation, value); }
        }

        public ObservableCollection<FilterModel> FilterModels { get; } = new ObservableCollection<FilterModel>();

        public ObservableCollection<FilterLinkModel> LinkModels
        {
            get { return _linkModels; }
            set { SetProperty(ref _linkModels, value); }
        }
        
        private void fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType type)
        {
            /*if (type == FilterOperationModelUpdatedEventType.Links)
            {
                ClearFilterModels();
            }
            FilterOperationModelUpdated?.Invoke(this, new FilterOperationModelUpdatedEventArgs(type));

            if (type != FilterOperationModelUpdatedEventType.FilterModels)
            {
                SchemaModel.QueryExecuter?.ExecuteOperationModel(this);
            }*/
            _host.FireOperationModelUpdated(new FilterOperationModelUpdatedEventArgs(type));
        }

        public void ClearFilterModels()
        {
            foreach (var filterModel in FilterModels.ToArray())
            {
                FilterModels.Remove(filterModel);
            }
            fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.ClearFilterModels);
        }

        public void AddFilterModels(List<FilterModel> filterModels)
        {
            foreach (var filterModel in filterModels)
            {
                FilterModels.Add(filterModel);
            }
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
            {
                FilterModels.Remove(filterItem);
            }
            if (filterModels.Count > 0)
            {
                fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.FilterModels);
            }
        }

        private void LinkModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool fire = false;
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (Equals(((FilterLinkModel) item).ToOperationModel, this._host))
                    {
                        ((FilterLinkModel) item).FromOperationModel.OperationModelUpdated -= FromOperationModel_OperationModelUpdated;
                        fire = true;
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (Equals(((FilterLinkModel) item).ToOperationModel, this._host))
                    {
                        ((FilterLinkModel)item).FromOperationModel.OperationModelUpdated += FromOperationModel_OperationModelUpdated;
                        fire = true;
                    }
                }
            }
            if (fire)
            {
                fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.Links);
            }
        }

        private void FromOperationModel_OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.Links);
        }
    }


    public class FilterOperationModelUpdatedEventArgs : OperationModelUpdatedEventArgs
    {
        public FilterOperationModelUpdatedEventArgs(FilterOperationModelUpdatedEventType type)
        {
            FilterOperationModelUpdatedEventType = type;
        }

        public FilterOperationModelUpdatedEventType FilterOperationModelUpdatedEventType { get; set; }
    }


    public enum FilterOperationModelUpdatedEventType
    {
        Links,
        FilterModels,
        ClearFilterModels
    }
}