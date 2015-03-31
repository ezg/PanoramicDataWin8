using Microsoft.Practices.Prism.Mvvm;
using PanoramicData.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Popups;

namespace PanoramicData.controller.input
{
    public class DatasetConfiguration :BindableBase
    {
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

        private List<string> _attributeNames = new List<string>();
        public List<string> AttributeNames
        {
            get
            {
                return _attributeNames;
            }
            set
            {
                this.SetProperty(ref _attributeNames, value);
            }
        }

        private List<string> _attributeDataTypes = new List<string>();
        public List<string> AttributeDataTypes
        {
            get
            {
                return _attributeDataTypes;
            }
            set
            {
                this.SetProperty(ref _attributeDataTypes, value);
            }
        }

        private List<string> _attributeVisualizationTypes = new List<string>();
        public List<string> AttributeVisualizationTypes
        {
            get
            {
                return _attributeVisualizationTypes;
            }
            set
            {
                this.SetProperty(ref _attributeVisualizationTypes, value);
            }
        }

        private List<bool> _attributeIsDisplayed = new List<bool>();
        public List<bool> AttributeIsDisplayed
        {
            get
            {
                return _attributeIsDisplayed;
            }
            set
            {
                this.SetProperty(ref _attributeIsDisplayed, value);
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
                        config.AttributeNames = CSVParser.CSVLineSplit(parts[1].Trim());
                    }
                    else if (parts[0] == "DataTypes")
                    {
                        config.AttributeDataTypes = CSVParser.CSVLineSplit(parts[1].Trim());
                    }
                    else if (parts[0] == "VisualizationTypes")
                    {
                        config.AttributeVisualizationTypes = CSVParser.CSVLineSplit(parts[1].Trim());
                    }
                    else if (parts[0] == "IsDisplayed")
                    {
                        config.AttributeIsDisplayed = CSVParser.CSVLineSplit(parts[1].Trim()).Select(s => s.ToLower() == "true").ToList();
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
