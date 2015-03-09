﻿using Microsoft.Practices.Prism.Mvvm;
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
