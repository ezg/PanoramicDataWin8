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
        public event EventHandler<List<QueryResultItemModel>> JobUpdate;
        public event EventHandler<EventArgs> JobCompleted;

        private SimDataProvider _simDataProvider = null;
        private bool _isRunning = false;
        private double numberOfXBins = 0;
        private double numberOfYBins = 0;
        private int _sampleSize = 0;
        private bool _additive = false;
        private bool _isGrouped = false;
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
            _isGrouped = QueryModel.AttributeOperationModels.Any(aom => aom.IsGrouped || aom.IsBinned) || QueryModel.AttributeOperationModels.Any(aom => aom.AggregateFunction != AggregateFunction.None);

            if (QueryModel.VisualizationType == VisualizationType.Table)
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
                    Incremental = !_isGrouped,
                    XAxisType = _xAxisType,
                    YAxisType = _yAxisType
                };
            }
            _simDataProvider = new SimDataProvider(QueryModelClone, (QueryModel.SchemaModel.OriginModels[0] as SimOriginModel).Data, samplesToCheck);

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
            List<QueryResultItemModel> samples = _simDataProvider.GetSampleQueryResultItemModels(_sampleSize);
            while (samples != null && _isRunning)
            {
                if (QueryModelClone.VisualizationType != VisualizationType.Table)
                {
                    setVisualizationValues(samples);
                    if (_binner != null)
                    {
                        _binner.ProcessStep(samples);
                    }
                    samples = convertBinsToQueryResultItemModels(_binner.LastBinStructure);
                }

                if (_isRunning)
                {
                    await fireUpdated(samples);
                }
                await Task.Delay(_throttle);
                samples = _simDataProvider.GetSampleQueryResultItemModels(_sampleSize);
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
                if (_xAxisType == AxisType.Quantitative)
                {
                    sample.VisualizationResultValues.Add(VisualizationResult.X, sample.AttributeValues[xAom]);
                }
                else
                {
                    var queryValue = sample.AttributeValues[xAom];
                    if (!_xUniqueValues.ContainsKey(queryValue.StringValue))
                    {
                        _xUniqueValues.Add(queryValue.StringValue, _xUniqueValues.Count);
                    }
                    sample.VisualizationResultValues.Add(VisualizationResult.X,
                        new QueryResultItemValueModel()
                        {
                            Value = _xUniqueValues[queryValue.StringValue]
                        });
                }
                if (_yAxisType == AxisType.Quantitative)
                {
                    sample.VisualizationResultValues.Add(VisualizationResult.Y, sample.AttributeValues[yAom]);
                }
                else
                {
                    var queryValue = sample.AttributeValues[yAom];
                    if (!_yUniqueValues.ContainsKey(queryValue.StringValue))
                    {
                        _yUniqueValues.Add(queryValue.StringValue, _yUniqueValues.Count);
                    }
                    sample.VisualizationResultValues.Add(VisualizationResult.Y,
                        new QueryResultItemValueModel()
                        {
                            Value = _yUniqueValues[queryValue.StringValue]
                        });
                }
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

                    if (_xAxisType != AxisType.Quantitative)
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
                        binClone.LabelX = bin.BinMinX.ToString();
                    }
                    if (_yAxisType != AxisType.Quantitative)
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
                        binClone.LabelY = bin.BinMinY.ToString();
                    }

                    QueryResultItemModel itemModel = new QueryResultItemModel();
                    itemModel.Bin = binClone;

                    GroupingObject go = new GroupingObject(true, false, -1);
                    go.Add(0, col);
                    go.Add(1, row);
                    go.Add(2, binStructure.BinSizeX);
                    go.Add(3, binStructure.BinSizeY);

                    itemModel.GroupingObject = go;
                    newSamples.Add(itemModel);
                }
            }
            return newSamples;
        }


        private async Task fireUpdated(List<QueryResultItemModel> samples)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (JobUpdate != null)
                {
                    JobUpdate(this, samples);
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
}
