﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.tuppleware;

namespace PanoramicDataWin8.model.view
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

        private List<TaskModel> _taskModels = null;
        public List<TaskModel> TaskModels
        {
            get
            {
                return _taskModels;
            }
            set
            {
                this.SetProperty(ref _taskModels, value);
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

        private string _ip;
        public string Ip
        {
            get
            {
                return _ip;
            }
            set
            {
                this.SetProperty(ref _ip, value);
            }
        }

        private string _participant = "";
        public string Participant
        {
            get
            {
                return _participant;
            }
            set
            {
                this.SetProperty(ref _participant, value);
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

        private double _nrOfXBins = 10.0;
        public double NrOfXBins
        {
            get
            {
                return _nrOfXBins;
            }
            set
            {
                this.SetProperty(ref _nrOfXBins, value);
            }
        }

        private double _nrOfYBins = 10.0;
        public double NrOfYBins
        {
            get
            {
                return _nrOfYBins;
            }
            set
            {
                this.SetProperty(ref _nrOfYBins, value);
            }
        }

        private double _nrOfGroupBins = 10.0;
        public double NrOfGroupBins
        {
            get
            {
                return _nrOfGroupBins;
            }
            set
            {
                this.SetProperty(ref _nrOfGroupBins, value);
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

        private bool _verbose = false;
        public bool Verbose
        {
            get
            {
                return _verbose;
            }
            set
            {
                this.SetProperty(ref _verbose, value);
            }
        }
        private bool _renderFingersAndPen = false;
        public bool RenderFingersAndPen
        {
            get
            {
                return _renderFingersAndPen;
            }
            set
            {
                this.SetProperty(ref _renderFingersAndPen, value);
            }
        }

        private bool _showCodeGen = false;
        public bool ShowCodeGen
        {
            get
            {
                return _showCodeGen;
            }
            set
            {
                this.SetProperty(ref _showCodeGen, value);
            }
        }

        private bool _renderShadingIn1DHistograms = false;
        public bool RenderShadingIn1DHistograms
        {
            get
            {
                return _renderShadingIn1DHistograms;
            }
            set
            {
                this.SetProperty(ref _renderShadingIn1DHistograms, value);
            }
        }

        private GraphRenderOptions _graphRenderOption = GraphRenderOptions.Cell;
        public GraphRenderOptions GraphRenderOption
        {
            get
            {
                return _graphRenderOption;
            }
            set
            {
                this.SetProperty(ref _graphRenderOption, value);
            }
        }
    }

    public enum GraphRenderOptions { Grid, Cell}
}
