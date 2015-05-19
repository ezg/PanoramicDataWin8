using System.Net.Http;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.data.tuppleware;

namespace PanoramicDataWin8.controller.data.tuppleware
{
    public class TuppleWareQueryExecuter : QueryExecuter
    {
        public override void ExecuteQuery(QueryModel queryModel)
        {
            IItemsProvider<ResultItemModel> itemsProvider = new TuppleWareItemsProvider(queryModel.Clone());
            AsyncVirtualizedCollection<ResultItemModel> dataValues = new AsyncVirtualizedCollection<ResultItemModel>(itemsProvider,
                1000,  // page size
                -1);
            //queryModel.ResultModel.ResultItemModels = dataValues;
        }
    }
}
