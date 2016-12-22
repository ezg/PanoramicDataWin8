using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using IDEA_common.operations;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.operation
{
    public class FilterConsumerOperationModelImpl : ExtendedBindableBase, IFilterConsumerOperationModel
    {
        private FilteringOperation _filteringOperation = FilteringOperation.AND;
        private readonly IOperationModel _host;

        private ObservableCollection<FilterLinkModel> _linkModels = new ObservableCollection<FilterLinkModel>();

        public FilterConsumerOperationModelImpl(IOperationModel host)
        {
            _linkModels.CollectionChanged += LinkModels_CollectionChanged;
            _host = host;
        }

        public FilteringOperation FilteringOperation
        {
            get { return _filteringOperation; }
            set { SetProperty(ref _filteringOperation, value); }
        }
        public int ExecutionId { get; set; } = 0;
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
        public IOperationModel ResultCauserClone { get; set; }
        public SchemaModel SchemaModel { get; set; }

        public OperationModel Clone()
        {
            throw new NotImplementedException();
        }

        private void LinkModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var fire = false;
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    if (Equals(((FilterLinkModel) item).ToOperationModel, _host))
                    {
                        ((FilterLinkModel) item).FromOperationModel.OperationModelUpdated -= FromOperationModel_OperationModelUpdated;
                        fire = true;
                    }
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    if (Equals(((FilterLinkModel) item).ToOperationModel, _host))
                    {
                        ((FilterLinkModel) item).FromOperationModel.OperationModelUpdated += FromOperationModel_OperationModelUpdated;
                        fire = true;
                    }
            if (fire)
                fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.Links);
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
}