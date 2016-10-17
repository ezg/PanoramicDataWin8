using System.Collections.Generic;
using System.Collections.ObjectModel;

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
}