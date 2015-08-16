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
    public class SimDataProvider : DataProvider
    {
        private SimOriginModel _simOriginModel = null;
        private int _nrProcessedSamples = 0;
        private StreamReader _streamReader = null;

        public SimDataProvider(QueryModel queryModelClone, SimOriginModel simOriginModel)
        {
            QueryModelClone = queryModelClone;
            _simOriginModel = simOriginModel;
            IsInitialized = false;
        }

        public override async Task StartSampling()
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
            _streamReader = new StreamReader(await file.OpenStreamForReadAsync());

            _nrProcessedSamples = 0;
        }

        public override async Task<DataPage> GetSampleDataRows(int sampleSize)
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
                return new DataPage() {DataRow = returnList, IsEmpty = false};
            }
            else
            {
                return null;
            }
        }

        public override int GetNrTotalSamples()
        {
            if (NrSamplesToCheck == -1)
            {
                return _simOriginModel.DatasetConfiguration.NrOfRecords;
            }
            else
            {
                return NrSamplesToCheck;
            }
        }

        public override double Progress()
        {
            return Math.Min(1.0, (double)_nrProcessedSamples / (double)GetNrTotalSamples());
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
}
