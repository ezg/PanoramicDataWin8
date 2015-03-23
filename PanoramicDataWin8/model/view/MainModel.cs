using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using PanoramicData.controller.input;
using PanoramicData.model.data;

namespace PanoramicData.model.view
{
    public class MainModel : BindableBase
    {
        private ObservableCollection<DatasetConfiguration> _datasetConfigurations = new ObservableCollection<DatasetConfiguration>();
        public ObservableCollection<DatasetConfiguration> DatasetConfigurations
        {
            get
            {
                return _datasetConfigurations;
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                this.SetProperty(ref _errorMessage, value);
            }
        }

        private SchemaModel _schemaModel;
        public SchemaModel SchemaModel
        {
            get
            {
                return _schemaModel;
            }
            set
            {
                this.SetProperty(ref _schemaModel, value);
            }
        }

        private double _sampleSize = 100.0;
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

        private double _throttleInMillis = 300.0;
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
    }
}
