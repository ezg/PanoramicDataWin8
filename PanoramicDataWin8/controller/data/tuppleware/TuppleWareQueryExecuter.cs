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
            IItemsProvider<QueryResultItemModel> itemsProvider = new TuppleWareItemsProvider(queryModel.Clone());
            AsyncVirtualizedCollection<QueryResultItemModel> dataValues = new AsyncVirtualizedCollection<QueryResultItemModel>(itemsProvider,
                1000,  // page size
                -1);
            queryModel.QueryResultModel.QueryResultItemModels = dataValues;
        }

        public async void LoadFileDescription(TuppleWareOriginModel tuppleWareOriginModel)
        {
            TuppleWareWebClient web = new TuppleWareWebClient();
            string responseStr = await web.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, "files");
            dynamic responseObj = JObject.Parse(responseStr);

            tuppleWareOriginModel.FileId = responseObj["files"][0]["id"].Value;

            for (int i = 0; i < responseObj["files"][0]["names"].Count; i++)
            {
                TuppleWareAttributeModel attributeModel = new TuppleWareAttributeModel(i, responseObj["files"][0]["names"][i].Value, responseObj["files"][0]["types"][0].Value, "numeric");
                attributeModel.OriginModel = tuppleWareOriginModel;
                tuppleWareOriginModel.AttributeModels.Add(attributeModel);
            }
        }
    }

    public class TuppleWareWebClient
    {
        public async Task<string> Request(string endPoint, string query)
        {
            bool atBrown = false;
            if (atBrown)
            {
                var httpClient = new HttpClient();
                var content = await httpClient.GetStringAsync(endPoint + "/" + query);
                return content;
            }
            else
            {
                if (query.StartsWith("files"))
                {
                    return "{\"files\":[{\"id\":0,\"names\":[\"a0\",\"a1\"],\"types\":[\"float\",\"float\"]}]}";
                }
                else if (query.StartsWith("sample"))
                {
                    return "{\"samples\":[[0.5,0.5],[0.9,0.9],[2.1,2.1]]}";
                }
                else if (query.StartsWith("job"))
                {
                    return "{\"k\":[[0,0],[1,1],[2,2]],\"samples\":[[0.5,0.5],[0.9,0.9],[2.1,2.1],[2.5,2.5]]}";
                }
            }
            return "";
        }
    }

    public class TuppleWareItemsProvider : IItemsProvider<QueryResultItemModel>
    {
        private QueryModel _queryModel = null;
        private int _fetchCount = 0;

        public TuppleWareItemsProvider(QueryModel queryModel)
        {
            _queryModel = queryModel;
        }

        public async Task<int> FetchCount()
        {
            if (_queryModel.JobType == JobType.DB)
            {
                dynamic responseObj = await getDbWebResponse();
                _fetchCount = responseObj["samples"].Count;
            }
            else if (_queryModel.JobType == JobType.Kmeans && _queryModel.GetFunctionAttributeOperationModel(AttributeFunction.JobInput).Count >= 2)
            {
                _fetchCount = _queryModel.KmeansClusters + _queryModel.KmeansNrSamples;
            }
            return _fetchCount;
        }

        public async Task<IList<QueryResultItemModel>> FetchPage(int startIndex, int pageCount)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Debug.WriteLine("Start Get Page : " + startIndex + " " + pageCount);

            TuppleWareWebClient web = new TuppleWareWebClient();
            IList<QueryResultItemModel> returnList = new List<QueryResultItemModel>();

            if (_queryModel.JobType == JobType.DB)
            {
                dynamic responseObj = await getDbWebResponse();
                foreach (var sample in responseObj["samples"])
                {
                    QueryResultItemModel item = new QueryResultItemModel();
                    foreach (var attributeOperationModel in _queryModel.GetFunctionAttributeOperationModel(AttributeFunction.X))
                    {
                        QueryResultItemValueModel valueModel = fromRaw(
                            attributeOperationModel.AttributeModel.AttributeDataType,
                            sample[(attributeOperationModel.AttributeModel as TuppleWareAttributeModel).Index]);

                        if (!item.AttributeValues.ContainsKey(attributeOperationModel))
                        {
                            item.AttributeValues.Add(attributeOperationModel, valueModel);
                        }
                    }
                    returnList.Add(item);
                }
            }
            else if (_queryModel.JobType == JobType.Kmeans && _queryModel.GetFunctionAttributeOperationModel(AttributeFunction.JobInput).Count >= 2)
            {
                dynamic responseObj = await getKmeansWebResponse();
                // clusters first
                foreach (var k in responseObj["k"])
                {
                    QueryResultItemModel item = new QueryResultItemModel();
                    QueryResultItemValueModel valueModel = null;

                    valueModel = fromRaw(AttributeDataTypeConstants.FLOAT, k[0]);
                    item.JobResultValues.Add(JobTypeResult.ClusterX, valueModel);

                    valueModel = fromRaw(AttributeDataTypeConstants.FLOAT, k[1]);
                    item.JobResultValues.Add(JobTypeResult.ClusterY, valueModel);

                    returnList.Add(item);
                }

                // then samples
                foreach (var sample in responseObj["samples"])
                {
                    QueryResultItemModel item = new QueryResultItemModel();
                    QueryResultItemValueModel valueModel = null;

                    valueModel = fromRaw(AttributeDataTypeConstants.FLOAT, sample[0]);
                    item.JobResultValues.Add(JobTypeResult.SampleX, valueModel);

                    valueModel = fromRaw(AttributeDataTypeConstants.FLOAT, sample[1]);
                    item.JobResultValues.Add(JobTypeResult.SampleY, valueModel);

                    returnList.Add(item);
                }
            }

            Debug.WriteLine("End Get Page : " + sw.ElapsedMilliseconds + " millis");
            return returnList;
        }

        private async Task<object> getDbWebResponse()
        {
            TuppleWareWebClient web = new TuppleWareWebClient();
            string responseStr = await web.Request((_queryModel.SchemaModel as TuppleWareSchemaModel).RootOriginModel.DatasetConfiguration.EndPoint,
                    "sample/?q={\"file_id\":" + (_queryModel.SchemaModel as TuppleWareSchemaModel).RootOriginModel.FileId + ",\"num_samples\":100}");

            dynamic responseObj = JObject.Parse(responseStr);
            return responseObj;
        }

        private async Task<object> getKmeansWebResponse()
        {
            TuppleWareWebClient web = new TuppleWareWebClient();
            string responseStr = await web.Request((_queryModel.SchemaModel as TuppleWareSchemaModel).RootOriginModel.DatasetConfiguration.EndPoint,
                    "job/?q={\"job\":\"kmeans\"," +
                        "\"file_id\":" + (_queryModel.SchemaModel as TuppleWareSchemaModel).RootOriginModel.FileId + "," +
                        "\"attrs\":" + "[" + string.Join(",", _queryModel.GetFunctionAttributeOperationModel(AttributeFunction.JobInput).Select(aom => (aom.AttributeModel as TuppleWareAttributeModel).Index)) + "]" + "," +
                        "\"k\":" + _queryModel.KmeansClusters + "," +
                        "\"samples\":" + _queryModel.KmeansNrSamples + "}"
                    );

            dynamic responseObj = JObject.Parse(responseStr);
            return responseObj;
        }

        private static QueryResultItemValueModel fromRaw(string dataType, object value)
        {
            QueryResultItemValueModel valueModel = new QueryResultItemValueModel();

            if (value == null)
            {
                valueModel.Value = null;
                valueModel.StringValue = "";
                valueModel.ShortStringValue = "";
            }
            else
            {
                double d = 0.0;
                valueModel.Value = value;
                if (double.TryParse(value.ToString(), out d))
                {
                    valueModel.StringValue = valueModel.Value.ToString().Contains(".") ? d.ToString("N") : valueModel.Value.ToString();
                    if (dataType == AttributeDataTypeConstants.BIT)
                    {
                        if (d == 1.0)
                        {
                            valueModel.StringValue = "True";
                        }
                        else if (d == 0.0)
                        {
                            valueModel.StringValue = "False";
                        }
                    }
                }
                else
                {
                    valueModel.StringValue = valueModel.Value.ToString();
                    if (valueModel.Value is DateTime)
                    {
                        valueModel.StringValue = ((DateTime)valueModel.Value).ToString();
                    }
                }

                if (dataType == AttributeDataTypeConstants.GEOGRAPHY)
                {

                    string toSplit = valueModel.StringValue;
                    if (toSplit.Contains("(") && toSplit.Contains(")"))
                    {
                        toSplit = toSplit.Substring(toSplit.IndexOf("("));
                        toSplit = toSplit.Substring(1, toSplit.IndexOf(")") - 1);
                    }
                    valueModel.ShortStringValue = valueModel.StringValue.Replace("(" + toSplit + ")", "");
                }
                else
                {
                    valueModel.ShortStringValue = valueModel.StringValue.TrimTo(300);
                }
            }
            return valueModel;
        }
    }
}
