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
namespace PanoramicData.controller.data.sim
{
    public class SimQueryExecuter : QueryExecuter
    {
        private Dictionary<QueryModel, SimJob> _activeJobs = new Dictionary<QueryModel, SimJob>();
        private Dictionary<QueryModel, Dictionary<GroupingObject, KeyValuePair<int, QueryResultItemModel>>> _updateIndexCache = new Dictionary<QueryModel, Dictionary<GroupingObject, KeyValuePair<int, QueryResultItemModel>>>();
        
        public override void ExecuteQuery(QueryModel queryModel)
        {
            queryModel.QueryResultModel.QueryResultItemModels = new ObservableCollection<QueryResultItemModel>();

            if (_activeJobs.ContainsKey(queryModel))
            {
                _activeJobs[queryModel].Stop();
                _activeJobs[queryModel].JobUpdate -= simJob_JobUpdate;
                _activeJobs[queryModel].JobCompleted -= simJob_JobCompleted;
                _activeJobs.Remove(queryModel);
                _updateIndexCache.Remove(queryModel);
            }
            // determine if new job is even needed (i.e., are all relevant attributeModels set)
            if ((queryModel.VisualizationType == VisualizationType.Table && queryModel.AttributeFunctionOperationModels.Count > 0) ||
                (queryModel.VisualizationType != VisualizationType.Table && queryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).Any() &&  queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).Any()))
            {
                SimJob simJob = new SimJob(queryModel, TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis), (int)MainViewController.Instance.MainModel.SampleSize);
                _activeJobs.Add(queryModel, simJob);
                _updateIndexCache.Add(queryModel, new Dictionary<GroupingObject, KeyValuePair<int, QueryResultItemModel>>());
                simJob.JobUpdate += simJob_JobUpdate;
                simJob.JobCompleted += simJob_JobCompleted;
                simJob.Start();
            }
            
        }

        void simJob_JobCompleted(object sender, EventArgs e)
        {
        }

        void simJob_JobUpdate(object sender, List<QueryResultItemModel> samples)
        {
            SimJob job = sender as SimJob;
            var oldItems = job.QueryModel.QueryResultModel.QueryResultItemModels;

            var cache = _updateIndexCache[job.QueryModel];

            // update existing ones
            for (int i = 0; i < samples.Count; i++)
            {
                var sample = samples[i];
                if (cache.ContainsKey(sample.GroupingObject))
                {
                    KeyValuePair<int, QueryResultItemModel> kvp = cache[sample.GroupingObject];
                    if (kvp.Key == i)
                    {
                        oldItems[i].Update(sample);
                    }
                    else
                    {
                        kvp.Value.Update(sample);
                        if (oldItems.Count <= i)
                        {
                            oldItems.Add(kvp.Value);
                        }
                        else
                        {
                            oldItems[i] = kvp.Value;
                        }
                    }
                    sample = oldItems[i];
                    cache[sample.GroupingObject] = new KeyValuePair<int, QueryResultItemModel>(i, sample);
                }
                else
                {
                    if (oldItems.Count <= i)
                    {
                        oldItems.Add(sample);
                    }
                    else
                    {
                        oldItems[i] = sample;
                    }
                    cache.Add(sample.GroupingObject, new KeyValuePair<int, QueryResultItemModel>(i, sample));
                }
            }
            // remove old ones
            for (int i = samples.Count; i < oldItems.Count; i++)
            {
                var oldItem = oldItems[i];
                oldItems.RemoveAt(i);
                if (cache.ContainsKey(oldItem.GroupingObject))
                {
                    cache.Remove(oldItem.GroupingObject);
                }
            }

            job.QueryModel.QueryResultModel.FireQueryResultModelUpdated();
            Debug.Assert(oldItems.Count < 10000, "size of result list should not be big");
        }
    }

    public class SimJob
    {
        public event EventHandler<List<QueryResultItemModel>> JobUpdate;
        public event EventHandler<EventArgs> JobCompleted;

        private SimDataProvider _simDataProvider = null;
        private bool _isRunning = false;
        private double _nrBinsX = 0;
        private double _nrBinsY = 0;
        private int _sampleSize = 0;
        private bool _additive = false;
        private TimeSpan _throttle = TimeSpan.FromMilliseconds(0);
        private Binner _binner = new Binner();
        private Object _lock = new Object();
        private AxisType _xAxisType = AxisType.Nominal;
        private AxisType _yAxisType = AxisType.Nominal;

        public QueryModel QueryModel { get; set; }

        public SimJob(QueryModel queryModel, TimeSpan throttle, int sampleSize)
        {
            QueryModel = queryModel;
            _sampleSize = sampleSize;
            _throttle = throttle;
        }

        public void Start()
        {
            _isRunning = true;
            int samplesToCheck =-1;
            if (QueryModel.VisualizationType == VisualizationType.Table)
            {
                _binner = null;
                if (QueryModel.AttributeOperationModels.Any(aom => aom.IsGrouped || aom.IsBinned) || QueryModel.AttributeOperationModels.Any(aom => aom.AggregateFunction != AggregateFunction.None))
                {
                    samplesToCheck = 1000;
                }
            }
            else
            {
                var xAom = QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).First();
                var yAom = QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).First();
                _xAxisType = QueryModel.GetAxisType(xAom);
                _yAxisType = QueryModel.GetAxisType(yAom);
                QueryModel.QueryResultModel.XAxisType = _xAxisType; 
                QueryModel.QueryResultModel.YAxisType = _yAxisType;
            }
            _simDataProvider = new SimDataProvider(QueryModel.Clone(), (QueryModel.SchemaModel.OriginModels[0] as SimOriginModel).Data, samplesToCheck);

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
                /*List<DataPoint> sampleDataPoints = _dataController.GetSampleDataPoints(_nrProcessedSamples, _dataSampleSize);

                CurrentBinStructure = processStep(sampleDataPoints, CurrentBinStructure, (int)Math.Round(_nrBinsX), (int)Math.Round(_nrBinsY), _additive);
                _nrProcessedSamples += _dataSampleSize;

                if (_additive)
                {
                    AllProcessedDataPoints.AddRange(sampleDataPoints);
                }
                else
                {
                    AllProcessedDataPoints = sampleDataPoints;
                }

                foreach (var newBin in CurrentBinStructure.Bins.SelectMany(b => b))
                {
                    newBin.ColorIntensity = Math.Min(1, (double)(_nrProcessedSamples) / (double)_dataController.GetTotalSamples());
                }

                if (JobUpdate != null)
                {
                    JobUpdate(this, new EventArgs());
                }
                Debug.WriteLine("Process Step time : " + timer.ElapsedMilliseconds + " " + CurrentBinStructure.Bins.Sum(b => b.Count));*/
                //lock (_lock)
                {
                    if (_isRunning)
                    {
                        await fireUpdated(samples);
                    }
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

    public class SimDataProvider
    {
        private QueryModel _queryModel = null;
        private List<Dictionary<AttributeModel, object>> _data = null;
        private int _nrProcessedSamples = 0;
        private int _nrSamplesToCheck = -1;

        private Dictionary<GroupingObject, IterativeCalculationObject> _iterativeCaluclationObjects = new Dictionary<GroupingObject, IterativeCalculationObject>();

        public SimDataProvider(QueryModel queryModel, List<Dictionary<AttributeModel, object>> data, int nrSamplesToCheck = -1)
        {
            _queryModel = queryModel;
            _data = data;
            _nrSamplesToCheck = nrSamplesToCheck;
        }

        public void StartSampling()
        {
            _nrProcessedSamples = 0;
            _iterativeCaluclationObjects.Clear();
        }

        public List<QueryResultItemModel> GetSampleQueryResultItemModels(int sampleSize)
        {
            if (_nrProcessedSamples < GetNrTotalSamples())
            {
                List<QueryResultItemModel> returnList = new List<QueryResultItemModel>();

                foreach (Dictionary<AttributeModel, object> row in _data.Skip(_nrProcessedSamples).Take(sampleSize))
                {
                    GroupingObject groupingObject = getGroupingObject(row, _queryModel, row[(_queryModel.SchemaModel.OriginModels[0] as SimOriginModel).IdAttributeModel]);
                    if (!_iterativeCaluclationObjects.ContainsKey(groupingObject))
                    {
                        _iterativeCaluclationObjects.Add(groupingObject, new IterativeCalculationObject());
                    }
                    IterativeCalculationObject iterativeCalculation = _iterativeCaluclationObjects[groupingObject];
                    iterativeCalculation.Update(row, _queryModel);
                }

                foreach (GroupingObject groupingObject in _iterativeCaluclationObjects.Keys)
                {
                    var attributeOperationModels = _queryModel.AttributeOperationModels;
                    var iterativeCalculation = _iterativeCaluclationObjects[groupingObject];
                    QueryResultItemModel item = new QueryResultItemModel()
                    {
                        GroupingObject = groupingObject
                    };
                    foreach (var attributeOperationModel in attributeOperationModels)
                    {
                        if (iterativeCalculation.AggregateValues.ContainsKey(attributeOperationModel))
                        {
                            QueryResultItemValueModel valueModel = fromRaw(
                                attributeOperationModel,
                                iterativeCalculation.AggregateValues[attributeOperationModel],
                                iterativeCalculation.IsBinned[attributeOperationModel],
                                iterativeCalculation.BinSize[attributeOperationModel]);
                            if (!item.AttributeValues.ContainsKey(attributeOperationModel))
                            {
                                item.AttributeValues.Add(attributeOperationModel, valueModel);
                            }
                        }
                    }
                    returnList.Add(item);
                }
                _nrProcessedSamples += sampleSize;

                return returnList.OrderBy(item => item, new ItemComparer(_queryModel)).ToList();
            }
            else
            {
                return null;
            }
        }

        public int GetNrTotalSamples()
        {
            if (_nrSamplesToCheck == -1)
            {
                return _data.Count;
            }
            else
            {
                return _nrSamplesToCheck;
            }
        }

        private GroupingObject getGroupingObject(Dictionary<AttributeModel, object> item, QueryModel queryModel, object idValue)
        {
            var groupers = queryModel.AttributeOperationModels.Where(aom => aom.IsGrouped || aom.IsBinned).ToList();
            GroupingObject groupingObject = new GroupingObject(
                groupers.Count() > 0,
                queryModel.AttributeOperationModels.Any(aom => aom.AggregateFunction != AggregateFunction.None), 
                idValue);
            int count = 0;
            foreach (var attributeModel in item.Keys)
            {
                if (groupers.Count(avo => avo.IsGrouped && avo.AttributeModel.Equals(attributeModel)) > 0)
                {
                    groupingObject.Add(count++, item[attributeModel]);
                }
                else if (groupers.Count(avo => avo.IsBinned && avo.AttributeModel.Equals(attributeModel)) > 0)
                {
                    AttributeOperationModel bin = groupers.Where(avo => avo.IsBinned && avo.AttributeModel.Equals(attributeModel)).First();
                    if (item[attributeModel] == null)
                    {
                        groupingObject.Add(count++, item[attributeModel]);
                    }
                    else
                    {
                        double d = double.Parse(item[attributeModel].ToString());
                        groupingObject.Add(count++, Math.Floor(d / bin.BinSize) * bin.BinSize);
                    }
                }
            }
            return groupingObject;
        }

        private QueryResultItemValueModel fromRaw(AttributeOperationModel attributeOperationModel, object value, bool binned, double binSize)
        {
            QueryResultItemValueModel valueModel = new QueryResultItemValueModel();
            if (value == null)
            {
                valueModel.Value = null;
                valueModel.StringValue = "";
                valueModel.ShortStringValue = "";
            }
            else
            {
                double d = 0.0;
                valueModel.Value = value;
                if (double.TryParse(value.ToString(), out d))
                {
                    valueModel.StringValue = valueModel.Value.ToString().Contains(".") ? d.ToString("N") : valueModel.Value.ToString();
                    if (binned)
                    {
                        valueModel.StringValue = d + " - " + (d + binSize);
                    }
                    else if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.BIT)
                    {
                        if (d == 1.0)
                        {
                            valueModel.StringValue = "True";
                        }
                        else if (d == 0.0)
                        {
                            valueModel.StringValue = "False";
                        }
                    }
                }
                else
                {
                    valueModel.StringValue = valueModel.Value.ToString();
                    if (valueModel.Value is DateTime)
                    {
                        valueModel.StringValue = ((DateTime)valueModel.Value).ToString();
                    }
                }
                if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.GEOGRAPHY)
                {
                    string toSplit = valueModel.StringValue;
                    if (toSplit.Contains("(") && toSplit.Contains(")"))
                    {
                        toSplit = toSplit.Substring(toSplit.IndexOf("("));
                        toSplit = toSplit.Substring(1, toSplit.IndexOf(")") - 1);
                    }
                    valueModel.ShortStringValue = valueModel.StringValue.Replace("(" + toSplit + ")", "");
                }
                else
                {
                    valueModel.ShortStringValue = valueModel.StringValue.TrimTo(300);
                }
            }
            return valueModel;
        }
    }

    public class Binner
    {

    }
    
    public class DataEqualityComparer : IEqualityComparer<Dictionary<AttributeModel, object>>
    {
        private QueryModel _queryModel = null;
        public DataEqualityComparer(QueryModel queryModel)
        {
            _queryModel = queryModel;
        }
        public bool Equals(Dictionary<AttributeModel, object> x, Dictionary<AttributeModel, object> y)
        {
            return x[(_queryModel.SchemaModel.OriginModels[0] as SimOriginModel).IdAttributeModel].Equals(
            y[(_queryModel.SchemaModel.OriginModels[0] as SimOriginModel).IdAttributeModel]);
        }
        public int GetHashCode(Dictionary<AttributeModel, object> x)
        {
            return x[(_queryModel.SchemaModel.OriginModels[0] as SimOriginModel).IdAttributeModel].GetHashCode();
        }
    }

    public class ItemComparer : IComparer<QueryResultItemModel>
    {
        private QueryModel _queryModel = null;
        public ItemComparer(QueryModel queryModel)
        {
            _queryModel = queryModel;
        }
        public int Compare(QueryResultItemModel x, QueryResultItemModel y)
        {
            var attributeOperationModels = _queryModel.AttributeOperationModels.Where(aom => aom.SortMode != SortMode.None);
            foreach (var aom in attributeOperationModels)
            {
                int factor = aom.SortMode == SortMode.Asc ? 1 : -1;
                if (x.AttributeValues[aom].Value is string &&
                   ((string)x.AttributeValues[aom].Value).CompareTo((string)y.AttributeValues[aom].Value) != 0)
                {
                    return (x.AttributeValues[aom].Value as string).CompareTo(y.AttributeValues[aom].Value as string) * factor;
                }
                else if (x.AttributeValues[aom].Value is double &&
                        ((double)x.AttributeValues[aom].Value).CompareTo((double)y.AttributeValues[aom].Value) != 0)
                {
                    return ((double)x.AttributeValues[aom].Value).CompareTo((double)y.AttributeValues[aom].Value) * factor;
                }
                else if (x.AttributeValues[aom].Value is int &&
                        ((int)x.AttributeValues[aom].Value).CompareTo((int)y.AttributeValues[aom].Value) != 0)
                {
                    return ((int)x.AttributeValues[aom].Value).CompareTo((int)y.AttributeValues[aom].Value) * factor;
                }
                else if (x.AttributeValues[aom].Value is DateTime &&
                        ((DateTime)x.AttributeValues[aom].Value).CompareTo((DateTime)y.AttributeValues[aom].Value) != 0)
                {
                    return ((DateTime)x.AttributeValues[aom].Value).CompareTo((DateTime)y.AttributeValues[aom].Value) * factor;
                }
            }
            return 0;
        }
    }

    public class IterativeCalculationObject
    {
        private double _n = 0;

        public IterativeCalculationObject()
        {
            LastRow = null;
            AggregateValues = new Dictionary<AttributeOperationModel, object>();
            BinSize = new Dictionary<AttributeOperationModel, double>();
            IsBinned = new Dictionary<AttributeOperationModel, bool>();
        }
        public Dictionary<AttributeModel, object> LastRow { get; set; }
        public Dictionary<AttributeOperationModel, object> AggregateValues { get; set; }
        public Dictionary<AttributeOperationModel, double> BinSize { get; set; }
        public Dictionary<AttributeOperationModel, bool> IsBinned { get; set; }

        public void Update(Dictionary<AttributeModel, object> row, QueryModel queryModel)
        {
            LastRow = row;
            var attributeOperationModels = queryModel.AttributeOperationModels;
            foreach (var attributeOperationModel in attributeOperationModels)
            {
                bool binned = false;
                double binSize = 0;
                object value = row[attributeOperationModel.AttributeModel];
                object rawValue = null;

                if (attributeOperationModel.AggregateFunction == AggregateFunction.Max)
                {
                    object currentValue = value;
                    if (AggregateValues.ContainsKey(attributeOperationModel))
                    {
                        currentValue = AggregateValues[attributeOperationModel];
                    }
                    rawValue = new object[] { value, currentValue }.Max();
                }
                else if (attributeOperationModel.AggregateFunction == AggregateFunction.Min)
                {
                    object currentValue = value;
                    if (AggregateValues.ContainsKey(attributeOperationModel))
                    {
                        currentValue = AggregateValues[attributeOperationModel];
                    }
                    rawValue = new object[] { value, currentValue }.Min();
                }
                else if (attributeOperationModel.AggregateFunction == AggregateFunction.Avg)
                {
                    object currentValue = 0.0;
                    if (AggregateValues.ContainsKey(attributeOperationModel))
                    {
                        currentValue = AggregateValues[attributeOperationModel];
                    }

                    if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.FLOAT)
                    {
                        rawValue = (((double)currentValue * _n) + (double)value) / (_n + 1);
                    }
                    else if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.INT)
                    {
                        rawValue = (((double)currentValue * _n) + (int)value) / (_n + 1);
                    }
                }
                else if (attributeOperationModel.AggregateFunction == AggregateFunction.Sum)
                {
                    object currentValue = 0.0;
                    if (AggregateValues.ContainsKey(attributeOperationModel))
                    {
                        currentValue = AggregateValues[attributeOperationModel];
                    }

                    if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.FLOAT)
                    {
                        rawValue = (double)currentValue + (double)value;
                    }
                    else if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.INT)
                    {
                        rawValue = (double)currentValue + (int)value;
                    }
                }
                else if (attributeOperationModel.AggregateFunction == AggregateFunction.Count)
                {
                    rawValue = _n + 1;
                }
                else if (attributeOperationModel.AggregateFunction == AggregateFunction.None)
                {
                    if (queryModel.AttributeOperationModels.Any(aom => aom.IsGrouped || aom.IsBinned))
                    {
                        if (queryModel.AttributeOperationModels.Where(aom => aom.IsGrouped || aom.IsBinned).Any(aom => aom.AttributeModel.Equals(attributeOperationModel.AttributeModel)))
                        {
                            AttributeOperationModel grouper = queryModel.AttributeOperationModels.Where(aom => aom.IsGrouped || aom.IsBinned).Where(aom => aom.AttributeModel.Equals(attributeOperationModel.AttributeModel)).First();
                            if (grouper.IsGrouped)
                            {
                                rawValue = value;
                            }
                            else if (grouper.IsBinned)
                            {
                                if (value != null)
                                {
                                    double d = double.Parse(value.ToString());
                                    rawValue = Math.Floor(d / grouper.BinSize) * grouper.BinSize;
                                    binned = true;
                                    binSize = grouper.BinSize;
                                }
                                else
                                {
                                    rawValue = null;
                                    binned = true;
                                    binSize = grouper.BinSize;
                                }
                            }
                        }
                        else
                        {
                            rawValue = "...";
                        }
                    }
                    else
                    {
                        if (queryModel.AttributeOperationModels.Any(aom => aom.AggregateFunction != AggregateFunction.None))
                        {
                            rawValue = "...";
                        }
                        else
                        {
                            rawValue = value;
                        }
                    }
                } 
                if (!AggregateValues.ContainsKey(attributeOperationModel))
                {
                    AggregateValues.Add(attributeOperationModel, rawValue);
                    BinSize.Add(attributeOperationModel, binSize);
                    IsBinned.Add(attributeOperationModel, binned);
                }
                else
                {
                    AggregateValues[attributeOperationModel] = rawValue;
                    BinSize[attributeOperationModel] = binSize;
                    IsBinned[attributeOperationModel] = binned;
                }
            }
            _n++;
        }
    }
}