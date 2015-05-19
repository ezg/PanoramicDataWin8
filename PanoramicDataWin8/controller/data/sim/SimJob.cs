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
using Windows.System.Threading;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.common;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.data.sim;

namespace PanoramicDataWin8.controller.data.sim
{
    public class SimJob
    {
        public event EventHandler<JobEventArgs> JobUpdate;
        public event EventHandler<EventArgs> JobCompleted;

        private SimDataProvider _simDataProvider = null;
        private bool _isRunning = false;
        private int _sampleSize = 0;
        private bool _isIncremental = false;
        private TimeSpan _throttle = TimeSpan.FromMilliseconds(0);
        private DataBinner _binner = null;
        private DataAggregator _aggregator = new DataAggregator();
        private Object _lock = new Object();
        private List<AxisType> _axisTypes = new List<AxisType>();
        private Stopwatch _stopWatch = new Stopwatch();

        private List<InputOperationModel> _dimensions = new List<InputOperationModel>();
        private List<Dictionary<object, double>> _uniqueValues = new List<Dictionary<object, double>>();

        public QueryModel QueryModel { get; set; }
        public QueryModel QueryModelClone { get; set; }

        public SimJob(QueryModel queryModel, TimeSpan throttle, int sampleSize)
        {
            QueryModel = queryModel;
            _sampleSize = sampleSize;
            _throttle = throttle;
        }

        public void Start()
        {
            _stopWatch.Start();

            QueryModelClone = QueryModel.Clone();

            _isRunning = true;
            int samplesToCheck = -1;

            if (QueryModel.VisualizationType == VisualizationType.table)
            {
                _binner = null;
                samplesToCheck = 1000;
            }
            else
            {
                _dimensions = QueryModel.GetUsageInputOperationModel(InputUsage.X).Concat(
                                 QueryModel.GetUsageInputOperationModel(InputUsage.Y)).Concat(
                                 QueryModel.GetUsageInputOperationModel(InputUsage.Group)).ToList();

                _uniqueValues = _dimensions.Select(d => new Dictionary<object, double>()).ToList();

                _axisTypes = _dimensions.Select(d => QueryModel.GetAxisType(d)).ToList();
                (QueryModel.ResultModel.ResultDescriptionModel as VisualizationResultDescriptionModel).AxisTypes = _axisTypes;

                _isIncremental = _dimensions.Any(aom => aom.AggregateFunction == AggregateFunction.None);

                _binner = new DataBinner()
                {
                    NrOfBins = new double[] { MainViewController.Instance.MainModel.NrOfXBins, MainViewController.Instance.MainModel.NrOfYBins }.Concat(
                                    QueryModel.GetUsageInputOperationModel(InputUsage.Group).Select(qom => MainViewController.Instance.MainModel.NrOfGroupBins)).ToList(),
                    Incremental = _isIncremental,
                    AxisTypes = _axisTypes,
                    IsAxisAggregated = _dimensions.Select(d => d.AggregateFunction != AggregateFunction.None).ToList(),
                    Dimensions = _dimensions.Select(aom => aom.InputModel).ToList()
                };
            }
            _simDataProvider = new SimDataProvider(QueryModelClone, (QueryModel.SchemaModel.OriginModels[0] as SimOriginModel), samplesToCheck);

            Task.Run(() => run());

            //ThreadPool.RunAsync(_ => run(), WorkItemPriority.Low);
        }

        public void Stop()
        {
            lock (_lock)
            {
                _isRunning = false;
            }
        }

        private async void run()
        {
            /*for (long i = 0; i < 100000; i++)
            {
                for (long j = 0; j < 100000; j++)
                {
                    var tt = j * i;
                   
                }
                //await Task.Delay(5);
            }
            await fireCompleted();
            return;*/

            if (!_simDataProvider.IsInitialized)
            {
                await _simDataProvider.StartSampling();
            }

            List<DataRow> dataRows = await _simDataProvider.GetSampleDataRows(_sampleSize);
            List<ResultItemModel> resultItemModels = new List<ResultItemModel>();
            while (dataRows != null && _isRunning)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                if (QueryModelClone.VisualizationType != VisualizationType.table)
                {
                    if (!_isIncremental)
                    {
                        _uniqueValues = _dimensions.Select(d => new Dictionary<object, double>()).ToList();
                    }
                    setVisualizationValues(dataRows);
                    if (_binner != null)
                    {
                        _binner.BinStep(dataRows);
                    }
                    if (_aggregator != null)
                    {
                        _aggregator.AggregateStep(_binner.BinStructure, QueryModelClone, _simDataProvider.Progress());
                    }
                    resultItemModels = convertBinsToResultItemModels(_binner.BinStructure);
                }

                if (_isRunning)
                {
                    ResultDescriptionModel resultDescriptionModel = null;
                    if (_binner != null)
                    {
                        resultDescriptionModel = new VisualizationResultDescriptionModel()
                        {
                            BinRanges = _binner.BinStructure.BinRanges,
                            NullCount = _binner.BinStructure.NullCount,
                            Dimensions = _dimensions,
                            AxisTypes = _axisTypes,
                            MinValues = _binner.BinStructure.AggregatedMinValues.ToDictionary(entry => entry.Key, entry => entry.Value),
                            MaxValues = _binner.BinStructure.AggregatedMaxValues.ToDictionary(entry => entry.Key, entry => entry.Value)
                        };
                    }
                    await fireUpdated(resultItemModels, _simDataProvider.Progress(), resultDescriptionModel);
                }
                dataRows = await _simDataProvider.GetSampleDataRows(_sampleSize);

                if (MainViewController.Instance.MainModel.Verbose)
                {
                    Debug.WriteLine("Job Iteration Time: " + sw.ElapsedMilliseconds);
                }
                if (_throttle.Ticks > 0)
                {
                    await Task.Delay(_throttle);
                }
            }
            lock (_lock)
            {
                _isRunning = false;
            }
            await fireCompleted();
        }

        private void setVisualizationValues(List<DataRow> samples)
        {
            foreach (var sample in samples)
            {
                for (int d = 0; d < _dimensions.Count; d++)
                {
                    sample.VisualizationValues[_dimensions[d].InputModel] = getVisualizationValue(_axisTypes[d], sample.Entries[_dimensions[d].InputModel], _dimensions[d], _uniqueValues[d]);
                }
            }
        }

        private double? getVisualizationValue(AxisType axisType, object value, InputOperationModel inputOperationModel, Dictionary<object, double> uniqueValues) 
        {
            if (axisType == AxisType.Quantitative)
            {
                return value == null ? null : (double?)double.Parse(value.ToString());
            }
            else if (axisType == AxisType.Time)
            {
                return value == null ? null : (double?)((DateTime)value).TimeOfDay.Ticks;
            }
            else if (axisType == AxisType.Date)
            {
                return value == null ? null : (double?)((DateTime)value).Ticks;
            }
            else
            {
                if (!uniqueValues.ContainsKey(value.ToString()))
                {
                    uniqueValues.Add(value.ToString(), uniqueValues.Count);
                }
                return uniqueValues[value.ToString()];
            }
        }

        private List<ResultItemModel> convertBinsToResultItemModels(BinStructure binStructure)
        {
            List<ResultItemModel> returnValues = new List<ResultItemModel>();

            for (int d = 0; d < _dimensions.Count; d++)
            {
                if (binStructure.BinRanges[d] is NominalBinRange)
                {
                    (binStructure.BinRanges[d] as NominalBinRange).Labels = _uniqueValues[d].OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key.ToString()).ToList();
                }
            }
            foreach (var bin in binStructure.Bins.Values)
            {
                VisualizationItemResultModel itemModel = new VisualizationItemResultModel();
                for (int d = 0; d < _dimensions.Count; d++)
                {
                    if (!(binStructure.BinRanges[d] is AggregateBinRange))
                    {
                        itemModel.AddValue(_dimensions[d],
                            new ResultItemValueModel(bin.Spans[d].Min, bin.Spans[d].Max));
                    }
                }

                foreach (var aom in bin.Values.Keys)
                {
                    itemModel.AddValue(aom, new ResultItemValueModel(
                               bin.Values[aom],
                               bin.NormalizedValues[aom]));
                }      
                returnValues.Add(itemModel);
            }

            return returnValues;
        }


        private async Task fireUpdated(List<ResultItemModel> samples, double progress, ResultDescriptionModel resultDescriptionModel)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (JobUpdate != null)
                {
                    JobUpdate(this, new JobEventArgs()
                    {
                        Samples = samples,
                        Progress = progress,
                        ResultDescriptionModel = resultDescriptionModel
                    });
                }
            });
        }

        private async Task fireCompleted()
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (JobCompleted != null)
                {
                    JobCompleted(this, new EventArgs());
                }
            }); 
            if (MainViewController.Instance.MainModel.Verbose)
            {
                Debug.WriteLine("Job Total Run Time: " + _stopWatch.ElapsedMilliseconds);
            }
        }
    }

    public class JobEventArgs : EventArgs
    {
        public List<ResultItemModel> Samples { get; set; }
        public double Progress { get; set; }
        public ResultDescriptionModel ResultDescriptionModel { get; set; }
    }
}

