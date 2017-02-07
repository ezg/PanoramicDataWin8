using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.controller.data;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class MainModel : ExtendedBindableBase
    {
        private string _backend = "";

        private string _errorMessage;

        private GraphRenderOptions _graphRenderOption = GraphRenderOptions.Cell;

        private string _ip;

        private double _nrOfGroupBins = 10.0;

        private double _nrOfXBins = 10.0;

        private double _nrOfYBins = 10.0;

        private List<OperationTypeModel> _operationTypeModels = new List<OperationTypeModel>();
        private bool _renderFingersAndPen;

        private bool _renderShadingIn1DHistograms;

        private double _sampleSize = 100.0;

        private SchemaModel _schemaModel;

        private bool _showCodeGen;

        private bool _isDefaultHypothesisEnabled = false;

        private QueryExecuter _queryExecuter;

        private string _startDataset = "";

        private double _throttleInMillis = 300.0;

        private bool _verbose;

        public ObservableCollection<DatasetConfiguration> DatasetConfigurations { get; } = new ObservableCollection<DatasetConfiguration>();

        public List<OperationTypeModel> OperationTypeModels
        {
            get { return _operationTypeModels; }
            set { SetProperty(ref _operationTypeModels, value); }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetProperty(ref _errorMessage, value); }
        }

        public string Ip
        {
            get { return _ip; }
            set { SetProperty(ref _ip, value); }
        }

        public QueryExecuter QueryExecuter
        {
            get { return _queryExecuter; }
            set { SetProperty(ref _queryExecuter, value); }
        }

        public SchemaModel SchemaModel
        {
            get { return _schemaModel; }
            set { SetProperty(ref _schemaModel, value); }
        }

        public double SampleSize
        {
            get { return _sampleSize; }
            set { SetProperty(ref _sampleSize, value); }
        }

        public double NrOfXBins
        {
            get { return _nrOfXBins; }
            set { SetProperty(ref _nrOfXBins, value); }
        }

        public double NrOfYBins
        {
            get { return _nrOfYBins; }
            set { SetProperty(ref _nrOfYBins, value); }
        }

        public double NrOfGroupBins
        {
            get { return _nrOfGroupBins; }
            set { SetProperty(ref _nrOfGroupBins, value); }
        }

        public double ThrottleInMillis
        {
            get { return _throttleInMillis; }
            set { SetProperty(ref _throttleInMillis, value); }
        }

        public bool IsDefaultHypothesisEnabled
        {
            get { return _isDefaultHypothesisEnabled; }
            set { SetProperty(ref _isDefaultHypothesisEnabled, value); }
        }

        public string StartDataset
        {
            get { return _startDataset; }
            set { SetProperty(ref _startDataset, value); }
        }

        public string Backend
        {
            get { return _backend; }
            set { SetProperty(ref _backend, value); }
        }

        public bool Verbose
        {
            get { return _verbose; }
            set { SetProperty(ref _verbose, value); }
        }

        public bool RenderFingersAndPen
        {
            get { return _renderFingersAndPen; }
            set { SetProperty(ref _renderFingersAndPen, value); }
        }

        public bool ShowCodeGen
        {
            get { return _showCodeGen; }
            set { SetProperty(ref _showCodeGen, value); }
        }

        public bool RenderShadingIn1DHistograms
        {
            get { return _renderShadingIn1DHistograms; }
            set { SetProperty(ref _renderShadingIn1DHistograms, value); }
        }

        public GraphRenderOptions GraphRenderOption
        {
            get { return _graphRenderOption; }
            set { SetProperty(ref _graphRenderOption, value); }
        }
    }
}