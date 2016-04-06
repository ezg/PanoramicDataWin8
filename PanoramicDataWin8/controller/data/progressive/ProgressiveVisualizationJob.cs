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
        private MessageWebSocket _webSocket = null;
        private int _sampleSize = 0;
        private Stopwatch _stopWatch = new Stopwatch();
        private JObject _query = null;

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

            List<string> aggregateFunctions = new List<string>();
            List<string> aggregateDimensions = new List<string>();
            List<string> dimensionAggregateFunctions = new List<string>();
            List<string> dimensions = new List<string>();
            List<string> brushes = new List<string>();
            List<double> nrOfBins = new List<double>();

            nrOfBins = new double[] {MainViewController.Instance.MainModel.NrOfXBins, MainViewController.Instance.MainModel.NrOfYBins}.Concat(
                QueryModel.GetUsageInputOperationModel(InputUsage.Group).Select(qom => MainViewController.Instance.MainModel.NrOfGroupBins)).ToList();

            dimensionAggregateFunctions = QueryModelClone.GetUsageInputOperationModel(InputUsage.X).Select(iom => iom.AggregateFunction.ToString()).Concat(
                     QueryModelClone.GetUsageInputOperationModel(InputUsage.Y).Select(iom => iom.AggregateFunction.ToString())).Concat(
                     QueryModelClone.GetUsageInputOperationModel(InputUsage.Group).Select(iom => iom.AggregateFunction.ToString())).ToList();

            dimensions = QueryModelClone.GetUsageInputOperationModel(InputUsage.X).Select(iom => iom.InputModel.Name).Concat(
                                 QueryModelClone.GetUsageInputOperationModel(InputUsage.Y).Select(iom => iom.InputModel.Name)).Concat(
                                 QueryModelClone.GetUsageInputOperationModel(InputUsage.Group).Select(iom => iom.InputModel.Name)).ToList();

            var aggregates = QueryModelClone.GetUsageInputOperationModel(InputUsage.Value).Concat(
                 QueryModelClone.GetUsageInputOperationModel(InputUsage.DefaultValue)).Concat(
                 QueryModelClone.GetUsageInputOperationModel(InputUsage.X).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Concat(
                 QueryModelClone.GetUsageInputOperationModel(InputUsage.Y).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Distinct().ToList();

            aggregateDimensions = aggregates.Select(iom => iom.InputModel.Name).ToList();
            aggregateFunctions = aggregates.Select(iom => iom.AggregateFunction.ToString()).ToList();

            _query = new JObject(
                new JProperty("type", "execute"),
                new JProperty("dataset", psm.RootOriginModel.DatasetConfiguration.Name),
                new JProperty("task",
                    new JObject(
                        new JProperty("filter", filter),
                        new JProperty("aggregateFunctions", aggregateFunctions),
                        new JProperty("type", "visualization"),
                        new JProperty("chunkSize", sampleSize),
                        new JProperty("aggregateDimensions", aggregateDimensions),
                        new JProperty("nrOfBins", nrOfBins),
                        new JProperty("brushes", brushes),
                        new JProperty("dimensionAggregateFunctions", dimensionAggregateFunctions),
                        new JProperty("dimensions", dimensions)
                    ))
                );
        }
        public override void Start()
        {
            _stopWatch.Start();
            run();
            // Task.Run(() => run());
        }

        private async void run()
        {
            _webSocket = new MessageWebSocket();
            _webSocket.Control.MessageType = SocketMessageType.Utf8;
            _webSocket.MessageReceived += webSocket_MessageReceived;

            _webSocket.Closed += webSocket_Closed;
            await _webSocket.ConnectAsync(new Uri(MainViewController.Instance.MainModel.Ip));

            var data = _query.ToString();
            DataWriter messageWriter = new DataWriter(_webSocket.OutputStream);
            messageWriter.WriteString(data);
            await messageWriter.StoreAsync();

        }

        void webSocket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {   
        }

        async void webSocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            using (DataReader reader = args.GetDataReader())
            {
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                string message = reader.ReadString(reader.UnconsumedBufferLength);
                

                JObject result = JObject.Parse(message);

                var aggregates = QueryModelClone.GetUsageInputOperationModel(InputUsage.Value).Concat(
                    QueryModelClone.GetUsageInputOperationModel(InputUsage.DefaultValue)).Concat(
                        QueryModelClone.GetUsageInputOperationModel(InputUsage.X).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Concat(
                            QueryModelClone.GetUsageInputOperationModel(InputUsage.Y).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Distinct().ToList();

                var aggregateDimensions = aggregates.Select(iom => iom.InputModel.Name).ToList();
                var aggregateFunctions = aggregates.Select(iom => iom.AggregateFunction.ToString()).ToList();

                var dimensions = QueryModelClone.GetUsageInputOperationModel(InputUsage.X).Concat(
                                 QueryModelClone.GetUsageInputOperationModel(InputUsage.Y)).Concat(
                                 QueryModelClone.GetUsageInputOperationModel(InputUsage.Group)).ToList();

                var axisTypes = dimensions.Select(d => QueryModelClone.GetAxisType(d)).ToList();

                var brushes = new List<string>();

                VisualizationResultDescriptionModel resultDescriptionModel = new VisualizationResultDescriptionModel();
                resultDescriptionModel.BinRanges = new List<BinRange>();
                List<ResultItemModel> resultItemModels = new List<ResultItemModel>();
                double progress = (double) result["progress"];
                JObject binStruct = (JObject) result["binStructure"];
                double nullCount = (double) binStruct["nullCount"];
                JObject bins = (JObject)binStruct["bins"];
                JArray binRanges = (JArray)binStruct["binRanges"];

                List<BrushIndex> brushIndices = new List<BrushIndex>();
                for (int b = 0; b < brushes.Count; b++)
                {
                    brushIndices.Add(new BrushIndex(b.ToString()));
                }
                brushIndices.Add(BrushIndex.OVERLAP);
                brushIndices.Add(BrushIndex.ALL);

                var binRangeBins = new List<List<double>>();

                foreach (var binRange in binRanges)
                {
                    if (binRange["type"].ToString() == "QuantitativeBinRange")
                    {
                        var qbr = new QuantitativeBinRange()
                        {
                            DataMaxValue = (double) binRange["dataMaxValue"],
                            DataMinValue = (double) binRange["dataMinValue"],
                            MaxValue = (double) binRange["maxValue"],
                            MinValue = (double) binRange["minValue"],
                            Step = (double) binRange["step"],
                            IsIntegerRange = (bool) binRange["isIntegerRange"],
                            TargetBinNumber = (double) binRange["targetBinNumber"]
                        };
                        resultDescriptionModel.BinRanges.Add(qbr);
                        binRangeBins.Add(qbr.GetBins());
                    }
                    else if (binRange["type"].ToString() == "AggregatedBinRange")
                    {
                        resultDescriptionModel.BinRanges.Add(new AggregateBinRange());
                        binRangeBins.Add(new List<double>());
                    }
                }

                foreach (var bin in bins)
                {

                    var marginsAbsolute = (JObject) (bin.Value)["marginsAbsolute"];
                    var counts = (JObject) (bin.Value)["counts"];
                    var values = (JObject) (bin.Value)["values"];
                    var countsInterpolated = (JObject) (bin.Value)["countsInterpolated"];
                    var margins = (JObject) (bin.Value)["margins"];

                    ProgressiveVisualizationResultItemModel resultItem = new ProgressiveVisualizationResultItemModel();
                    foreach (var inputOperationModel in aggregates)
                    {
                        for (int b = 0; b < brushIndices.Count; b++)
                        {
                            updateItemResultModel(resultItem, inputOperationModel, brushIndices[b], b, marginsAbsolute, margins, counts, values, countsInterpolated);
                        }
                    }

                    var span = bin.Key.ToString().Replace("[", "").Replace("]", "").Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(r => int.Parse(r.Trim())).ToList();
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
                await fireUpdated(resultItemModels, progress, resultDescriptionModel);

                if (progress == 1.0)
                {
                    await fireCompleted();
                }
                /* if (_binner != null && _binner.BinStructure != null)
                 {
                     resultItemModels = convertBinsToResultItemModels(_binner.BinStructure);
                     resultDescriptionModel = new VisualizationResultDescriptionModel()
                     {
                         BinRanges = _binner.BinStructure.BinRanges,
                         NullCount = _binner.BinStructure.NullCount,
                         Dimensions = _dimensions,
                         AxisTypes = _axisTypes,
                         MinValues = _binner.BinStructure.AggregatedMinValues.ToDictionary(entry => entry.Key, entry => entry.Value),
                         MaxValues = _binner.BinStructure.AggregatedMaxValues.ToDictionary(entry => entry.Key, entry => entry.Value)
                     };

                     await fireUpdated(resultItemModels, _dataProvider.Progress(), resultDescriptionModel);
                 }*/
            }
        }

        private void updateItemResultModel(ProgressiveVisualizationResultItemModel resultItem, InputOperationModel iom, BrushIndex brushIndex, int brushIntIndex, JObject marginsAbsolute, JObject margins, JObject counts, JObject values, JObject countsInterpolated)
        {
            resultItem.AddCount(iom, brushIndex, getValue(counts, iom, brushIntIndex));
            resultItem.AddMargin(iom, brushIndex, getValue(margins, iom, brushIntIndex));
            resultItem.AddMarginAbsolute(iom, brushIndex, getValue(marginsAbsolute, iom, brushIntIndex));
            resultItem.AddValue(iom, brushIndex, getValue(values, iom, brushIntIndex));
            resultItem.AddCountInterpolated(iom, brushIndex, getValue(countsInterpolated, iom, brushIntIndex));
        }

        private double getValue(JObject dictionary, InputOperationModel iom, int brushIndex)
        {
            return (double) dictionary[iom.InputModel.Name][iom.AggregateFunction.ToString()][brushIndex.ToString()];
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
            if (_webSocket != null)
            {
                _webSocket.Close(1000, "");
                _webSocket = null;
            }
        }
    }
}
