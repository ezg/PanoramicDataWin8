using PanoramicData.model.data;
using PanoramicData.model.data.sim;
using PanoramicData.model.view;
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
using PanoramicData.controller.view;
using PanoramicDataWin8.model.data;

namespace PanoramicDataWin8.controller.data.sim
{
    public class SimJob
    {
        public event EventHandler<JobEventArgs> JobUpdate;
        public event EventHandler<EventArgs> JobCompleted;

        private SimDataProvider _simDataProvider = null;
        private bool _isRunning = false;
        private int _sampleSize = 0;
        private bool _isGrouped = false;
        private bool _isIncremental = false;
        private TimeSpan _throttle = TimeSpan.FromMilliseconds(0);
        private Binner _binner = new Binner();
        private Object _lock = new Object();
        private AxisType _xAxisType = AxisType.Nominal;
        private AxisType _yAxisType = AxisType.Nominal;


        private Dictionary<object, double> _xUniqueValues = new Dictionary<object, double>();
        private Dictionary<object, double> _yUniqueValues = new Dictionary<object, double>();

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
            QueryModelClone = QueryModel.Clone();

            _isRunning = true;
            int samplesToCheck = -1;
            _isGrouped = QueryModel.AttributeOperationModels.Any(aom => aom.GroupMode != GroupMode.None) || QueryModel.AttributeOperationModels.Any(aom => aom.AggregateFunction != AggregateFunction.None);
            _isIncremental = !_isGrouped;

            if (QueryModel.VisualizationType == VisualizationType.table)
            {
                _binner = null;
                samplesToCheck = !_isGrouped ? 1000 : -1;
            }
            else
            {
                var xAom = QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).First();
                var yAom = QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).First();
                _xAxisType = QueryModel.GetAxisType(xAom);
                _yAxisType = QueryModel.GetAxisType(yAom);
                QueryModel.QueryResultModel.XAxisType = _xAxisType;
                QueryModel.QueryResultModel.YAxisType = _yAxisType;

                _binner = new Binner()
                {
                    NrOfXBins = MainViewController.Instance.MainModel.NrOfXBins,
                    NrOfYBins = MainViewController.Instance.MainModel.NrOfYBins,
                    Incremental = _isIncremental,
                    XAxisType = _xAxisType,
                    YAxisType = _yAxisType
                };
            }
            _simDataProvider = new SimDataProvider(QueryModelClone, (QueryModel.SchemaModel.OriginModels[0] as SimOriginModel), samplesToCheck);

            Task.Run(() => run());
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
            if (!_simDataProvider.IsInitialized)
            {
                await _simDataProvider.StartSampling();
            }

            List<QueryResultItemModel> samples = await _simDataProvider.GetSampleQueryResultItemModels(_sampleSize);
            while (samples != null && _isRunning)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                if (QueryModelClone.VisualizationType != VisualizationType.table)
                {
                    if (!_isIncremental)
                    {
                        _xUniqueValues.Clear();
                        _yUniqueValues.Clear();
                    }
                    setVisualizationValues(samples);
                    if (_binner != null)
                    {
                        _binner.ProcessStep(samples);
                    }
                    samples = convertBinsToQueryResultItemModels(_binner.LastBinStructure);
                }

                if (_isRunning)
                {
                    await fireUpdated(
                        samples,
                        _simDataProvider.Progress(),
                        _binner == null ? 0 : _binner.LastBinStructure.XNullCount,
                        _binner == null ? 0 : _binner.LastBinStructure.YNullCount,
                        _binner == null ? 0 : _binner.LastBinStructure.XAndYNullCount,
                        _binner == null ? null : _binner.LastBinStructure.XScale,
                        _binner == null ? null : _binner.LastBinStructure.YScale);
                }
                samples = await _simDataProvider.GetSampleQueryResultItemModels(_sampleSize);

                Debug.WriteLine("Job Iteration Time: " + sw.ElapsedMilliseconds);
                await Task.Delay(_throttle);
            }
            lock (_lock)
            {
                _isRunning = false;
            }
            await fireCompleted();
        }

        private void setVisualizationValues(List<QueryResultItemModel> samples)
        {
            var xAom = QueryModelClone.GetFunctionAttributeOperationModel(AttributeFunction.X).First();
            var yAom = QueryModelClone.GetFunctionAttributeOperationModel(AttributeFunction.Y).First();
            foreach (var sample in samples)
            {
                sample.VisualizationResultValues.Add(VisualizationResult.X, getVisualizationValue(_xAxisType, sample.AttributeValues[xAom], xAom, _xUniqueValues));
                sample.VisualizationResultValues.Add(VisualizationResult.Y, getVisualizationValue(_yAxisType, sample.AttributeValues[yAom], yAom, _yUniqueValues));
            }
        }

        private double? getVisualizationValue(AxisType axisType, QueryResultItemValueModel itemValue, AttributeOperationModel attributeOperationModel, Dictionary<object, double> uniqueValues) 
        {
            if (axisType == AxisType.Quantitative)
            {
                return itemValue.Value == null ? null : (double?)double.Parse(itemValue.Value.ToString());
            }
            else if (axisType == AxisType.Time)
            {
                return itemValue.Value == null ? null : (double?)((DateTime) itemValue.Value).TimeOfDay.Ticks;
            }
            else if (axisType == AxisType.Date)
            {
                return itemValue == null ? null : (double?)((DateTime)itemValue.Value).Ticks;
            }
            else
            {
                if (!uniqueValues.ContainsKey(itemValue.StringValue))
                {
                    uniqueValues.Add(itemValue.StringValue, uniqueValues.Count);
                }
                return uniqueValues[itemValue.StringValue];
            }
        }

        private List<QueryResultItemModel> convertBinsToQueryResultItemModels(BinStructure binStructure)
        {
            List<QueryResultItemModel> newSamples = new List<QueryResultItemModel>();
            for (int col = 0; col < binStructure.Bins.Count; col++)
            {
                for (int row = 0; row < binStructure.Bins[col].Count; row++)
                {
                    Bin bin = binStructure.Bins[col][row];
                    Bin binClone = new Bin()
                    {
                        BinMaxX = bin.BinMaxX,
                        BinMaxY = bin.BinMaxY,
                        BinMinX = bin.BinMinX,
                        BinMinY = bin.BinMinY,
                        Count = bin.Count,
                        HasInterval = bin.HasInterval,
                        IntervalMaxX = bin.IntervalMaxX,
                        IntervalMaxY = bin.IntervalMaxY,
                        IntervalMinX = bin.IntervalMinX,
                        IntervalMinY = bin.IntervalMinY,
                        NormalizedCount = bin.NormalizedCount,
                        Size = bin.Size
                    };

                    if (_xAxisType == AxisType.Nominal || _xAxisType == AxisType.Ordinal)
                    {
                        var k =_xUniqueValues.Where(kvp => kvp.Value == bin.BinMinX).FirstOrDefault();
                        if (k.Key != null)
                        {
                            binClone.LabelX = k.Key.ToString();
                        }
                        else
                        {
                            binClone.LabelX = "";
                        }
                    }
                    else
                    {
                        binClone.LabelX = binStructure.XScale.GetLabel(bin.BinMinX);
                    }

                    if (_yAxisType == AxisType.Nominal || _yAxisType == AxisType.Ordinal)
                    {
                        var k = _yUniqueValues.Where(kvp => kvp.Value == bin.BinMinY).FirstOrDefault();
                        if (k.Key != null)
                        {
                            binClone.LabelY = k.Key.ToString();
                        }
                        else
                        {
                            binClone.LabelY = "";
                        }
                    }
                    else
                    {
                        binClone.LabelY = binStructure.YScale.GetLabel(bin.BinMinY);
                    }

                    QueryResultItemModel itemModel = new QueryResultItemModel();
                    itemModel.Bin = binClone;

                    GroupingObject go = new GroupingObject(true, false, -1);
                    go.Add(0, col);
                    go.Add(1, row);
                    //go.Add(2, binStructure.BinSizeX);
                    //go.Add(3, binStructure.BinSizeY);

                    itemModel.GroupingObject = go;
                    newSamples.Add(itemModel);
                }
            }
            return newSamples;
        }


        private async Task fireUpdated(List<QueryResultItemModel> samples, double progress, double xNullCount, double yNullCount, double xAndYNullCount, Scale xScale, Scale yScale)
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
                        XAndYNullCount = xAndYNullCount,
                        YNullCount = yNullCount,
                        XNullCount = xNullCount,
                        XScale = xScale,
                        YScale = yScale
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
        }
    }

    public class JobEventArgs : EventArgs
    {
        public List<QueryResultItemModel> Samples { get; set; }
        public double Progress { get; set; }
        public double XNullCount { get; set; }
        public double YNullCount { get; set; }
        public double XAndYNullCount { get; set; }
        public Scale XScale { get; set; }
        public Scale YScale { get; set; }
    }
}

