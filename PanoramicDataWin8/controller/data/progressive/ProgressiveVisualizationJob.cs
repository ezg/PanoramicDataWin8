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
using IDEA_common.aggregates;
using IDEA_common.binning;
using IDEA_common.operations;
using IDEA_common.operations.histogram;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
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
        public HistogramOperationModel HistogramOperationModel { get; set; }
        public HistogramOperationModel HistogramOperationModelClone { get; set; }

        public ProgressiveVisualizationJob(HistogramOperationModel histogramOperationModel, HistogramOperationModel histogramOperationModelClone, TimeSpan throttle, int sampleSize)
        {
            HistogramOperationModel = histogramOperationModel;
            HistogramOperationModelClone = histogramOperationModelClone;
            _sampleSize = sampleSize;
            _throttle = throttle;
            var psm = (histogramOperationModelClone.SchemaModel as ProgressiveSchemaModel);
            string filter = "";
            List<FilterModel> filterModels = new List<FilterModel>();
            filter = FilterModel.GetFilterModelsRecursive(HistogramOperationModelClone, new List<IFilterProviderOperationModel>(), filterModels, true);
            
            List<string> aggregateFunctions = new List<string>();
            List<string> aggregateDimensions = new List<string>();
            List<string> dimensionAggregateFunctions = new List<string>();
            List<string> dimensions = new List<string>();
            List<string> brushes = new List<string>();


            foreach (var brushOperationModel in HistogramOperationModelClone.BrushOperationModels)
            {
                filterModels = new List<FilterModel>();
                var brush = FilterModel.GetFilterModelsRecursive(brushOperationModel, new List<IFilterProviderOperationModel>(), filterModels, false);
                brushes.Add(brush);
            }

            List<double> nrOfBins = new List<double>();

            nrOfBins = new double[] {MainViewController.Instance.MainModel.NrOfXBins, MainViewController.Instance.MainModel.NrOfYBins}.Concat(
                HistogramOperationModel.GetUsageAttributeTransformationModel(InputUsage.Group).Select(qom => MainViewController.Instance.MainModel.NrOfGroupBins)).ToList();


            dimensionAggregateFunctions = HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.X).Select(iom => iom.AggregateFunction.ToString()).Concat(
                     HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Y).Select(iom => iom.AggregateFunction.ToString())).Concat(
                     HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Group).Select(iom => iom.AggregateFunction.ToString())).ToList();


            if (HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.X).First().AttributeModel.RawName == "long" ||
                HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.X).First().AttributeModel.RawName == "lat")
            {
                nrOfBins[0] = 20;
            }

            if (HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Y).First().AttributeModel.RawName == "long" ||
               HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Y).First().AttributeModel.RawName == "lat")
            {
                nrOfBins[0] = 20;
            }

            dimensions = HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.X).Select(iom => iom.AttributeModel.RawName).Concat(
                                 HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Y).Select(iom => iom.AttributeModel.RawName)).Concat(
                                 HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Group).Select(iom => iom.AttributeModel.RawName)).ToList();

            var aggregates = HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Value).Concat(
                 HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.DefaultValue)).Concat(
                 HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.X).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Concat(
                 HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Y).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Distinct().ToList();

            aggregateDimensions = aggregates.Select(iom => iom.AttributeModel.RawName).ToList();
            aggregateFunctions = aggregates.Select(iom => iom.AggregateFunction.ToString()).ToList();

            var xIom = HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.X).FirstOrDefault();
            var yIom = HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Y).FirstOrDefault();

            BinningParameters xBinning = xIom.AggregateFunction == AggregateFunction.None
                ? (BinningParameters) new EquiWidthBinningParameters()
                {
                    Dimension = xIom.AttributeModel.Index,
                    RequestedNrOfBins = MainViewController.Instance.MainModel.NrOfXBins,
                }
                : (BinningParameters) new SingleBinBinningParameters()
                {
                    Dimension = xIom.AttributeModel.Index,
                };


            BinningParameters yBinning = yIom.AggregateFunction == AggregateFunction.None
                ? (BinningParameters)new EquiWidthBinningParameters()
                {
                    Dimension = yIom.AttributeModel.Index,
                    RequestedNrOfBins = MainViewController.Instance.MainModel.NrOfYBins,
                }
                : (BinningParameters)new SingleBinBinningParameters()
                {
                    Dimension = yIom.AttributeModel.Index,
                };

            var aggregateParameters = new List<AggregateParameters>();
            foreach (var agg in aggregates)
            {
                if (agg.AggregateFunction == AggregateFunction.Avg)
                {
                    aggregateParameters.Add(new AverageAggregateParameters()
                    {
                        Dimension = agg.AttributeModel.Index
                    });
                }
                else if (agg.AggregateFunction == AggregateFunction.Count)
                {
                    aggregateParameters.Add(new CountAggregateParameters()
                    {
                        Dimension = agg.AttributeModel.Index
                    });
                }

                aggregateParameters.Add(new MarginAggregateParameters()
                {
                    Dimension = agg.AttributeModel.Index,
                    AggregateFunction = agg.AggregateFunction.ToString()
                });
            }

            _query = new HistogramOperationParameters()
            {
                AdapterName = psm.RootOriginModel.DatasetConfiguration.Schema.RawName,
                Filter = filter,
                Brushes = brushes,
                BinningParameters = IDEA_common.util.Extensions.Yield(xBinning, yBinning).ToList(),
                SampleStreamBlockSize = sampleSize,
                PerBinAggregateParameters = aggregateParameters
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
                _operationReference = JsonConvert.DeserializeObject<OperationReference>(response, ProgressiveGateway.JsonSerializerSettings);

                // starting looping for updates
                while (_isRunning)
                {
                    string message = await ProgressiveGateway.Request(JsonConvert.SerializeObject(_operationReference, ProgressiveGateway.JsonSerializerSettings), "result");
                    if (message != "null")
                    {
                        HistogramResult result = (HistogramResult)JsonConvert.DeserializeObject<HistogramResult>(message, ProgressiveGateway.JsonSerializerSettings);

                        await fireUpdated(result);

                        if (result.Progress >= 1.0)
                        {
                            Stop();
                            await fireCompleted(result);
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
        

        private async Task fireUpdated(IResult result)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                FireJobUpdated(new JobEventArgs()
                {
                    Result = result
                });
            });
        }

        private async Task fireCompleted(IResult result)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                FireJobCompleted(new JobEventArgs()
                {
                    Result = result
                });
            });
        }


        public override void Stop()
        {
            lock (_lock)
            {
                ProgressiveGateway.Request(JsonConvert.SerializeObject(_operationReference, ProgressiveGateway.JsonSerializerSettings), "pause");
                _isRunning = false;
                //JObject lookupData = new JObject(
                //   new JProperty("type", "halt"),
                //   new JProperty("uuid", _requestUuid));
                //ProgressiveGateway.Request(lookupData);
            }
        }
    }
}
