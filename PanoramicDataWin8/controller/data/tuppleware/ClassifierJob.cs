using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.data.tuppleware.gateway;
using PanoramicDataWin8.controller.data.tuppleware.json;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.data.tuppleware;
using PanoramicDataWin8.utils;
using SharpDX.WIC;

namespace PanoramicDataWin8.controller.data.tuppleware
{
    public class ClassifierJob : Job
    {
        private Stopwatch _stopWatch = new Stopwatch();
        private TimeSpan _throttle = TimeSpan.FromMilliseconds(0);
        private TuppleWareOriginModel _originModel = null;
        private QueryModel _queryModelClone = null;

        public QueryModel QueryModel { get; set; }

        public ClassifierJob(QueryModel queryModel, QueryModel queryModelClone, TuppleWareOriginModel orignModel, TimeSpan throttle)
        {
            QueryModel = queryModel;
            _queryModelClone = queryModelClone;
            _originModel = orignModel;
            _throttle = throttle;
        }

        public override void Start()
        {
            _stopWatch.Start();
            Task.Run(() => run());
        }

        private async void run()
        {
            if (_throttle.Ticks > 0)
            {
                await Task.Delay(_throttle);
            }
            try
            {
                List<InputFieldModel> labels = new List<InputFieldModel>();
                getInputFieldModelsRecursive(_queryModelClone.GetUsageInputOperationModel(InputUsage.Label).Select(iom => iom.InputModel).ToList(), labels);
                labels = labels.Distinct().ToList();

                List<InputFieldModel> features = new List<InputFieldModel>();
                getInputFieldModelsRecursive(_queryModelClone.GetUsageInputOperationModel(InputUsage.Feature).Select(iom => iom.InputModel).ToList(), features);
                features = features.Distinct().ToList();

                Dictionary<InputFieldModel, string> labelsUuid = new Dictionary<InputFieldModel, string>();
                Dictionary<InputFieldModel, string> classifysUuid = new Dictionary<InputFieldModel, string>();

                ProjectCommand projectCommand = new ProjectCommand();
                ClassifyCommand classifyCommand = new ClassifyCommand();
                LookupCommand lookupCommand = new LookupCommand();
                SelectCommand selectCommand = new SelectCommand();

                List<FilterModel> filterModels = new List<FilterModel>();
                string featuresUuid = _originModel.DatasetConfiguration.BaseUUID;
                string select = FilterModel.GetFilterModelsRecursive(_queryModelClone, new List<QueryModel>(), filterModels, true);
                if (select != "")
                {
                    featuresUuid = (await selectCommand.Select(_originModel, featuresUuid, select))["uuid"].Value<string>();
                }
                featuresUuid = (await projectCommand.Project(_originModel, featuresUuid, features))["uuid"].Value<string>();
      
                foreach (var label in labels)
                {
                    string labelUuid = _originModel.DatasetConfiguration.BaseUUID;
                    if (select != "")
                    {
                        labelUuid = (await selectCommand.Select(_originModel, labelUuid, select))["uuid"].Value<string>();
                    }
                    labelUuid = (await projectCommand.Project(_originModel, labelUuid, new List<InputFieldModel>() { label }))["uuid"].Value<string>();
                    labelsUuid.Add(label, labelUuid);
                }

                if (labels.Count > 1 && false)
                {
                    var labelUuids = (await projectCommand.Project(_originModel, _originModel.DatasetConfiguration.BaseUUID, labels))["uuid"].Value<string>();
                    CorrelateCommand correlateCommand = new CorrelateCommand();
                    var correlateUuid = (await correlateCommand.Correlate(_originModel, labelUuids))["uuid"].Value<string>();
                    JToken correlationToken = await lookupCommand.Lookup(_originModel, correlateUuid, 0, 100) as JToken;
                    while (correlationToken is JObject && correlationToken["empty"] != null && correlationToken["empty"].Value<bool>())
                    {
                        await Task.Delay(100);
                        correlationToken = await lookupCommand.Lookup(_originModel, correlateUuid, 0, 100) as JToken;
                    }
                }

                await Task.Delay(50);

                foreach (var label in labels)
                {
                    string clasifyUuid = (await classifyCommand.Classify(_originModel, _queryModelClone.TaskType, labelsUuid[label], featuresUuid))["uuid"].Value<string>();
                    classifysUuid.Add(label, clasifyUuid);
                }

                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    QueryModel.GenerateCodeUuids = classifysUuid.Select(kvp => kvp.Value).ToList();
                });
            
                await Task.Delay(50);

                ClassfierResultDescriptionModel resultDescriptionModel = new ClassfierResultDescriptionModel();
                resultDescriptionModel.Labels = labels;
                foreach (var label in labels)
                {
                    JObject classifyResultTocken = await lookupCommand.Lookup(_originModel, classifysUuid[label], -1, -1) as JObject;

                    while (classifyResultTocken["empty"] != null && classifyResultTocken["empty"].Value<bool>())
                    {
                        await Task.Delay(100);
                        classifyResultTocken = await lookupCommand.Lookup(_originModel, classifysUuid[label], -1, -1) as JObject;
                    }

                    var classifyResult = JsonConvert.DeserializeObject<ClassifyResult>(classifyResultTocken.ToString());

                    resultDescriptionModel.ConfusionMatrices.Add(label, new List<List<double>>());
                    resultDescriptionModel.ConfusionMatrices[label].Add(new List<double>());
                    resultDescriptionModel.ConfusionMatrices[label][0].Add((double) classifyResult.tp);
                    resultDescriptionModel.ConfusionMatrices[label][0].Add((double) classifyResult.fn);

                    resultDescriptionModel.ConfusionMatrices[label].Add(new List<double>());
                    resultDescriptionModel.ConfusionMatrices[label][1].Add((double) classifyResult.fp);
                    resultDescriptionModel.ConfusionMatrices[label][1].Add((double) classifyResult.tn);

                    var xs = classifyResult.fpr;
                    var ys = classifyResult.tpr;
                    resultDescriptionModel.RocCurves.Add(label, new List<Pt>());
                    resultDescriptionModel.RocCurves[label].Add(new Pt(0, 0));
                    var step = 1;//ys.Count() > 300 ? 50 : 1;  

                    if (xs != null && ys != null)
                    {
                        for (int i = 0; i < xs.Count(); i += step)
                        {
                            resultDescriptionModel.RocCurves[label].Add(new Pt((double) xs[i], (double) ys[i]));
                        }
                        resultDescriptionModel.RocCurves[label].Add(new Pt(1, 1));
                    }

                    resultDescriptionModel.AUCs.Add(label, classifyResult.auc);

                    resultDescriptionModel.F1s.Add(label, classifyResult.f1);
                    resultDescriptionModel.AvgF1 += classifyResult.f1;

                    resultDescriptionModel.Precisions.Add(label, classifyResult.precision);
                    resultDescriptionModel.AvgPrecision += classifyResult.precision;

                    resultDescriptionModel.Recalls.Add(label, classifyResult.recall);
                    resultDescriptionModel.AvRecall += classifyResult.recall;
                }
                resultDescriptionModel.AvgF1 /= (double)labels.Count;
                resultDescriptionModel.AvgPrecision /= (double)labels.Count;
                resultDescriptionModel.AvRecall /= (double)labels.Count;

                await fireUpdated(new List<ResultItemModel>(), 1.0, resultDescriptionModel);
                await fireCompleted();
            }
            catch (Exception exc)
            {
                ErrorHandler.HandleError(exc.Message);
            }
        }

        private void getInputFieldModelsRecursive(List<InputModel> inputModels, List<InputFieldModel> inputFieldModels)
        {
            inputFieldModels.AddRange(inputModels.OfType<InputFieldModel>());
            if (inputModels.OfType<InputGroupModel>().Any())
            {
                foreach (var inputGroupModel in inputModels.OfType<InputGroupModel>())
                {
                    getInputFieldModelsRecursive(inputGroupModel.InputModels, inputFieldModels);
                }
            }
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
            if (MainViewController.Instance.MainModel.Verbose)
            {
                Debug.WriteLine("DataJob Total Run Time: " + _stopWatch.ElapsedMilliseconds);
            }
        }

        public override void Stop()
        {
           
        }
    }
}
