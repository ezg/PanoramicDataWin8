using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Composition;
using Windows.UI.Core;
using IDEA_common.operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.data;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.progressive;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class ProgressiveClassifyJob : Job
    {
        private Object _lock = new Object();
        private bool _isRunning = false;

        private string _requestUuid = "";
        private int _sampleSize = 0;
        private Stopwatch _stopWatch = new Stopwatch();
        private JObject _query = null;

        private TimeSpan _throttle = TimeSpan.FromMilliseconds(0);
        public HistogramOperationModel HistogramOperationModel { get; set; }
        public HistogramOperationModel HistogramOperationModelClone { get; set; }

        public ProgressiveClassifyJob(HistogramOperationModel histogramOperationModel, HistogramOperationModel histogramOperationModelClone, TimeSpan throttle, int sampleSize)
        {
           /* HistogramOperationModel = histogramOperationModel;
            HistogramOperationModelClone = histogramOperationModelClone;
            _sampleSize = sampleSize;
            _throttle = throttle;
            var psm = (histogramOperationModelClone.SchemaModel as ProgressiveSchemaModel);

            var features = HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Feature).Select(iom => iom.AttributeModel.RawName).ToList();

            string filter = "";
            List<FilterModel> filterModels = new List<FilterModel>();
            filter = FilterModel.GetFilterModelsRecursive(HistogramOperationModelClone, new List<HistogramOperationModel>(), filterModels, true);

            List<string> brushes = new List<string>();
            foreach (var brushQueryModel in HistogramOperationModelClone.BrushQueryModels)
            {
                filterModels = new List<FilterModel>();
                var brush = FilterModel.GetFilterModelsRecursive(brushQueryModel, new List<HistogramOperationModel>(), filterModels, false);
                brushes.Add(brush);
            }

            _query = new JObject(
                new JProperty("type", "execute"),
                new JProperty("dataset", psm.RootOriginModel.DatasetConfiguration.Schema.RawName),
                new JProperty("task",
                    new JObject(
                        new JProperty("type", "classify"),
                        new JProperty("filter", filter),
                        new JProperty("chunkSize", sampleSize),
                        new JProperty("classifier", HistogramOperationModelClone.TaskModel.Name),
                        new JProperty("label", brushes[0]),
                        new JProperty("features", features)
                    ))
                );*/
        }
        public override void Start()
        {
            _stopWatch.Start();
            Task.Run(() => run());
            lock (_lock)
            {
                _isRunning = true;
            }
        }


        private async void run()
        {
            try
            {
                string response = null;//await ProgressiveGateway.Request(_query);
                JObject dict = JObject.Parse(response);
                _requestUuid = dict["uuid"].ToString();

                // starting looping for updates
                while (_isRunning)
                {

                    // starting looping for updates
                    while (_isRunning)
                    {
                        JObject lookupData = new JObject(
                            new JProperty("type", "lookup"),
                            new JProperty("uuid", _requestUuid));
                        string message = null;;//await ProgressiveGateway.Request(lookupData);

                        if (message != "None" && message != "null" && message != "\"None\"")
                        {
                            /*List<string> brushes = new List<string>();
                            foreach (var brushQueryModel in HistogramOperationModelClone.BrushQueryModels)
                            {
                                List<FilterModel> filterModels = new List<FilterModel>();
                                var brush = FilterModel.GetFilterModelsRecursive(brushQueryModel, new List<HistogramOperationModel>(), filterModels, false);
                                brushes.Add(brush);
                            }
                            var label = brushes[0];

                            ClassfierResultDescriptionModel resultDescriptionModel = new ClassfierResultDescriptionModel();
                            resultDescriptionModel.Uuid = _requestUuid;
                            JObject result = JObject.Parse(message);
                            double progress = (double)result["progress"];
                            */
                            var features = HistogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Feature).ToList();
                            foreach (var feature in features)
                            {
                                //['actual and predicted', 'not actual and predicted', 'not actual and not predicted', 'actual and not predicted']
                                List<string> visBrushes = new List<string>() { "0", "1", "2", "3" };

                                /*JObject token = (JObject)result["histograms"][feature.AttributeModel.RawName];
                                VisualizationResultDescriptionModel visResultDescriptionModel = new VisualizationResultDescriptionModel();
                                List<ResultItemModel> resultItemModels = ProgressiveVisualizationJob.UpdateVisualizationResultDescriptionModel(visResultDescriptionModel, token, visBrushes,
                                    new List<AttributeOperationModel>()
                                    {
                                new AttributeOperationModel(feature.AttributeModel)
                                {
                                    AggregateFunction = AggregateFunction.None
                                },
                                new AttributeOperationModel(feature.AttributeModel)
                                {
                                    AggregateFunction = AggregateFunction.Count
                                }
                                    }, new List<AxisType>() { AxisType.Quantitative, AxisType.Quantitative }, new List<AttributeOperationModel>()
                                    {
                                new AttributeOperationModel(feature.AttributeModel)
                                {
                                    AggregateFunction = AggregateFunction.Count
                                }
                                    });
                                resultDescriptionModel.VisualizationResultModel.Add(new ResultModel()
                                {
                                    Progress = progress,
                                    ResultDescriptionModel = visResultDescriptionModel,
                                    ResultItemModels = new ObservableCollection<ResultItemModel>(resultItemModels)
                                });*/
                            }

                            /*var classifyResult = JsonConvert.DeserializeObject<ClassifyResult>(result[label].ToString());


                            resultDescriptionModel.ConfusionMatrices.Add(new List<double>());
                            resultDescriptionModel.ConfusionMatrices[0].Add((double)classifyResult.tp);
                            resultDescriptionModel.ConfusionMatrices[0].Add((double)classifyResult.fn);

                            resultDescriptionModel.ConfusionMatrices.Add(new List<double>());
                            resultDescriptionModel.ConfusionMatrices[1].Add((double)classifyResult.fp);
                            resultDescriptionModel.ConfusionMatrices[1].Add((double)classifyResult.tn);

                            var xs = classifyResult.fpr;
                            var ys = classifyResult.tpr;
                            resultDescriptionModel.RocCurve = new List<Pt>();
                            resultDescriptionModel.RocCurve.Add(new Pt(0, 0));
                            var step = 1; //ys.Count() > 300 ? 50 : 1;  

                            if (xs != null && ys != null)
                            {
                                for (int i = 0; i < xs.Count(); i += step)
                                {
                                    resultDescriptionModel.RocCurve.Add(new Pt((double)xs[i], (double)ys[i]));
                                }
                                resultDescriptionModel.RocCurve.Add(new Pt(1, 1));
                            }

                            resultDescriptionModel.Precision = classifyResult.precision;
                            resultDescriptionModel.Recall = classifyResult.recall;
                            resultDescriptionModel.AUC = classifyResult.auc;
                            resultDescriptionModel.F1s = classifyResult.f1;
                            resultDescriptionModel.Progresses = classifyResult.progress;


                            await fireUpdated(new List<ResultItemModel>(), progress, resultDescriptionModel);*/

                           // if (progress >= 1.0)
                            {
                                Stop();
                                //await fireCompleted();
                            }
                        }

                        await Task.Delay(_throttle);
                    }

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
                _isRunning = false;

                JObject lookupData = new JObject(
                               new JProperty("type", "halt"),
                               new JProperty("uuid", _requestUuid));
                //ProgressiveGateway.Request(lookupData);
            }
        }
    }
}
