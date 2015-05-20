using System.Collections.Generic;
using System.Threading.Tasks;

namespace PanoramicDataWin8.controller.data.virt
{
    public interface IItemsProvider<T>
    {
        Task<int> FetchCount();
        Task<IList<T>> FetchPage(int pageIndex, int pageLength);
    }
}
