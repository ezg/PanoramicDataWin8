using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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

        public TuppleWareDataProvider(QueryModel queryModelClone, TuppleWareOriginModel originModel)
        {
            QueryModelClone = queryModelClone;
            _originModel = originModel;
            IsInitialized = false;
        }

        public override async Task StartSampling()
        {
            _nrProcessedSamples = 0;
            await Task.Delay(0);
        }

        public override async Task<List<DataRow>> GetSampleDataRows(int sampleSize)
        {
            if (_nrProcessedSamples < GetNrTotalSamples())
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int page = (int) _nrProcessedSamples / sampleSize;
                List<Dictionary<InputFieldModel, object>> data = await getDataFromWeb(page, sampleSize);
                List<DataRow> returnList = data.Select(d => new DataRow() { Entries = d }).ToList();
                _nrProcessedSamples += sampleSize;

                if (MainViewController.Instance.MainModel.Verbose)
                {
                    Debug.WriteLine("From File Time: " + sw.ElapsedMilliseconds);
                }
                return returnList;
            }
            else
            {
                return null;
            }
        }


        private void getFilterModelsRecursive(QueryModel queryModel, List<QueryModel> visitedQueryModels, List<FilterModel> filterModels, bool isFirst)
        {
            visitedQueryModels.Add(queryModel);
            if (!isFirst)
            {
                filterModels.AddRange(queryModel.FilterModels);
            }

            foreach (var linkModel in queryModel.LinkModels)
            {
                if (linkModel.FromQueryModel != null && !visitedQueryModels.Contains(linkModel.FromQueryModel))
                {
                    getFilterModelsRecursive(linkModel.FromQueryModel, visitedQueryModels, filterModels, false);
                }
                /*if (linkModel.ToQueryModel != null && !visitedQueryModels.Contains(linkModel.ToQueryModel))
                {
                    getFilterModelsRecursive(linkModel.ToQueryModel, visitedQueryModels, filterModels, false);
                }*/
            }
        }


        private async Task<List<Dictionary<InputFieldModel, object>>> getDataFromWeb(int page, int sampleSize)
        {
            int count = 0;
            List<FilterModel> filterModels = new List<FilterModel>();
            getFilterModelsRecursive(QueryModelClone, new List<QueryModel>(), filterModels, true);

            var inputModels = QueryModelClone.InputOperationModels.Select(iom => iom.InputModel as InputFieldModel).ToList();
            JArray lines = await TuppleWareGateway.GetData(_originModel, inputModels, page, sampleSize);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<Dictionary<InputFieldModel, object>> data = new List<Dictionary<InputFieldModel, object>>();

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
                        if (DateTime.TryParseExact(line[inputModel.Name].ToString(), new string[] { "HH:mm:ss", "mm:ss", "mm:ss.f", "m:ss" }, null, System.Globalization.DateTimeStyles.None, out timeStamp))
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
                        if (DateTime.TryParseExact(line[inputModel.Name].ToString(), new string[] { "MM/dd/yyyy HH:mm:ss", "M/d/yyyy" }, null, System.Globalization.DateTimeStyles.None, out date))
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
            if (MainViewController.Instance.MainModel.Verbose)
            {
                Debug.WriteLine("TuppleWare Parse Time: " + sw.ElapsedMilliseconds);
            }
            return data;
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
}
