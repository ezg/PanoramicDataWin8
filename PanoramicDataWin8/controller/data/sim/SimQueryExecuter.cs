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
            int samplesToCheck =-1;
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
                    Incremental = true,
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
            int xCount = 0;
            int yCount = 0;
            Dictionary<object, double> xUniqueValues = new Dictionary<object, double>();
            Dictionary<object, double> yUniqueValues = new Dictionary<object, double>();
            foreach (var sample in samples)
            {
                if (_xAxisType == AxisType.Nominal)
                {
                    var queryValue = sample.AttributeValues[xAom];
                    if (!xUniqueValues.ContainsKey(queryValue.Value))
                    {
                        xUniqueValues.Add(queryValue.Value, xCount++);
                    }
                    sample.VisualizationResultValues.Add(VisualizationResult.X,
                        new QueryResultItemValueModel()
                        {
                            Value = xUniqueValues[queryValue.Value]
                        });
                }
                else
                {
                    sample.VisualizationResultValues.Add(VisualizationResult.X, sample.AttributeValues[xAom]);
                }

                if (_yAxisType == AxisType.Nominal)
                {
                    var queryValue = sample.AttributeValues[yAom];
                    if (!yUniqueValues.ContainsKey(queryValue.Value))
                    {
                        yUniqueValues.Add(queryValue.Value, yCount++);
                    }
                    sample.VisualizationResultValues.Add(VisualizationResult.Y,
                        new QueryResultItemValueModel()
                        {
                            Value = yUniqueValues[queryValue.Value]
                        });
                }
                else
                {
                    sample.VisualizationResultValues.Add(VisualizationResult.Y, sample.AttributeValues[yAom]);
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
                    QueryResultItemModel itemModel = new QueryResultItemModel();
                    itemModel.BinMinXValue = bin.BinMinX;
                    itemModel.BinMinYValue = bin.BinMinY;
                    itemModel.BinMaxXValue = bin.BinMaxX;
                    itemModel.BinMaxYValue = bin.BinMaxY;
                    itemModel.BinSize = bin.Size;

                    GroupingObject go = new GroupingObject(true, false, -1);
                    go.Add(0, col);
                    go.Add(1, row);

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
        public bool Incremental { get; set; }
        public AxisType XAxisType { get; set; }
        public AxisType YAxisType { get; set; }
        public double NrOfXBins { get; set; }
        public double NrOfYBins { get; set; }
        public BinStructure LastBinStructure { get; set; }

        public void ProcessStep(List<QueryResultItemModel> sampleQueryResultItemModels)
        {
            double minX = sampleQueryResultItemModels.Min(dp => dp.XValue);
            double minY = sampleQueryResultItemModels.Min(dp => dp.YValue);
            double maxX = sampleQueryResultItemModels.Max(dp => dp.XValue);
            double maxY = sampleQueryResultItemModels.Max(dp => dp.YValue);

            if (LastBinStructure != null && Incremental)
            {
                minX = Math.Min(LastBinStructure.MinX, minX);
                minY = Math.Min(LastBinStructure.MinY, minY);
                maxX = Math.Max(LastBinStructure.MaxX, maxX);
                maxY = Math.Max(LastBinStructure.MaxY, maxY);
            }
            else
            {
                double[] xExtent = getExtent(minX, maxX, XAxisType != AxisType.Quantitative ? (maxX - minX) : NrOfXBins);
                double[] yExtent = getExtent(minY, maxY, XAxisType != AxisType.Quantitative ? (maxY - minY) : NrOfYBins);
                LastBinStructure = 
                    initializeBinStructure(xExtent[0], xExtent[1], yExtent[0], yExtent[1],
                    xExtent[2], yExtent[2],
                    XAxisType != AxisType.Quantitative ? (maxX - minX) : NrOfXBins,
                    XAxisType != AxisType.Quantitative ? (maxY - minY) : NrOfYBins);
            }
            double plusX = 0;
            double plusY = 0;
            calculateNrOfBinsToAdd(out plusX, out plusY, LastBinStructure, minX, maxX, minY, maxY);
            minX = LastBinStructure.MinX - (Math.Ceiling((LastBinStructure.MinX - minX) / LastBinStructure.SizeX) * LastBinStructure.SizeX);
            minY = LastBinStructure.MinY - (Math.Ceiling((LastBinStructure.MinY - minY) / LastBinStructure.SizeY) * LastBinStructure.SizeY);

            BinStructure tempBinStructure = 
                initializeBinStructure(minX, maxX, minY, maxY,
                    LastBinStructure.SizeX, LastBinStructure.SizeY,
                    (LastBinStructure.Bins.Count - 1) + plusX, (LastBinStructure.Bins[0].Count - 1) + plusY);

            binSamples(tempBinStructure, sampleQueryResultItemModels);

            // re-map old bins
            if (Incremental)
            {
                foreach (var oldBin in LastBinStructure.Bins.SelectMany(b => b))
                {
                    int containCount = 0;
                    foreach (var newBin in tempBinStructure.Bins.SelectMany(b => b))
                    {
                        if (newBin.ContainsBin(oldBin))
                        {
                            newBin.Count += oldBin.Count;
                            containCount++;
                        }
                    }
                    if (containCount != 1)
                    {

                    }
                }
            }

            int xBinsToMerge = XAxisType == AxisType.Quantitative ? (int)((tempBinStructure.Bins.Count -1) / (NrOfXBins)) : 1;
            int yBinsToMerge = YAxisType == AxisType.Quantitative ? (int)((tempBinStructure.Bins[0].Count -1) / (NrOfYBins)) : 1;

            BinStructure mergedBinStructure = new BinStructure();
            mergedBinStructure.SizeX = tempBinStructure.SizeX * xBinsToMerge;
            mergedBinStructure.SizeY = tempBinStructure.SizeY * yBinsToMerge;

            mergeBinStructure(mergedBinStructure, tempBinStructure, xBinsToMerge, yBinsToMerge);

            adjustNormalizedCount(mergedBinStructure);

            LastBinStructure = mergedBinStructure;
        }

        private void mergeBinStructure(BinStructure mergedBinStructure, BinStructure binStructure, int xBinsToMerge, int yBinsToMerge)
        {
            // merge bins
            for (int col = 0; col < binStructure.Bins.Count; col += xBinsToMerge)
            {
                List<Bin> newBinCol = new List<Bin>();
                for (int row = 0; row < binStructure.Bins[col].Count; row += yBinsToMerge)
                {
                    List<Bin> bins = binStructure.Bins.Skip(col).Take(xBinsToMerge).SelectMany(b => b.Skip(row).Take(yBinsToMerge)).ToList();

                    Bin bin = new Bin()
                    {
                        BinMinX = bins.Min(b => b.BinMinX),
                        BinMaxX = bins.Max(b => b.BinMaxX),
                        BinMinY = bins.Min(b => b.BinMinY),
                        BinMaxY = bins.Max(b => b.BinMaxY),
                        Count = bins.Sum(b => b.Count)
                    };
                     
                    newBinCol.Add(bin);
                }
                mergedBinStructure.Bins.Add(newBinCol);
            }
            mergedBinStructure.MinX = mergedBinStructure.Bins.SelectMany(b => b).Min(b => b.BinMinX);
            mergedBinStructure.MinY = mergedBinStructure.Bins.SelectMany(b => b).Min(b => b.BinMinY);
            mergedBinStructure.MaxX = mergedBinStructure.Bins.SelectMany(b => b).Max(b => b.BinMaxX);
            mergedBinStructure.MaxY = mergedBinStructure.Bins.SelectMany(b => b).Max(b => b.BinMaxY);
        }

        private void adjustNormalizedCount(BinStructure binStructure)
        {
            // adjust normalized count
            double maxCount = binStructure.Bins.SelectMany(b => b).Max(b => b.Count);
            foreach (var bin in binStructure.Bins.SelectMany(b => b))
            {
                //bin.NormalizedCount = Math.Log(bin.Count) / Math.Log(maxCount);
                bin.NormalizedCount = bin.Count / maxCount;
                if (bin.NormalizedCount == 0)
                {
                    bin.Size = 0;
                }
                else
                {
                    //double r = sliderRange.Value;
                    // 0.1 * log10(x ) + 1
                    bin.Size = Math.Sqrt(bin.NormalizedCount);// 0.1 * Math.Log10(bin.NormalizedCount) + 1;// bin.NormalizedCount; // = (1.0 / (r + 1)) * Math.Ceiling(bin.NormalizedCount / (1.0 / r));
                }
            }
        }

        private void binSamples(BinStructure binStructure, List<QueryResultItemModel> samples)
        {
            foreach (var bin in binStructure.Bins.SelectMany(b => b))
            {
                List<QueryResultItemModel> intersectingQueryResultItemModels = new List<QueryResultItemModel>();
                foreach (var dp in samples)
                {
                    if (bin.BinIntersects(dp.XValue, dp.YValue))
                    {
                        intersectingQueryResultItemModels.Add(dp);
                    }
                }
                bin.Count += intersectingQueryResultItemModels.Count;
            }
        }
        
        private double[] getExtent(double min, double max, double m)
        {
            double span = max - min;

            double step = Math.Pow(10, Math.Floor(Math.Log10(span / m)));
            double err = m / span * step;

            if (err <= .15)
                step *= 10;
            else if (err <= .35)
                step *= 5;
            else if (err <= .75)
                step *= 2;

            double[] ret = new double[3];
            ret[0] = (double)(Math.Floor(min / step) * step);
            ret[1] = (double)(Math.Floor(max / step) * step + step);
            ret[2] = (double)step;

            return ret;
        }

        private BinStructure initializeBinStructure(double minX, double maxX, double minY, double maxY, double sizeX, double sizeY, double nrOfXBins, double nrOfYBins)
        {
            BinStructure binStructure = new BinStructure();
            binStructure.SizeX = sizeX;
            binStructure.SizeY = sizeY;

            for (int xIndex = 0; xIndex <= nrOfXBins; xIndex++)
            {
                double x = minX + xIndex * sizeX;
                List<Bin> newBinCol = new List<Bin>();
                for (int yIndex = 0; yIndex <= nrOfYBins; yIndex++)
                {
                    double y = minY + yIndex * sizeY;
                    Bin bin = new Bin()
                    {
                        BinMinX = x,
                        BinMaxX = x + sizeX,
                        BinMinY = y,
                        BinMaxY = y + sizeY,
                        Count = 0
                    };
                    newBinCol.Add(bin);
                }
                binStructure.Bins.Add(newBinCol);
            }

            binStructure.MinX = minX;
            binStructure.MinY = minY;
            binStructure.MaxX = maxX;
            binStructure.MaxY = maxY;

            return binStructure;
        }

        private BinStructure initializeBinStructure(double minX, double maxX, double minY, double maxY)
        {
            double nrOfXBins = NrOfXBins;
            double nrOfYBins = NrOfYBins; 
            if (XAxisType != AxisType.Quantitative)
            {
                nrOfXBins = (maxX - minX);
            }
            if (YAxisType != AxisType.Quantitative)
            {
                nrOfYBins = (maxY - minY);
            }

            double sizeX = (maxX - minX) / nrOfXBins;
            double sizeY = (maxY - minY) / nrOfYBins;

            return initializeBinStructure(minX, maxX, minY, maxY, sizeX, sizeY, nrOfXBins, nrOfYBins);            
        }

        private void calculateNrOfBinsToAdd(out double plusX, out double plusY, BinStructure binStructure, double minX, double maxX, double minY, double maxY)
        {
            plusX = 0;
            plusY = 0;
            if (XAxisType == AxisType.Quantitative)
            {
                if (minX < binStructure.MinX)
                {
                    minX = binStructure.MinX - (Math.Ceiling((binStructure.MinX - minX) / binStructure.SizeX) * binStructure.SizeX);
                    plusX += Math.Ceiling((binStructure.MinX - minX) / binStructure.SizeX);
                }
                if (maxX > binStructure.MaxX)
                {
                    maxX = binStructure.MaxX + (Math.Ceiling((maxX - binStructure.MaxX) / binStructure.SizeX) * binStructure.SizeX);
                    plusX += Math.Ceiling((maxX - binStructure.MaxX) / binStructure.SizeX);
                }

                if (plusX != 0)
                {
                    //plusX = Math.Ceiling(plusX / NrOfXBins) * NrOfXBins;
                }
            }
            else
            {
                plusX = Math.Max(0, (maxX - minX) - (binStructure.MaxX - binStructure.MinX));
            }

            if (YAxisType == AxisType.Quantitative)
            {
                if (minY < binStructure.MinY)
                {
                    minY = binStructure.MinY - (Math.Ceiling((binStructure.MinY - minY) / binStructure.SizeY) * binStructure.SizeY);
                    plusY += Math.Ceiling((binStructure.MinY - minY) / binStructure.SizeY);
                }
                if (maxY > binStructure.MaxY)
                {
                    maxY = binStructure.MaxY + (Math.Ceiling((maxY - binStructure.MaxY) / binStructure.SizeY) * binStructure.SizeY);
                    plusY += Math.Ceiling((maxY - binStructure.MaxY) / binStructure.SizeY);
                }

                if (plusY != 0)
                {
                    //plusY = Math.Ceiling(plusY / NrOfYBins) * NrOfYBins;
                }
            }
            else
            {
                plusY = Math.Max(0, (maxY - minY) - (binStructure.MaxY - binStructure.MinY));
            }
        }
    }

     public class BinStructure
    {
        public BinStructure()
        {
            Bins = new List<List<Bin>>();
            MinX = Double.MaxValue;
            MinY = Double.MaxValue;
            MaxX = Double.MinValue;
            MaxY = Double.MinValue;
        }

        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public double SizeX { get; set; }
        public double SizeY { get; set; }
        public List<List<Bin>> Bins { get; set; }
    }

    public class Bin
    {
        public Bin()
        {
            IntervalMinX = Double.MaxValue;
            IntervalMinY = Double.MaxValue;
            IntervalMaxX = Double.MinValue;
            IntervalMaxY = Double.MinValue;
        }

        private bool intersects(double r1Left, double r1Right, double r1Top, double r1Bottom, double r2Left, double r2Right, double r2Top, double r2Bottom)
        {
            return !(r2Left > r1Right ||
                     r2Right < r1Left ||
                     r2Top > r1Bottom ||
                     r2Bottom < r1Top);
        }
        public bool ContainsBin(Bin bin)
        {
            return (bin.BinMinX >= this.BinMinX || aboutEqual(bin.BinMinX, this.BinMinX)) &&
                   (bin.BinMaxX <= this.BinMaxX || aboutEqual(bin.BinMaxX, this.BinMaxX)) &&
                   (bin.BinMinY >= this.BinMinY || aboutEqual(bin.BinMinY, this.BinMinY)) &&
                   (bin.BinMaxY <= this.BinMaxY || aboutEqual(bin.BinMaxY, this.BinMaxY));
        }

        private bool aboutEqual(double x, double y)
        {
            double epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1E-15;
            return Math.Abs(x - y) <= epsilon;
        }


        public bool BinIntersects(double x, double y)
        {
            return x >= this.BinMinX && x < this.BinMaxX && y >= this.BinMinY && y < this.BinMaxY;
        }

        public bool BinIntersects(Bin b)
        {
            return intersects(this.BinMinX, this.BinMaxX, this.BinMinY, this.BinMaxY, b.BinMinX, b.BinMaxX, b.BinMinY, b.BinMaxY);
        }

        public bool IntervalIntersects(Bin b)
        {
            return intersects(this.IntervalMinX, this.IntervalMaxX, this.IntervalMinY, this.IntervalMaxY, b.IntervalMinX, b.IntervalMaxX, b.IntervalMinY, b.IntervalMaxY);
        }

        public double BinXOverlapRatio(double left, double right)
        {
            if (left < this.BinMaxX && right >= this.BinMinX)
            {
                double newLeft = Math.Max(left, this.BinMinX);
                double newRight = Math.Min(right, this.BinMaxX);
                if (right - left == 0)
                {
                    return 1.0;
                }
                else
                {
                    return (newRight - newLeft) / (right - left);
                }
            }
            return 0;
        }

        public double BinYOverlapRatio(double top, double bottom)
        {
            if (top < this.BinMaxY && bottom >= this.BinMinY)
            {
                double newTop = Math.Max(top, this.BinMinY);
                double newBottom = Math.Min(bottom, this.BinMaxY);
                if (bottom - top == 0)
                {
                    return 1.0;
                }
                else
                {
                    return (newBottom - newTop) / (bottom - top);
                }
            }
            return 0;
        }

        public double Size { get; set; }
        public double Count { get; set; }
        public double NormalizedCount { get; set; }
        public double BinMinX { get; set; }
        public double BinMinY { get; set; }
        public double BinMaxX { get; set; }
        public double BinMaxY { get; set; }
        public bool HasInterval { get; set; }
        public double IntervalMinX { get; set; }
        public double IntervalMinY { get; set; }
        public double IntervalMaxX { get; set; }
        public double IntervalMaxY { get; set; }
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