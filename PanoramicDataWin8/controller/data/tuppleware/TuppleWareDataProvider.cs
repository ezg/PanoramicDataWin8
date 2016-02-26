using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
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
        private QueryModel _queryModelOriginal = null;

        public TuppleWareDataProvider(QueryModel queryModelClone, QueryModel queryModelOriginal, TuppleWareOriginModel originModel)
        {
            QueryModelClone = queryModelClone;
            _originModel = originModel;
            _queryModelOriginal = queryModelOriginal;
            IsInitialized = false;
        }

        public override async Task StartSampling()
        {
            _nrProcessedSamples = 0;

            var inputModels = QueryModelClone.InputOperationModels.Select(iom => iom.InputModel as InputFieldModel).ToList();

            _uuid = _originModel.DatasetConfiguration.BaseUUID;
            List<FilterModel> filterModels = new List<FilterModel>();
            string select = FilterModel.GetFilterModelsRecursive(QueryModelClone, new List<QueryModel>(), filterModels, true);
            if (select != "")
            {
                SelectCommand selectCommand = new SelectCommand();
                _uuid = (await selectCommand.Select(_originModel, _uuid, select))["uuid"].Value<string>();
            }
            
            ProjectCommand projectCommand = new ProjectCommand();
            _uuid = (await projectCommand.Project(_originModel, _uuid, inputModels))["uuid"].Value<string>();

            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _queryModelOriginal.GenerateCodeUuids = new List<string>() {_uuid};
            });
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
                       // _nrProcessedSamples = GetNrTotalSamples();
                    }
                    return new DataPage() {DataRows = returnList, IsEmpty = false};
                }
            }
            else
            {
                return null;
            }
        }

        private async Task<RawDataPage> getDataFromWeb(int page, int sampleSize)
        {
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
