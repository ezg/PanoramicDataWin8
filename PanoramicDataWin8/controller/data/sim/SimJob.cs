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
        private bool _isIncremental = false;
        private TimeSpan _throttle = TimeSpan.FromMilliseconds(0);
        private DataBinner _binner = new DataBinner();
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

            if (QueryModel.VisualizationType == VisualizationType.table)
            {
                _binner = null;
                samplesToCheck = 1000;
            }
            else
            {
                var xAom = QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).First();
                var yAom = QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).First();
                _xAxisType = QueryModel.GetAxisType(xAom);
                _yAxisType = QueryModel.GetAxisType(yAom);
                QueryModel.QueryResultModel.XAxisType = _xAxisType;
                QueryModel.QueryResultModel.YAxisType = _yAxisType;

                _isIncremental = xAom.AggregateFunction == AggregateFunction.None && yAom.AggregateFunction == AggregateFunction.None;

                _binner = new DataBinner()
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

            List<DataRow> dataRows = await _simDataProvider.GetSampleDataRows(_sampleSize);
            List<QueryResultItemModel> queryResultItemModels = new List<QueryResultItemModel>();
            while (dataRows != null && _isRunning)
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
                    setVisualizationValues(dataRows);
                    if (_binner != null)
                    {
                        _binner.ProcessStep(dataRows);
                    }
                    queryResultItemModels = convertBinsToQueryResultItemModels(_binner.DataBinStructure);
                }

                if (_isRunning)
                {
                    await fireUpdated(
                        queryResultItemModels,
                        _simDataProvider.Progress(),
                        _binner == null ? 0 : _binner.DataBinStructure.XNullCount,
                        _binner == null ? 0 : _binner.DataBinStructure.YNullCount,
                        _binner == null ? 0 : _binner.DataBinStructure.XAndYNullCount,
                        _binner == null ? null : _binner.DataBinStructure.XBinRange,
                        _binner == null ? null : _binner.DataBinStructure.YBinRange);
                }
                dataRows = await _simDataProvider.GetSampleDataRows(_sampleSize);

                Debug.WriteLine("Job Iteration Time: " + sw.ElapsedMilliseconds);
                await Task.Delay(_throttle);
            }
            lock (_lock)
            {
                _isRunning = false;
            }
            await fireCompleted();
        }

        private void setVisualizationValues(List<DataRow> samples)
        {
            var xAom = QueryModelClone.GetFunctionAttributeOperationModel(AttributeFunction.X).First();
            var yAom = QueryModelClone.GetFunctionAttributeOperationModel(AttributeFunction.Y).First();
            foreach (var sample in samples)
            {
                sample.VisualizationResultValues.Add(VisualizationResult.X, getVisualizationValue(_xAxisType, sample.Entries[xAom.AttributeModel], xAom, _xUniqueValues));
                sample.VisualizationResultValues.Add(VisualizationResult.Y, getVisualizationValue(_yAxisType, sample.Entries[yAom.AttributeModel], yAom, _yUniqueValues));
            }
        }

        private double? getVisualizationValue(AxisType axisType, object value, AttributeOperationModel attributeOperationModel, Dictionary<object, double> uniqueValues) 
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

        private List<QueryResultItemModel> convertBinsToQueryResultItemModels(DataBinStructure binStructure)
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
                        binClone.LabelX = binStructure.XBinRange.GetLabel(bin.BinMinX);
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
                        binClone.LabelY = binStructure.YBinRange.GetLabel(bin.BinMinY);
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


        private async Task fireUpdated(List<QueryResultItemModel> samples, double progress, double xNullCount, double yNullCount, double xAndYNullCount, BinRange xBinRange, BinRange yBinRange)
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
                        XBinRange = xBinRange,
                        YBinRange = yBinRange
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
        public BinRange XBinRange { get; set; }
        public BinRange YBinRange { get; set; }
    }
}

