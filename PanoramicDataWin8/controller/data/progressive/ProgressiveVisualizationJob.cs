using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Composition;
using Windows.UI.Core;
using IDEA_common.binning;
using IDEA_common.operations;
using IDEA_common.operations.histogram;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.common;
using PanoramicDataWin8.model.data.progressive;
using PanoramicDataWin8.model.data.result;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class ProgressiveVisualizationJob : Job
    {
        private Object _lock = new Object();
        private bool _isRunning = false;

        private Stopwatch _stopWatch = new Stopwatch();
        private HistogramOperationParameters _query = null;
        
        private int _sampleSize = 0;
        private IOperationReference _operationReference = null;

        private TimeSpan _throttle = TimeSpan.FromMilliseconds(0);
        public QueryModel QueryModel { get; set; }
        public QueryModel QueryModelClone { get; set; }

        public ProgressiveVisualizationJob(QueryModel queryModel, QueryModel queryModelClone, TimeSpan throttle, int sampleSize)
        {
            QueryModel = queryModel;
            QueryModelClone = queryModelClone;
            _sampleSize = sampleSize;
            _throttle = throttle;
            var psm = (queryModelClone.SchemaModel as ProgressiveSchemaModel);
            string filter = "";
            List<FilterModel> filterModels = new List<FilterModel>();
            filter = FilterModel.GetFilterModelsRecursive(QueryModelClone, new List<QueryModel>(), filterModels, true);
            
            List<string> aggregateFunctions = new List<string>();
            List<string> aggregateDimensions = new List<string>();
            List<string> dimensionAggregateFunctions = new List<string>();
            List<string> dimensions = new List<string>();
            List<string> brushes = new List<string>();


            foreach (var brushQueryModel in QueryModelClone.BrushQueryModels)
            {
                filterModels = new List<FilterModel>();
                var brush = FilterModel.GetFilterModelsRecursive(brushQueryModel, new List<QueryModel>(), filterModels, false);
                brushes.Add(brush);
            }

            List<double> nrOfBins = new List<double>();

            nrOfBins = new double[] {MainViewController.Instance.MainModel.NrOfXBins, MainViewController.Instance.MainModel.NrOfYBins}.Concat(
                QueryModel.GetUsageInputOperationModel(InputUsage.Group).Select(qom => MainViewController.Instance.MainModel.NrOfGroupBins)).ToList();


            dimensionAggregateFunctions = QueryModelClone.GetUsageInputOperationModel(InputUsage.X).Select(iom => iom.AggregateFunction.ToString()).Concat(
                     QueryModelClone.GetUsageInputOperationModel(InputUsage.Y).Select(iom => iom.AggregateFunction.ToString())).Concat(
                     QueryModelClone.GetUsageInputOperationModel(InputUsage.Group).Select(iom => iom.AggregateFunction.ToString())).ToList();


            if (QueryModelClone.GetUsageInputOperationModel(InputUsage.X).First().InputModel.RawName == "long" ||
                QueryModelClone.GetUsageInputOperationModel(InputUsage.X).First().InputModel.RawName == "lat")
            {
                nrOfBins[0] = 20;
            }

            if (QueryModelClone.GetUsageInputOperationModel(InputUsage.Y).First().InputModel.RawName == "long" ||
               QueryModelClone.GetUsageInputOperationModel(InputUsage.Y).First().InputModel.RawName == "lat")
            {
                nrOfBins[0] = 20;
            }

            dimensions = QueryModelClone.GetUsageInputOperationModel(InputUsage.X).Select(iom => iom.InputModel.RawName).Concat(
                                 QueryModelClone.GetUsageInputOperationModel(InputUsage.Y).Select(iom => iom.InputModel.RawName)).Concat(
                                 QueryModelClone.GetUsageInputOperationModel(InputUsage.Group).Select(iom => iom.InputModel.RawName)).ToList();

            var aggregates = QueryModelClone.GetUsageInputOperationModel(InputUsage.Value).Concat(
                 QueryModelClone.GetUsageInputOperationModel(InputUsage.DefaultValue)).Concat(
                 QueryModelClone.GetUsageInputOperationModel(InputUsage.X).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Concat(
                 QueryModelClone.GetUsageInputOperationModel(InputUsage.Y).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Distinct().ToList();

            aggregateDimensions = aggregates.Select(iom => iom.InputModel.RawName).ToList();
            aggregateFunctions = aggregates.Select(iom => iom.AggregateFunction.ToString()).ToList();

            var xIom = QueryModelClone.GetUsageInputOperationModel(InputUsage.X).FirstOrDefault();
            var yIom = QueryModelClone.GetUsageInputOperationModel(InputUsage.Y).FirstOrDefault();

            BinningParameters xBinning = xIom.AggregateFunction == AggregateFunction.None
                ? (BinningParameters) new EquiWidthBinningParameters()
                {
                    Dimension = xIom.InputModel.RawName,
                    RequestedNrOfBins = MainViewController.Instance.MainModel.NrOfXBins,
                }
                : (BinningParameters) new SingleBinBinningParameters()
                {
                    Dimension = xIom.InputModel.RawName,
                };


            BinningParameters yBinning = yIom.AggregateFunction == AggregateFunction.None
                ? (BinningParameters)new EquiWidthBinningParameters()
                {
                    Dimension = yIom.InputModel.RawName,
                    RequestedNrOfBins = MainViewController.Instance.MainModel.NrOfYBins,
                }
                : (BinningParameters)new SingleBinBinningParameters()
                {
                    Dimension = yIom.InputModel.RawName,
                };

            _query = new HistogramOperationParameters()
            {
                AdapterName = psm.RootOriginModel.DatasetConfiguration.Schema.RawName,
                Filter = filter,
                Brushes = brushes,
                BinningParameters = IDEA_common.util.Extensions.Yield(xBinning, yBinning).ToList(),
                SampleStreamBlockSize = sampleSize
            };
        }
        public override void Start()
        {
            _stopWatch.Start();
            lock (_lock)
            {
                _isRunning = true;
            }
            Task.Run(() => run());
        }

        private async void run()
        {
            try
            {
                string response = await ProgressiveGateway.Request(JsonConvert.SerializeObject(_query, ProgressiveGateway.JsonSerializerSettings), "operation");
                _operationReference = JsonConvert.DeserializeObject<IOperationReference>(response, ProgressiveGateway.JsonSerializerSettings);

                // starting looping for updates
                while (_isRunning)
                {
                    string message = await ProgressiveGateway.Request(JsonConvert.SerializeObject(_operationReference, ProgressiveGateway.JsonSerializerSettings), "result");
                    if (message != "null")
                    {
                        HistogramResult result = (HistogramResult)JsonConvert.DeserializeObject<IResult>(message, ProgressiveGateway.JsonSerializerSettings);

                        var aggregates = QueryModelClone.GetUsageInputOperationModel(InputUsage.Value).Concat(
                            QueryModelClone.GetUsageInputOperationModel(InputUsage.DefaultValue)).Concat(
                                QueryModelClone.GetUsageInputOperationModel(InputUsage.X).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Concat(
                                    QueryModelClone.GetUsageInputOperationModel(InputUsage.Y).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Distinct().ToList();

                        var aggregateDimensions = aggregates.Select(iom => iom.InputModel.RawName).ToList();
                        var aggregateFunctions = aggregates.Select(iom => iom.AggregateFunction.ToString()).ToList();

                        var dimensions = QueryModelClone.GetUsageInputOperationModel(InputUsage.X).Concat(
                            QueryModelClone.GetUsageInputOperationModel(InputUsage.Y)).Concat(
                                QueryModelClone.GetUsageInputOperationModel(InputUsage.Group)).ToList();

                        var axisTypes = dimensions.Select(d => QueryModelClone.GetAxisType(d)).ToList();

                        List<string> brushes = new List<string>();
                        foreach (var brushQueryModel in QueryModelClone.BrushQueryModels)
                        {
                            List<FilterModel> filterModels = new List<FilterModel>();
                            var brush = FilterModel.GetFilterModelsRecursive(brushQueryModel, new List<QueryModel>(), filterModels, false);
                            brushes.Add(brush);
                        }

                        VisualizationResultDescriptionModel resultDescriptionModel = new VisualizationResultDescriptionModel();
                        //List<ResultItemModel> resultItemModels = UpdateVisualizationResultDescriptionModel(resultDescriptionModel, result, brushes, dimensions, axisTypes, aggregates);
                        double progress = 0;//(double) result["progress"];

                       // await fireUpdated(resultItemModels, progress, resultDescriptionModel);

                        if (progress >= 1.0)
                        {
                            Stop();
                            await fireCompleted();
                        }
                    }
                    await Task.Delay(_throttle);
                }

            }
            catch (Exception exc)
            {
                ErrorHandler.HandleError(exc.Message);
            }
        }

        public static List<ResultItemModel> UpdateVisualizationResultDescriptionModel(VisualizationResultDescriptionModel resultDescriptionModel, 
            JObject result, List<string> brushes, 
            List<InputOperationModel> dimensions, List<AxisType> axisTypes, List<InputOperationModel> aggregates)
        {
            resultDescriptionModel.BinRanges = new List<BinRange>();
            List<ResultItemModel> resultItemModels = new List<ResultItemModel>();
            double progress = (double)result["progress"];
            JObject binStruct = (JObject)result["binStructure"];
            double nullCount = (double)binStruct["nullCount"];
            resultDescriptionModel.NullCount = nullCount;
            resultDescriptionModel.OverallMeans = binStruct["overallMeans"].ToObject<Dictionary<string, double>>();
            resultDescriptionModel.OverallCount = binStruct["overallCount"].ToObject<Dictionary<string, double>>();
            resultDescriptionModel.OverallSumOfSquares = binStruct["overallSumOfSquares"].ToObject<Dictionary<string, double>>();
            resultDescriptionModel.OverallSampleStandardDeviations = binStruct["overallSampleStandardDeviations"].ToObject<Dictionary<string, double>>();
            JObject bins = (JObject)binStruct["bins"];
            JArray binRanges = (JArray)binStruct["binRanges"];

            List<BrushIndex> brushIndices = new List<BrushIndex>();
            for (int b = 0; b < brushes.Count; b++)
            {
                brushIndices.Add(new BrushIndex(b.ToString()));
            }
            brushIndices.Add(BrushIndex.OVERLAP);
            brushIndices.Add(BrushIndex.ALL);
            resultDescriptionModel.BrushIndices = brushIndices;

            var binRangeBins = new List<List<double>>();

            foreach (var binRange in binRanges)
            {
                if (binRange["type"].ToString() == "QuantitativeBinRange")
                {
                    var qbr = new QuantitativeBinRange()
                    {
                        DataMaxValue = (double)binRange["dataMaxValue"],
                        DataMinValue = (double)binRange["dataMinValue"],
                        MaxValue = (double)binRange["maxValue"],
                        MinValue = (double)binRange["minValue"],
                        Step = (double)binRange["step"],
                        IsIntegerRange = (bool)binRange["isIntegerRange"],
                        TargetBinNumber = (double)binRange["targetBinNumber"]
                    };
                    resultDescriptionModel.BinRanges.Add(qbr);
                    binRangeBins.Add(qbr.GetBins());
                }
                else if (binRange["type"].ToString() == "AggregatedBinRange")
                {
                    resultDescriptionModel.BinRanges.Add(new AggregateBinRange());
                    binRangeBins.Add(new List<double>());
                }
                else if (binRange["type"].ToString() == "NominalBinRange")
                {
                    var valuesLabel = binRange["valuesLabel"];
                    var nbr = new NominalBinRange();
                    resultDescriptionModel.BinRanges.Add(nbr);
                    var count = 0;
                    foreach (var token in valuesLabel)
                    {
                        var prop = (JProperty)token;
                        nbr.LabelsValue.Add(double.Parse(prop.Name.ToString()), prop.Value.ToString());
                        count++;
                        if (count > 20)
                        {
                            //break
                        }
                    }
                    nbr.MaxValue = nbr.LabelsValue.Count-1;
                    nbr.Step = 1;

                    binRangeBins.Add(nbr.GetBins());
                }
            }

            foreach (var bin in bins)
            {

                var marginsAbsolute = (JObject)(bin.Value)["marginsAbsolute"];
                var counts = (JObject)(bin.Value)["counts"];
                var values = (JObject)(bin.Value)["values"];
                var countsInterpolated = (JObject)(bin.Value)["countsInterpolated"];
                var margins = (JObject)(bin.Value)["margins"];

                ProgressiveVisualizationResultItemModel resultItem = new ProgressiveVisualizationResultItemModel();
                foreach (var inputOperationModel in aggregates)
                {
                    for (int b = 0; b < brushIndices.Count; b++)
                    {
                        updateItemResultModel(resultItem, inputOperationModel, brushIndices[b], b, marginsAbsolute, margins, counts, values, countsInterpolated);
                    }
                }

                var span = bin.Key.ToString().Replace("[", "").Replace("]", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(r => int.Parse(r.Trim())).ToList();
                for (int d = 0; d < span.Count; d++)
                {
                    if (!(resultDescriptionModel.BinRanges[d] is AggregateBinRange))
                    {
                        resultItem.AddValue(dimensions[d], BrushIndex.ALL, binRangeBins[d][span[d]]);
                    }
                }
                resultItemModels.Add(resultItem);
            }

            resultDescriptionModel.Dimensions = dimensions;
            resultDescriptionModel.AxisTypes = axisTypes;
            resultDescriptionModel.MinValues = new Dictionary<InputOperationModel, Dictionary<BrushIndex, double>>();
            resultDescriptionModel.MaxValues = new Dictionary<InputOperationModel, Dictionary<BrushIndex, double>>();
            //resultDescriptionModel.MinValues = aggregates.Select(a => re).
            foreach (var dim in aggregates)
            {
                resultDescriptionModel.MinValues.Add(dim, new Dictionary<BrushIndex, double>());
                resultDescriptionModel.MaxValues.Add(dim, new Dictionary<BrushIndex, double>());
                foreach (var b in brushIndices)
                {
                    resultDescriptionModel.MinValues[dim].Add(b, resultItemModels.Select(rim => (double)((ProgressiveVisualizationResultItemModel)rim).Values[dim][b]).Min());
                    resultDescriptionModel.MaxValues[dim].Add(b, resultItemModels.Select(rim => (double)((ProgressiveVisualizationResultItemModel)rim).Values[dim][b]).Max());
                }

            }
            return resultItemModels;
        }

        private static void updateItemResultModel(ProgressiveVisualizationResultItemModel resultItem, InputOperationModel iom, BrushIndex brushIndex, int brushIntIndex, JObject marginsAbsolute, JObject margins, JObject counts, JObject values, JObject countsInterpolated)
        {
            resultItem.AddCount(iom, brushIndex, getValue(counts, iom, brushIntIndex));
            resultItem.AddMargin(iom, brushIndex, getValue(margins, iom, brushIntIndex));
            resultItem.AddMarginAbsolute(iom, brushIndex, getValue(marginsAbsolute, iom, brushIntIndex));
            resultItem.AddValue(iom, brushIndex, getValue(values, iom, brushIntIndex));
            resultItem.AddCountInterpolated(iom, brushIndex, getValue(countsInterpolated, iom, brushIntIndex));
        }

        private static double getValue(JObject dictionary, InputOperationModel iom, int brushIndex)
        {
            return (double) dictionary[iom.InputModel.RawName][iom.AggregateFunction.ToString()][brushIndex.ToString()];
        }

        private async Task fireUpdated(List<ResultItemModel> samples, double progress, ResultDescriptionModel resultDescriptionModel)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                FireJobUpdated(new JobEventArgs()
                {
                    Samples = samples,
                    Progress = progress,
                    ResultDescriptionModel = resultDescriptionModel
                });
            });
        }

        private async Task fireCompleted()
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                FireJobCompleted(new EventArgs());
            });
        }


        public override void Stop()
        {
            lock (_lock)
            {
                _isRunning = false;

                //JObject lookupData = new JObject(
                //   new JProperty("type", "halt"),
                //   new JProperty("uuid", _requestUuid));
                //ProgressiveGateway.Request(lookupData);
            }
        }
    }
}
