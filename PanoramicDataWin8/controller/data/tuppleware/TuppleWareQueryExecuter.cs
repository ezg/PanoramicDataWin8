using PanoramicData.model.data;
using PanoramicData.model.data.sim;
using PanoramicData.model.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Dynamic;
using System.Diagnostics;
using PanoramicDataWin8.utils;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Serialization;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace PanoramicData.controller.data.sim
{
    public class TuppleWareQueryExecuter : QueryExecuter
    {
        public override void ExecuteQuery(QueryModel queryModel)
        {
            /*IItemsProvider<QueryResultItemModel> itemsProvider = new SimItemsProvider(queryModel.Clone(), (queryModel.SchemaModel.OriginModels[0] as SimOriginModel).Data);
            AsyncVirtualizingCollection<QueryResultItemModel> dataValues = new AsyncVirtualizingCollection<QueryResultItemModel>(itemsProvider,
                queryModel.VisualizationType == VisualizationType.Table ? 1000 : (queryModel.SchemaModel.OriginModels[0] as SimOriginModel).Data.Count + 1,  // page size
                1000);
            queryModel.QueryResultModel.QueryResultItemModels = dataValues;*/
        }

        public void LoadFileDescription(TuppleWareOriginModel tuppleWareOriginModel)
        {
            TuppleWareWebClient web = new TuppleWareWebClient();
            string responseStr = "{\"files\":[{\"id\":0,\"names\":[\"a0\",\"a1\"],\"types\":[\"float\",\"float\"]}]}";//await web.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, "files");
            dynamic responseObj = JObject.Parse(responseStr);

            for (int i = 0; i < responseObj["files"][0]["names"].Count; i++)
            {
                TuppleWareAttributeModel attributeModel = new TuppleWareAttributeModel(responseObj["files"][0]["names"][i].Value, responseObj["files"][0]["types"][0].Value, "numeric");
                attributeModel.OriginModel = tuppleWareOriginModel;
                tuppleWareOriginModel.AttributeModels.Add(attributeModel);
            }
        }
    }

    public class TuppleWareWebClient
    {
        public async Task<string> Request(string endPoint, string query)
        {
            if (query == "files")
                return "{\"files\":[{\"id\":0,\"names\":[\"a0\",\"a1\"],\"types\":[\"float\",\"float\"]}]}";
            else if (query.StartsWith("job"))
                return "{\"k\":[[0,0],[1,1],[2,2]],\"samples\":[[0.5,0.5],[0.9,0.9],[2.1,2.1],[2.5,2.5]]}";
            else if (query.StartsWith("sample"))
                return "{\"samples\":[[0.5,0.5],[0.9,0.9],[2.1,2.1]]}";
            else
                return "";
            var httpClient = new HttpClient();
            var content = await httpClient.GetStringAsync(endPoint + "/" + query);
            return content;
        }
    }

    public class TuppleWareItemsProvider : IItemsProvider<QueryResultItemModel>
    {
        private QueryModel _queryModel = null;
        private int _fetchCount = -1;

        public TuppleWareItemsProvider(QueryModel queryModel)
        {
            _queryModel = queryModel;
        }

        public TuppleWareItemsProvider(QueryModel queryModel, List<Dictionary<AttributeModel, object>> data)
        {
            _queryModel = queryModel;
           // _data = data;
        }

        public int FetchCount()
        {
            //_fetchCount = QueryEngine.ComputeQueryResult(_queryModel, _data).Count;
            return _fetchCount;
        }

        public IList<QueryResultItemModel> FetchRange(int startIndex, int pageCount, out int overallCount)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Debug.WriteLine("Start Get Page : " + startIndex + " " + pageCount);
            //System.Threading.Tasks.Task.Delay(500).Wait();

            IList<QueryResultItemModel> returnList = null;//returnList = QueryEngine.ComputeQueryResult(_queryModel, _data).Skip(startIndex).Take(pageCount).ToList();

            // reset selections
            foreach (var queryResultItemModel in returnList)
            {
                FilterModel filterQueryResultItemModel = new FilterModel(queryResultItemModel);
                foreach (var fi in _queryModel.FilterModels.ToArray())
                {
                    if (fi != null)
                    {
                        if (fi.Equals(filterQueryResultItemModel))
                        {
                            queryResultItemModel.IsSelected = true;
                        }
                    }
                }
            }

            overallCount = _fetchCount;

            Debug.WriteLine("End Get Page : " + sw.ElapsedMilliseconds + " millis");
            return returnList;
        }
    }
}
