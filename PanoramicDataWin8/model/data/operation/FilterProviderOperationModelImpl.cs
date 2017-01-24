using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IDEA_common.operations;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.operation
{
    public class FilterProviderOperationModelImpl : ExtendedBindableBase, IFilterProviderOperationModel
    {
        private readonly IOperationModel _host;

        public FilterProviderOperationModelImpl(IOperationModel host)
        {
            _host = host;
        }

        public ObservableCollection<FilterModel> FilterModels { get; } = new ObservableCollection<FilterModel>();
        public ObservableCollection<BczBinMapModel> BczBinMapModels { get; } = new ObservableCollection<BczBinMapModel>();

        public void ClearFilterModels()
        {
            foreach (var filterModel in FilterModels.ToArray())
                FilterModels.Remove(filterModel);
            fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.ClearFilterModels);
        }


        public void AddFilterModels(List<FilterModel> filterModels)
        {
            foreach (var filterModel in filterModels)
                FilterModels.Add(filterModel);
            fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.FilterModels);
        }
        public void AddBczBinMapModels(List<BczBinMapModel> binMapModels)
        {
            var newFilters = new ObservableCollection<BczBinMapModel>();
            foreach (var fm in binMapModels)
            {
                newFilters.Add(fm);
            }
            foreach (var filterModel in BczBinMapModels)
            {
                bool skip = false;
                foreach (var fm in newFilters)
                    if (fm.SortAxis == filterModel.SortAxis)
                        skip = true;
                if (!skip)
                    newFilters.Add(filterModel);
            }
            BczBinMapModels.Clear();
            foreach (var fm in newFilters)
                BczBinMapModels.Add(fm);

            fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.BczBinMapModels);
        }
        
        public void AddFilterModel(FilterModel filterModel)
        {
            FilterModels.Add(filterModel);
            fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.FilterModels);
        }
        public void AddBczBinMapModel(BczBinMapModel binMapModel)
        {
            BczBinMapModels.Add(binMapModel);
            fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.BczBinMapModels);
        }

        public void RemoveFilterModel(FilterModel filterModel)
        {
            FilterModels.Remove(filterModel);
            fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.FilterModels);
        }
        public void RemoveBczBinMapModel(BczBinMapModel binMapModel)
        {
            BczBinMapModels.Remove(binMapModel);
            fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.BczBinMapModels);
        }

        public void RemoveFilterModels(List<FilterModel> filterModels)
        {
            foreach (var filterItem in filterModels)
                FilterModels.Remove(filterItem);
            if (filterModels.Count > 0)
                fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.FilterModels);
        }
        public void RemoveBczBinMapModels(List<BczBinMapModel> binMapModels)
        {
            foreach (var mapItem in binMapModels)
                BczBinMapModels.Remove(mapItem);
            if (binMapModels.Count > 0)
                fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.BczBinMapModels);
        }

        public event OperationModel.OperationModelUpdatedHandler OperationModelUpdated;

        public void FireOperationModelUpdated(OperationModelUpdatedEventArgs args)
        {
            _host.FireOperationModelUpdated(args);
        }
        public int ExecutionId { get; set; } = 0;
        public IResult Result { get; set; }
        public IOperationModel ResultCauserClone { get; set; }
        public SchemaModel SchemaModel { get; set; }

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