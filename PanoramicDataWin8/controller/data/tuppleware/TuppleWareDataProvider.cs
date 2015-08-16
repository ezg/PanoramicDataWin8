using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.data.tuppleware.gateway;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.sim;
using PanoramicDataWin8.model.data.tuppleware;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.controller.data.tuppleware
{
    public class TuppleWareDataProvider : DataProvider
    {
        private int _nrProcessedSamples = 0;
        private TuppleWareOriginModel _originModel = null;
        private string _uuid = "";

        public TuppleWareDataProvider(QueryModel queryModelClone, TuppleWareOriginModel originModel)
        {
            QueryModelClone = queryModelClone;
            _originModel = originModel;
            IsInitialized = false;
        }

        public override async Task StartSampling()
        {
            _nrProcessedSamples = 0;

            var inputModels = QueryModelClone.InputOperationModels.Select(iom => iom.InputModel as InputFieldModel).ToList();

            ProjectCommand projectCommand = new ProjectCommand();
            _uuid = (await projectCommand.Project(_originModel, _originModel.DatasetConfiguration.BaseUUID, inputModels))["uuid"].Value<string>();
        }

        public override async Task<DataPage> GetSampleDataRows(int sampleSize)
        {
            if (_nrProcessedSamples < GetNrTotalSamples())
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int page = (int) _nrProcessedSamples / sampleSize;
                RawDataPage rawDataPage = await getDataFromWeb(page, sampleSize);
                if (rawDataPage.IsEmpty)
                {
                    return new DataPage() {IsEmpty = true};
                }
                else
                {
                    List<DataRow> returnList = rawDataPage.Data.Select(d => new DataRow() { Entries = d }).ToList();
                    _nrProcessedSamples += sampleSize;

                    if (MainViewController.Instance.MainModel.Verbose)
                    {
                        Debug.WriteLine("From File Time: " + sw.ElapsedMilliseconds);
                    }

                    if (rawDataPage.Data.Count == 0)
                    {
                        _nrProcessedSamples = GetNrTotalSamples();
                    }
                    return new DataPage() {DataRow = returnList, IsEmpty = false};
                }
            }
            else
            {
                return null;
            }
        }


        private string getFilterModelsRecursive(QueryModel queryModel, List<QueryModel> visitedQueryModels, List<FilterModel> filterModels, bool isFirst)
        {
            string ret = "";
            visitedQueryModels.Add(queryModel);
            if (!isFirst && queryModel.FilterModels.Count > 0)
            {
                filterModels.AddRange(queryModel.FilterModels);
                ret = "(" + string.Join(" or ", queryModel.FilterModels.Select(fm => fm.ToPythonString())) + ")";
            }


            List<string> children = new List<string>();
            foreach (var linkModel in queryModel.LinkModels)
            {
                if (linkModel.FromQueryModel != null && !visitedQueryModels.Contains(linkModel.FromQueryModel))
                {
                    var child = getFilterModelsRecursive(linkModel.FromQueryModel, visitedQueryModels, filterModels, false);
                    if (child != "")
                    {
                        children.Add(child);
                    }
                }
            }

            string childrenJoined = string.Join(queryModel.FilteringOperation.ToString().ToLower(), children);
            if (children.Count > 0)
            {
                if (ret != "")
                {
                    ret = "(" + ret + " and " + childrenJoined + ")";
                }
                else
                {
                    ret = "(" + childrenJoined + ")";
                }
            }

            return ret;
        }


        private async Task<RawDataPage> getDataFromWeb(int page, int sampleSize)
        {
            int count = 0;
            List<FilterModel> filterModels = new List<FilterModel>();
            string select = getFilterModelsRecursive(QueryModelClone, new List<QueryModel>(), filterModels, true);

            var inputModels = QueryModelClone.InputOperationModels.Select(iom => iom.InputModel as InputFieldModel).ToList();

            LookupCommand lookupCommand = new LookupCommand();
            JToken jToken = await lookupCommand.Lookup(_originModel, _uuid, page, sampleSize) as JToken;
            if (jToken is JObject && jToken["empty"].Value<bool>())
            {
                return new RawDataPage() {IsEmpty = true};
            }
            else
            {
                JArray lines = (JArray) jToken;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                List<Dictionary<InputFieldModel, object>> data = new List<Dictionary<InputFieldModel, object>>();

                if (lines != null)
                {
                    foreach (var line in lines)
                    {
                        Dictionary<InputFieldModel, object> items = new Dictionary<InputFieldModel, object>();

                        foreach (var inputModel in inputModels)
                        {
                            object value = null;
                            if (inputModel.InputDataType == InputDataTypeConstants.NVARCHAR)
                            {
                                value = line[inputModel.Name].ToString();
                            }
                            else if (inputModel.InputDataType == InputDataTypeConstants.FLOAT)
                            {
                                double d = 0.0;
                                if (double.TryParse(line[inputModel.Name].ToString(), out d))
                                {
                                    value = d;
                                }
                            }
                            else if (inputModel.InputDataType == InputDataTypeConstants.INT)
                            {
                                int d = 0;
                                if (int.TryParse(line[inputModel.Name].ToString(), out d))
                                {
                                    value = d;
                                }
                            }
                            else if (inputModel.InputDataType == InputDataTypeConstants.TIME)
                            {
                                DateTime timeStamp = DateTime.Now;
                                if (DateTime.TryParseExact(line[inputModel.Name].ToString(), new string[] {"HH:mm:ss", "mm:ss", "mm:ss.f", "m:ss"}, null, System.Globalization.DateTimeStyles.None,
                                    out timeStamp))
                                {
                                    value = timeStamp;
                                }
                                else
                                {
                                    value = null;
                                }
                            }
                            else if (inputModel.InputDataType == InputDataTypeConstants.DATE)
                            {
                                DateTime date = DateTime.Now;
                                if (DateTime.TryParseExact(line[inputModel.Name].ToString(), new string[] {"MM/dd/yyyy HH:mm:ss", "M/d/yyyy"}, null, System.Globalization.DateTimeStyles.None, out date))
                                {
                                    value = date;
                                }
                                else
                                {
                                    value = null;
                                }
                            }
                            if (value == null || value.ToString().Trim() == "")
                            {
                                value = null;
                            }
                            items[inputModel] = value;
                        }
                        data.Add(items);
                        count++;
                    }
                }
                if (MainViewController.Instance.MainModel.Verbose)
                {
                    Debug.WriteLine("TuppleWare Parse Time: " + sw.ElapsedMilliseconds);
                }

                return new RawDataPage() {IsEmpty = false, Data = data};
            }
        }



        public override double Progress()
        {
            return Math.Min(1.0, (double)_nrProcessedSamples / (double)GetNrTotalSamples());
        }

        public override int GetNrTotalSamples()
        {
            if (NrSamplesToCheck == -1)
            {
                return _originModel.DatasetConfiguration.NrOfRecords;
            }
            else
            {
                return NrSamplesToCheck;
            }
        }
    }

    class RawDataPage
    {
        public List<Dictionary<InputFieldModel, object>> Data { get; set; }
        public bool IsEmpty { get;set; }
    }
}
