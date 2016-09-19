using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using IDEA_common.operations;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.operation
{
    public interface IFilterProviderOperationModel : IOperationModel
    {
        ObservableCollection<FilterModel> FilterModels { get; }
        void AddFilterModels(List<FilterModel> filterModels);
        void AddFilterModel(FilterModel filterModel);
        void RemoveFilterModel(FilterModel filterModel);
        void RemoveFilterModels(List<FilterModel> filterModels);
        void ClearFilterModels();
    }

    public interface IFilterConsumerOperationModel : IOperationModel
    {
        FilteringOperation FilteringOperation { get; set; }
        ObservableCollection<FilterLinkModel> LinkModels { get; }
    }

    public class FilterConsumerOperationModelOperationModel : ExtendedBindableBase, IFilterConsumerOperationModel
    {
        private FilteringOperation _filteringOperation = FilteringOperation.AND;
        private IOperationModel _host;

        private ObservableCollection<FilterLinkModel> _linkModels = new ObservableCollection<FilterLinkModel>();

        public FilterConsumerOperationModelOperationModel(IOperationModel host)
        {
            _linkModels.CollectionChanged += LinkModels_CollectionChanged;
            _host = host;
        }

        public FilteringOperation FilteringOperation
        {
            get { return _filteringOperation; }
            set { SetProperty(ref _filteringOperation, value); }
        }

        public ObservableCollection<FilterLinkModel> LinkModels
        {
            get { return _linkModels; }
            set { SetProperty(ref _linkModels, value); }
        }

        public event OperationModel.OperationModelUpdatedHandler OperationModelUpdated;

        public void FireOperationModelUpdated(OperationModelUpdatedEventArgs args)
        {
            _host.FireOperationModelUpdated(args);
        }

        public IResult Result { get; set; }
        public SchemaModel SchemaModel { get; set; }
        public OperationModel Clone()
        {
            throw new System.NotImplementedException();
        }

        private void LinkModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool fire = false;
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (Equals(((FilterLinkModel) item).ToOperationModel, _host))
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
                    if (Equals(((FilterLinkModel) item).ToOperationModel, _host))
                    {
                        ((FilterLinkModel) item).FromOperationModel.OperationModelUpdated += FromOperationModel_OperationModelUpdated;
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

        private void fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType type)
        {
            _host.FireOperationModelUpdated(new FilterOperationModelUpdatedEventArgs(type));
        }
    }

    public class FilterProviderOperationModelOperationModel : ExtendedBindableBase, IFilterProviderOperationModel
    {
        private IOperationModel _host;

        public FilterProviderOperationModelOperationModel(IOperationModel host)
        {
            _host = host;
        }

        public ObservableCollection<FilterModel> FilterModels { get; } = new ObservableCollection<FilterModel>();

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

        public event OperationModel.OperationModelUpdatedHandler OperationModelUpdated;

        public void FireOperationModelUpdated(OperationModelUpdatedEventArgs args)
        {
            _host.FireOperationModelUpdated(args);
        }

        public IResult Result { get; set; }
        public SchemaModel SchemaModel { get; set; }
        public OperationModel Clone()
        {
            throw new System.NotImplementedException();
        }

        private void fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType type)
        {
            _host.FireOperationModelUpdated(new FilterOperationModelUpdatedEventArgs(type));
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