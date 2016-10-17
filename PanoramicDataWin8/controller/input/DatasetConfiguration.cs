using System.Collections.Generic;
using IDEA_common.catalog;
using Microsoft.Practices.Prism.Mvvm;

namespace PanoramicDataWin8.controller.input
{
    public class DatasetConfiguration : BindableBase
    {
        private string _backend;

        private List<string> _inputFieldDataTypes = new List<string>();

        private List<bool> _inputFieldIsDisplayed = new List<bool>();

        private List<string> _inputFieldNames = new List<string>();

        private List<string> _inputFieldVisualizationTypes = new List<string>();

        private int _nrOfRecords;

        private double _sampleSize = 1;
        private Schema _schema;

        private double _throttleInMillis;

        public Schema Schema
        {
            get { return _schema; }
            set { SetProperty(ref _schema, value); }
        }

        public double SampleSize
        {
            get { return _sampleSize; }
            set { SetProperty(ref _sampleSize, value); }
        }

        public int NrOfRecords
        {
            get { return _nrOfRecords; }
            set { SetProperty(ref _nrOfRecords, value); }
        }

        public double ThrottleInMillis
        {
            get { return _throttleInMillis; }
            set { SetProperty(ref _throttleInMillis, value); }
        }

        public string Backend
        {
            get { return _backend; }
            set { SetProperty(ref _backend, value); }
        }

        public List<string> InputFieldNames
        {
            get { return _inputFieldNames; }
            set { SetProperty(ref _inputFieldNames, value); }
        }

        public List<string> InputFieldDataTypes
        {
            get { return _inputFieldDataTypes; }
            set { SetProperty(ref _inputFieldDataTypes, value); }
        }

        public List<string> InputFieldVisualizationTypes
        {
            get { return _inputFieldVisualizationTypes; }
            set { SetProperty(ref _inputFieldVisualizationTypes, value); }
        }

        public List<bool> InputFieldIsDisplayed
        {
            get { return _inputFieldIsDisplayed; }
            set { SetProperty(ref _inputFieldIsDisplayed, value); }
        }
    }
}