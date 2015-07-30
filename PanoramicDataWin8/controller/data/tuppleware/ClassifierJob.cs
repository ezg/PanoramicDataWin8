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

            List<InputFieldModel> labels = new List<InputFieldModel>();
            getInputFieldModelsRecursive(_queryModelClone.GetUsageInputOperationModel(InputUsage.Label).Select(iom => iom.InputModel).ToList(), labels);

            List<InputFieldModel> features = new List<InputFieldModel>();
            getInputFieldModelsRecursive(_queryModelClone.GetUsageInputOperationModel(InputUsage.Feature).Select(iom => iom.InputModel).ToList(), features);

            long featuresUuid = TuppleWareGateway.GetNextUuid();
            Dictionary<InputFieldModel, long> labelsUuid = new Dictionary<InputFieldModel, long>();
            Dictionary<InputFieldModel, long> classifysUuid = new Dictionary<InputFieldModel, long>();

            ProjectCommand projectCommand = new ProjectCommand();
            ClassifyCommand classifyCommand = new ClassifyCommand();
            LookupCommand lookupCommand = new LookupCommand();

            projectCommand.Project(_originModel, featuresUuid, _originModel.DatasetConfiguration.BaseUUID, features);
            
            foreach (var label in labels)
            {
                long labelUuid = TuppleWareGateway.GetNextUuid();
                labelsUuid.Add(label, labelUuid);
                projectCommand.Project(_originModel, labelUuid, _originModel.DatasetConfiguration.BaseUUID, new List<InputFieldModel>(){label});
            }

            await Task.Delay(50);

            foreach (var label in labels)
            {
                long clasifyUuid = TuppleWareGateway.GetNextUuid();
                classifysUuid.Add(label, clasifyUuid);
                classifyCommand.Classify(_originModel, _queryModelClone.JobType, labelsUuid[label], featuresUuid, clasifyUuid);
            }

            await Task.Delay(50);

            ClassfierResultDescriptionModel resultDescriptionModel = new ClassfierResultDescriptionModel();
            resultDescriptionModel.Labels = labels;
            foreach (var label in labels)
            {
                JToken classifyResultTocken = await lookupCommand.Lookup(_originModel, classifysUuid[label], -1, -1);
                var classifyResult = JsonConvert.DeserializeObject<ClassifyResult>(classifyResultTocken.ToString());

                resultDescriptionModel.ConfusionMatrices.Add(label, new List<List<double>>());
                resultDescriptionModel.ConfusionMatrices[label].Add(new List<double>());
                resultDescriptionModel.ConfusionMatrices[label][0].Add((double) classifyResult.tn);
                resultDescriptionModel.ConfusionMatrices[label][0].Add((double) classifyResult.fn);

                resultDescriptionModel.ConfusionMatrices[label].Add(new List<double>());
                resultDescriptionModel.ConfusionMatrices[label][1].Add((double) classifyResult.fp);
                resultDescriptionModel.ConfusionMatrices[label][1].Add((double) classifyResult.tp);

                var xs = classifyResult.fpr;
                var ys = classifyResult.tpr;
                resultDescriptionModel.RocCurves.Add(label, new List<Pt>());
                resultDescriptionModel.RocCurves[label].Add(new Pt(0, 0));
                var step = 1;//ys.Count() > 300 ? 50 : 1;  
                for (int i = 0; i < xs.Count(); i += step)
                {
                    resultDescriptionModel.RocCurves[label].Add(new Pt((double)xs[i], (double)ys[i]));
                }
                resultDescriptionModel.RocCurves[label].Add(new Pt(1, 1));

                resultDescriptionModel.F1s.Add(label, classifyResult.f1);
                resultDescriptionModel.AUCs.Add(label, classifyResult.auc);
            }
            await fireUpdated(new List<ResultItemModel>(), 1.0, resultDescriptionModel);
            await fireCompleted();
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
