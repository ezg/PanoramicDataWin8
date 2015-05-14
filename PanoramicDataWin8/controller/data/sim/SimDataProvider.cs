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
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Serialization;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using PanoramicData.controller.view;
using PanoramicData.controller.data.sim;
using Windows.ApplicationModel;
using Windows.Storage;
using PanoramicData.utils;
using Windows.Storage.FileProperties;
using System.Globalization;
using PanoramicData.model.data.result;

namespace PanoramicDataWin8.controller.data.sim
{
    public class SimDataProvider
    {
        private QueryModel _queryModel = null;
        private SimOriginModel _simOriginModel = null;
        private int _nrProcessedSamples = 0;
        private int _nrSamplesToCheck = -1;
        private StreamReader _streamReader = null;
        private BasicProperties _dataFileProperties = null;

        public bool IsInitialized { get; set; }

        public SimDataProvider(QueryModel queryModel, SimOriginModel simOriginModel, int nrSamplesToCheck = -1)
        {
            _queryModel = queryModel;
            _simOriginModel = simOriginModel;
            _nrSamplesToCheck = nrSamplesToCheck;
            IsInitialized = false;
        }

        public async Task StartSampling()
        {
            var installedLoc = Package.Current.InstalledLocation;

            StorageFile file = null;
            if (_simOriginModel.DatasetConfiguration.DataFile.StartsWith("Assets"))
            {
                file = await StorageFile.GetFileFromPathAsync(installedLoc.Path + "\\" + _simOriginModel.DatasetConfiguration.DataFile);
            }
            else
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(_simOriginModel.DatasetConfiguration.DataFile);
            }
            _dataFileProperties = await file.GetBasicPropertiesAsync();
            _streamReader = new StreamReader(await file.OpenStreamForReadAsync());

            _nrProcessedSamples = 0;
        }

        public async Task<List<DataRow>> GetSampleDataRows(int sampleSize)
        {
            if (_nrProcessedSamples < GetNrTotalSamples())
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                
                List<Dictionary<AttributeModel, object>> data = await getDataFromFile(sampleSize);
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

        public int GetNrTotalSamples()
        {
            if (_nrSamplesToCheck == -1)
            {
                return _simOriginModel.DatasetConfiguration.NrOfRecords;
            }
            else
            {
                return _nrSamplesToCheck;
            }
        }        

        public double Progress()
        {
            return Math.Min(1.0, (double)_nrProcessedSamples / (double)GetNrTotalSamples());
        }

        public ResultItemValueModel GetResultItemValueModel(AttributeOperationModel attributeOperationModel, Dictionary<AttributeModel, object> valueDict)
        {
            ResultItemValueModel valueModel = new ResultItemValueModel();
            if (valueDict[attributeOperationModel.AttributeModel] == null)
            {
                valueModel.Value = null;
                valueModel.StringValue = "";
                valueModel.ShortStringValue = "";
            }
            else
            {
                double d = 0.0;
                valueModel.Value = valueDict[attributeOperationModel.AttributeModel];
                valueModel.StringValue = valueModel.Value.ToString();

                if (double.TryParse(valueModel.Value.ToString(), out d))
                {
                    valueModel.StringValue = valueModel.Value.ToString().Contains(".") ? d.ToString("N") : valueModel.Value.ToString();
                }
                else if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.GEOGRAPHY)
                {
                    string toSplit = valueModel.StringValue;
                    if (toSplit.Contains("(") && toSplit.Contains(")"))
                    {
                        toSplit = toSplit.Substring(toSplit.IndexOf("("));
                        toSplit = toSplit.Substring(1, toSplit.IndexOf(")") - 1);
                    }
                    valueModel.ShortStringValue = valueModel.StringValue.Replace("(" + toSplit + ")", "");
                }
                else if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.TIME)
                {
                    if (valueModel.Value is DateTime)
                    {
                        valueModel.StringValue = ((DateTime)valueModel.Value).TimeOfDay.ToString();
                        valueModel.ShortStringValue = ((DateTime)valueModel.Value).TimeOfDay.ToString();
                    }
                }
                else if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.DATE)
                {
                    if (valueModel.Value is DateTime)
                    {
                        valueModel.StringValue = ((DateTime)valueModel.Value).ToString("MM/dd/yyyy");
                        valueModel.ShortStringValue = ((DateTime)valueModel.Value).ToString("MM/dd/yyyy");
                    }
                }
                valueModel.ShortStringValue = valueModel.StringValue.TrimTo(300);
            }
            return valueModel;
        }

        private async Task<List<Dictionary<AttributeModel, object>>> getDataFromFile(int sampleSize)
        {
            int count = 0;
            string line = await _streamReader.ReadLineAsync();

            List<Dictionary<AttributeModel, object>> data = new List<Dictionary<AttributeModel, object>>();

            while (line != null && count < sampleSize)
            {
                Dictionary<AttributeModel, object> items = new Dictionary<AttributeModel, object>();
                items[_simOriginModel.IdAttributeModel] = count;

                List<string> values = null;
                if (_simOriginModel.DatasetConfiguration.UseQuoteParsing)
                {
                    values = CSVParser.CSVLineSplit(line);
                }
                else
                {
                    values = line.Split(new char[] { ',' }).ToList();
                }
                for (int i = 0; i < values.Count; i++)
                {
                    object value = null;
                    if (_simOriginModel.AttributeModels[i].AttributeDataType == AttributeDataTypeConstants.NVARCHAR)
                    {
                        value = values[i].ToString();
                    }
                    else if (_simOriginModel.AttributeModels[i].AttributeDataType == AttributeDataTypeConstants.FLOAT)
                    {
                        double d = 0.0;
                        if (double.TryParse(values[i].ToString(), out d))
                        {
                            value = d;
                        }
                    }
                    else if (_simOriginModel.AttributeModels[i].AttributeDataType == AttributeDataTypeConstants.INT)
                    {
                        int d = 0;
                        if (int.TryParse(values[i].ToString(), out d))
                        {
                            value = d;
                        }
                    }
                    else if (_simOriginModel.AttributeModels[i].AttributeDataType == AttributeDataTypeConstants.TIME)
                    {
                        DateTime timeStamp = DateTime.Now;
                        if (DateTime.TryParseExact(values[i].ToString(), new string[] {"HH:mm:ss","mm:ss","mm:ss.f","m:ss"} , null, System.Globalization.DateTimeStyles.None, out timeStamp))
                        {
                            value = timeStamp;
                        }
                        else
                        {
                            value = null;
                        }
                    }
                    else if (_simOriginModel.AttributeModels[i].AttributeDataType == AttributeDataTypeConstants.DATE)
                    {
                        DateTime date = DateTime.Now;
                        if (DateTime.TryParseExact(values[i].ToString(), new string[] { "MM/dd/yyyy HH:mm:ss", "M/d/yyyy" }, null, System.Globalization.DateTimeStyles.None, out date))
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
                    items[_simOriginModel.AttributeModels[i]] = value;
                }
                data.Add(items);
                line = await _streamReader.ReadLineAsync();
                count++;
            }

            return data;
        }
    }

    public class DataRow
    {
        private  Dictionary<AttributeModel, object> _entries = null;
        public Dictionary<AttributeModel, object> Entries
        {
            get
            {
                return _entries;
            }
            set
            {
                _entries = value;
            }
        }

        private Dictionary<AttributeModel, double?> _visualizationValues = new Dictionary<AttributeModel, double?>();
        public Dictionary<AttributeModel, double?> VisualizationValues
        {
            get
            {
                return _visualizationValues;
            }
            set
            {
                _visualizationValues = value;
            }
        }
    }
}
