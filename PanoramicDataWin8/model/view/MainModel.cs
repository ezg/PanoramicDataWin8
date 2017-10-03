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

        private string _hostname;

        private string _apiPath;

        private double _nrOfGroupBins = 10.0;

        private double _nrOfXBins = 10.0;

        private double _nrOfYBins = 10.0;

        private List<OperationTypeModel> _operationTypeModels = new List<OperationTypeModel>();
        private bool _renderFingersAndPen;

        private bool _renderShadingIn1DHistograms;

        private double _sampleSize = 100.0;

        private SchemaModel _schemaModel;

        private bool _showCodeGen;

        private bool _isDarpaSubmissionMode;

        private bool _isDefaultHypothesisEnabled = false;

        private bool _isUnknownUnknownEnabled = false;

        private QueryExecuter _queryExecuter;

        private string _startDataset = "";

        private double _throttleInMillis = 300.0;

        private bool _pollForDecisions = false;

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

        public string Hostname
        {
            get { return _hostname; }
            set { SetProperty(ref _hostname, value); }
        }

        public string APIPath
        {
            get { return _apiPath; }
            set { SetProperty(ref _apiPath, value); }
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

        public bool IsDarpaSubmissionMode
        {
            get { return _isDarpaSubmissionMode; }
            set { SetProperty(ref _isDarpaSubmissionMode, value); }
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

        public bool IsDefaultHypothesisEnabled
        {
            get { return _isDefaultHypothesisEnabled; }
            set { SetProperty(ref _isDefaultHypothesisEnabled, value); }
        }

        public bool IsUnknownUnknownEnabled
        {
            get { return _isUnknownUnknownEnabled; }
            set { SetProperty(ref _isUnknownUnknownEnabled, value); }
        }

        public string StartDataset
        {
            get { return _startDataset; }
            set { SetProperty(ref _startDataset, value); }
        }
        
        public bool Verbose
        {
            get { return _verbose; }
            set { SetProperty(ref _verbose, value); }
        }

        public bool PollForDecisions
        {
            get { return _pollForDecisions; }
            set { SetProperty(ref _pollForDecisions, value); }
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
    }
}