using System.Collections.Generic;
using System.Threading.Tasks;

namespace PanoramicData.controller.data
{
    public interface IItemsProvider<T>
    {
        Task<int> FetchCount();
        Task<IList<T>> FetchPage(int pageIndex, int pageLength);
    }
}
