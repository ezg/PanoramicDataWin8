using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IDEA_common.catalog;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.controller.input
{
    public class DatasetConfiguration :BindableBase
    {
        private Schema _schema;
        public Schema Schema
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
    }
}
