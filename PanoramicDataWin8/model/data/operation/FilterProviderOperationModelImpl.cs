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
        public void ClearBczBinMapModels()
        {
            foreach (var mapModel in BczBinMapModels.ToArray())
                BczBinMapModels.Remove(mapModel);
            fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.ClearBczBinMapModels);
        }


        public void AddFilterModels(List<FilterModel> filterModels)
        {
            foreach (var filterModel in filterModels)
                FilterModels.Add(filterModel);
            fireFilterOperationModelUpdated(FilterOperationModelUpdatedEventType.FilterModels);
        }
        public void AddBczBinMapModels(List<BczBinMapModel> binMapModels)
        {
            var newbinMaps = new ObservableCollection<BczBinMapModel>();
            foreach (var binMapModel in binMapModels)
                BczBinMapModels.Add(binMapModel);// newbinMaps.Add(fm);
            // remove any existing binMap model that has the same sort axis as a new bin map model
            //foreach (var binMapModel in BczBinMapModels)
            //{
            //    bool skip = false;
            //    foreach (var fm in newbinMaps)
            //        if (fm.SortAxis == binMapModel.SortAxis)
            //            skip = true;
            //    if (!skip)
            //        newbinMaps.Add(binMapModel);
            //}
            //BczBinMapModels.Clear();
            //foreach (var fm in newbinMaps)
            //    BczBinMapModels.Add(fm);

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