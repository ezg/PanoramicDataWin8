using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.data.tuppleware.json;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.controller.input
{
    public class DatasetConfiguration :BindableBase
    {
        private long _baseUUID;
        public long BaseUUID
        {
            get
            {
                return _baseUUID;
            }
            set
            {
                this.SetProperty(ref _baseUUID, value);
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                this.SetProperty(ref _name, value);
            }
        }

        private JToken _schemaJson;

        public JToken SchemaJson
        {
            get
            {
                return _schemaJson;
            }
            set
            {
                this.SetProperty(ref _schemaJson, value);
            }
        }

        private string _endPoint;
        public string EndPoint
        {
            get
            {
                return _endPoint;
            }
            set
            {
                this.SetProperty(ref _endPoint, value);
            }
        }

        private double _sampleSize = 1;
        public double SampleSize
        {
            get
            {
                return _sampleSize;
            }
            set
            {
                this.SetProperty(ref _sampleSize, value);
            }
        }

        private int _nrOfRecords = 0;
        public int NrOfRecords
        {
            get
            {
                return _nrOfRecords;
            }
            set
            {
                this.SetProperty(ref _nrOfRecords, value);
            }
        }

        private double _throttleInMillis = 0;
        public double ThrottleInMillis
        {
            get
            {
                return _throttleInMillis;
            }
            set
            {
                this.SetProperty(ref _throttleInMillis, value);
            }
        }

        private string _schema;
        public string Schema
        {
            get
            {
                return _schema;
            }
            set
            {
                this.SetProperty(ref _schema, value);
            }
        }

        private string _table;
        public string Table
        {
            get
            {
                return _table;
            }
            set
            {
                this.SetProperty(ref _table, value);
            }
        }

        private string _backend;
        public string Backend
        {
            get
            {
                return _backend;
            }
            set
            {
                this.SetProperty(ref _backend, value);
            }
        }

        private bool _useQuoteParsing = true;
        public bool UseQuoteParsing
        {
            get
            {
                return _useQuoteParsing;
            }
            set
            {
                this.SetProperty(ref _useQuoteParsing, value);
            }
        }

        private string _dataFile;
        public string DataFile
        {
            get
            {
                return _dataFile;
            }
            set
            {
                this.SetProperty(ref _dataFile, value);
            }
        }

        private List<string> _inputFieldNames = new List<string>();
        public List<string> InputFieldNames
        {
            get
            {
                return _inputFieldNames;
            }
            set
            {
                this.SetProperty(ref _inputFieldNames, value);
            }
        }

        private List<string> _inputFieldDataTypes = new List<string>();
        public List<string> InputFieldDataTypes
        {
            get
            {
                return _inputFieldDataTypes;
            }
            set
            {
                this.SetProperty(ref _inputFieldDataTypes, value);
            }
        }

        private List<string> _inputFieldVisualizationTypes = new List<string>();
        public List<string> InputFieldVisualizationTypes
        {
            get
            {
                return _inputFieldVisualizationTypes;
            }
            set
            {
                this.SetProperty(ref _inputFieldVisualizationTypes, value);
            }
        }

        private List<bool> _inputFieldIsDisplayed = new List<bool>();
        public List<bool> InputFieldIsDisplayed
        {
            get
            {
                return _inputFieldIsDisplayed;
            }
            set
            {
                this.SetProperty(ref _inputFieldIsDisplayed, value);
            }
        }

        public static DatasetConfiguration FromContent(string content, string fileName)
        {
            try
            {
                DatasetConfiguration config = new DatasetConfiguration();
                string[] lines = content.Split('\n');

                foreach (var line in lines)
                {
                    if (line.Trim().StartsWith("#") || line.Trim() == "")
                    {
                        continue;
                    }

                    string[] parts = line.Split('=');
                    if (parts[0] == "Schema")
                    {
                        config.Schema = parts[1].Trim();
                    }
                    else if (parts[0] == "Name")
                    {
                        config.Name = parts[1].Trim();
                    }
                    else if (parts[0] == "Table")
                    {
                        config.Table = parts[1].Trim();
                    }
                    else if (parts[0] == "Backend")
                    {
                        config.Backend = parts[1].Trim();
                    }
                    else if (parts[0] == "DataFile")
                    {
                        config.DataFile = parts[1].Trim();
                    }
                    else if (parts[0] == "EndPoint")
                    {
                        config.EndPoint = parts[1].Trim();
                    }
                    else if (parts[0] == "SampleSize")
                    {
                        config.SampleSize = double.Parse(parts[1].Trim());
                    }
                    else if (parts[0] == "NrOfRecords")
                    {
                        config.NrOfRecords = int.Parse(parts[1].Trim());
                    }
                    else if (parts[0] == "ThrottleInMillis")
                    {
                        config.ThrottleInMillis = double.Parse(parts[1].Trim());
                    }
                    else if (parts[0] == "Names")
                    {
                        config.InputFieldNames = CSVParser.CSVLineSplit(parts[1].Trim());
                    }
                    else if (parts[0] == "DataTypes")
                    {
                        config.InputFieldDataTypes = CSVParser.CSVLineSplit(parts[1].Trim());
                    }
                    else if (parts[0] == "VisualizationTypes")
                    {
                        config.InputFieldVisualizationTypes = CSVParser.CSVLineSplit(parts[1].Trim());
                    }
                    else if (parts[0] == "IsDisplayed")
                    {
                        config.InputFieldIsDisplayed = CSVParser.CSVLineSplit(parts[1].Trim()).Select(s => s.ToLower() == "true").ToList();
                    }
                    else if (parts[0] == "UseQuoteParsing")
                    {
                        config.UseQuoteParsing = parts[1].ToLower().Trim() == "true";
                    }
                }

                return config;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not find or parse config file : " + fileName);
                return null;
            }
        }
    }
}
