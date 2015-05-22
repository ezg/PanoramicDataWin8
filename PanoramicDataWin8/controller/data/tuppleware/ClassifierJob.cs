using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Newtonsoft.Json.Linq;
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

            JArray lines = await TuppleWareGateway.Classify(_originModel, features, labels, _queryModelClone.JobType);
            JObject returnObject = lines[0] as JObject;

            ClassfierResultDescriptionModel resultDescriptionModel = new ClassfierResultDescriptionModel();
            resultDescriptionModel.Labels = labels;
            foreach (var labelInputFieldModel in labels)
            {
                var labelResponse = returnObject[labelInputFieldModel.Name];
                var conf = labelResponse[0];
                resultDescriptionModel.ConfusionMatrices.Add(labelInputFieldModel, new List<List<double>>());
                resultDescriptionModel.ConfusionMatrices[labelInputFieldModel].Add(new List<double>());
                resultDescriptionModel.ConfusionMatrices[labelInputFieldModel][0].Add((double)conf[0][0]);
                resultDescriptionModel.ConfusionMatrices[labelInputFieldModel][0].Add((double)conf[0][1]);
                resultDescriptionModel.ConfusionMatrices[labelInputFieldModel].Add(new List<double>());
                resultDescriptionModel.ConfusionMatrices[labelInputFieldModel][1].Add((double)conf[1][0]);
                resultDescriptionModel.ConfusionMatrices[labelInputFieldModel][1].Add((double)conf[1][1]);

                var xs = labelResponse[1];
                var ys = labelResponse[2];
                resultDescriptionModel.RocCurves.Add(labelInputFieldModel, new List<Pt>());
                resultDescriptionModel.RocCurves[labelInputFieldModel].Add(new Pt(0, 0));
                var step = 1;//ys.Count() > 300 ? 50 : 1;  
                for(int i = 0; i < xs.Count(); i += step)
                {
                    resultDescriptionModel.RocCurves[labelInputFieldModel].Add(new Pt((double)xs[i], (double)ys[i]));
                } 
                resultDescriptionModel.RocCurves[labelInputFieldModel].Add(new Pt(1, 1));

                var f1 = labelResponse[3];
                resultDescriptionModel.F1s.Add(labelInputFieldModel, (double) f1);
            }


            
            {
                /*BinRanges = _binner.BinStructure.BinRanges,
                NullCount = _binner.BinStructure.NullCount,
                Dimensions = _dimensions,
                AxisTypes = _axisTypes,
                MinValues = _binner.BinStructure.AggregatedMinValues.ToDictionary(entry => entry.Key, entry => entry.Value),
                MaxValues = _binner.BinStructure.AggregatedMaxValues.ToDictionary(entry => entry.Key, entry => entry.Value)*/
            };

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
