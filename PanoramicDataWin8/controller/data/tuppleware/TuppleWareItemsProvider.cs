using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.data.tuppleware;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.controller.data.tuppleware
{
    public class TuppleWareItemsProvider : IItemsProvider<ResultItemModel>
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
            else if (_queryModel.JobType == JobType.Kmeans && _queryModel.GetUsageInputOperationModel(InputUsage.JobInput).Count >= 2)
            {
                _fetchCount = _queryModel.KmeansClusters + _queryModel.KmeansNrSamples;
            }
            return _fetchCount;
        }

        public async Task<IList<ResultItemModel>> FetchPage(int startIndex, int pageCount)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Debug.WriteLine("Start Get Page : " + startIndex + " " + pageCount);

            TuppleWareGateway web = new TuppleWareGateway();
            IList<ResultItemModel> returnList = new List<ResultItemModel>();

            /*if (_queryModel.JobType == JobType.DB)
            {
                dynamic responseObj = await getDbWebResponse();
                foreach (var sample in responseObj["samples"])
                {
                    ResultItemModel item = new ResultItemModel();
                    foreach (var InputOperationModel in _queryModel.GetUsageInputOperationModel(InputUsage.X))
                    {
                        ResultItemValueModel valueModel = fromRaw(
                            InputOperationModel.InputFieldModel.InputDataType,
                            sample[(InputOperationModel.InputFieldModel as TuppleWareInputModel).Index]);

                        if (!item.AttributeValues.ContainsKey(InputOperationModel))
                        {
                            item.AttributeValues.Add(InputOperationModel, valueModel);
                        }
                    }
                    returnList.Add(item);
                }
            }
            else if (_queryModel.JobType == JobType.Kmeans && _queryModel.GetUsageInputOperationModel(InputUsage.JobInput).Count >= 2)
            {
                dynamic responseObj = await getKmeansWebResponse();
                // clusters first
                foreach (var k in responseObj["k"])
                {
                    ResultItemModel item = new ResultItemModel();
                    ResultItemValueModel valueModel = null;

                    valueModel = fromRaw(InputDataTypeConstants.FLOAT, k[0]);
                    item.JobResultValues.Add(JobResult.ClusterX, valueModel);

                    valueModel = fromRaw(InputDataTypeConstants.FLOAT, k[1]);
                    item.JobResultValues.Add(JobResult.ClusterY, valueModel);

                    returnList.Add(item);
                }

                // then samples
                foreach (var sample in responseObj["samples"])
                {
                    ResultItemModel item = new ResultItemModel();
                    ResultItemValueModel valueModel = null;

                    valueModel = fromRaw(InputDataTypeConstants.FLOAT, sample[0]);
                    item.JobResultValues.Add(JobResult.SampleX, valueModel);

                    valueModel = fromRaw(InputDataTypeConstants.FLOAT, sample[1]);
                    item.JobResultValues.Add(JobResult.SampleY, valueModel);

                    returnList.Add(item);
                }
            }*/

            Debug.WriteLine("End Get Page : " + sw.ElapsedMilliseconds + " millis");
            return returnList;
        }

        private async Task<object> getDbWebResponse()
        {
           /* TuppleWareGateway web = new TuppleWareGateway();
            string responseStr = await web.Request((_queryModel.SchemaModel as TuppleWareSchemaModel).RootOriginModel.DatasetConfiguration.EndPoint,
                "sample/?q={\"file_id\":" + (_queryModel.SchemaModel as TuppleWareSchemaModel).RootOriginModel.FileId + ",\"num_samples\":100}");

            dynamic responseObj = JObject.Parse(responseStr);
            return responseObj;*/
            return null;
        }

        private async Task<object> getKmeansWebResponse()
        {
            /*TuppleWareGateway web = new TuppleWareGateway();
            string responseStr = await web.Request((_queryModel.SchemaModel as TuppleWareSchemaModel).RootOriginModel.DatasetConfiguration.EndPoint,
                "job/?q={\"job\":\"kmeans\"," +
                "\"file_id\":" + (_queryModel.SchemaModel as TuppleWareSchemaModel).RootOriginModel.FileId + "," +
                "\"attrs\":" + "[" + string.Join(",", _queryModel.GetUsageInputOperationModel(InputUsage.JobInput).Select(aom => (aom.InputFieldModel as TuppleWareInputModel).Index)) + "]" + "," +
                "\"k\":" + _queryModel.KmeansClusters + "," +
                "\"samples\":" + _queryModel.KmeansNrSamples + "}"
                );

            dynamic responseObj = JObject.Parse(responseStr);*/
            return null;
        }

        private static ResultItemValueModel fromRaw(string dataType, object value)
        {
            ResultItemValueModel valueModel = new ResultItemValueModel();

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
                    if (dataType == InputDataTypeConstants.BIT)
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

                if (dataType == InputDataTypeConstants.GEOGRAPHY)
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