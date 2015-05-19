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
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.FileProperties;
using System.Globalization;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.data.sim;

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
                
                List<Dictionary<InputFieldModel, object>> data = await getDataFromFile(sampleSize);
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

        public ResultItemValueModel GetResultItemValueModel(InputOperationModel inputOperationModel, Dictionary<InputFieldModel, object> valueDict)
        {
            ResultItemValueModel valueModel = new ResultItemValueModel();
            if (valueDict[inputOperationModel.InputModel] == null)
            {
                valueModel.Value = null;
                valueModel.StringValue = "";
                valueModel.ShortStringValue = "";
            }
            else
            {
                double d = 0.0;
                valueModel.Value = valueDict[inputOperationModel.InputModel];
                valueModel.StringValue = valueModel.Value.ToString();

                if (double.TryParse(valueModel.Value.ToString(), out d))
                {
                    valueModel.StringValue = valueModel.Value.ToString().Contains(".") ? d.ToString("N") : valueModel.Value.ToString();
                }
                else if (inputOperationModel.InputModel.InputDataType == InputDataTypeConstants.GEOGRAPHY)
                {
                    string toSplit = valueModel.StringValue;
                    if (toSplit.Contains("(") && toSplit.Contains(")"))
                    {
                        toSplit = toSplit.Substring(toSplit.IndexOf("("));
                        toSplit = toSplit.Substring(1, toSplit.IndexOf(")") - 1);
                    }
                    valueModel.ShortStringValue = valueModel.StringValue.Replace("(" + toSplit + ")", "");
                }
                else if (inputOperationModel.InputModel.InputDataType == InputDataTypeConstants.TIME)
                {
                    if (valueModel.Value is DateTime)
                    {
                        valueModel.StringValue = ((DateTime)valueModel.Value).TimeOfDay.ToString();
                        valueModel.ShortStringValue = ((DateTime)valueModel.Value).TimeOfDay.ToString();
                    }
                }
                else if (inputOperationModel.InputModel.InputDataType == InputDataTypeConstants.DATE)
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

        private async Task<List<Dictionary<InputFieldModel, object>>> getDataFromFile(int sampleSize)
        {
            int count = 0;
            string line = await _streamReader.ReadLineAsync();

            List<Dictionary<InputFieldModel, object>> data = new List<Dictionary<InputFieldModel, object>>();

            while (line != null && count < sampleSize)
            {
                Dictionary<InputFieldModel, object> items = new Dictionary<InputFieldModel, object>();
                items[_simOriginModel.IdInputModel] = count;

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
                    if (((InputFieldModel) _simOriginModel.InputModels[i]).InputDataType == InputDataTypeConstants.NVARCHAR)
                    {
                        value = values[i].ToString();
                    }
                    else if (((InputFieldModel) _simOriginModel.InputModels[i]).InputDataType == InputDataTypeConstants.FLOAT)
                    {
                        double d = 0.0;
                        if (double.TryParse(values[i].ToString(), out d))
                        {
                            value = d;
                        }
                    }
                    else if (((InputFieldModel) _simOriginModel.InputModels[i]).InputDataType == InputDataTypeConstants.INT)
                    {
                        int d = 0;
                        if (int.TryParse(values[i].ToString(), out d))
                        {
                            value = d;
                        }
                    }
                    else if (((InputFieldModel) _simOriginModel.InputModels[i]).InputDataType == InputDataTypeConstants.TIME)
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
                    else if (((InputFieldModel) _simOriginModel.InputModels[i]).InputDataType == InputDataTypeConstants.DATE)
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
                    items[(InputFieldModel) _simOriginModel.InputModels[i]] = value;
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
        private  Dictionary<InputFieldModel, object> _entries = null;
        public Dictionary<InputFieldModel, object> Entries
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

        private Dictionary<InputFieldModel, double?> _visualizationValues = new Dictionary<InputFieldModel, double?>();
        public Dictionary<InputFieldModel, double?> VisualizationValues
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
