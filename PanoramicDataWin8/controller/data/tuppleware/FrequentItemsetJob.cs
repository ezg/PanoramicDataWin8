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

namespace PanoramicDataWin8.controller.data.tuppleware
{
    public class FrequentItemsetJob : Job
    {
        private Stopwatch _stopWatch = new Stopwatch();
        private TimeSpan _throttle = TimeSpan.FromMilliseconds(0);
        private TuppleWareOriginModel _originModel = null;
        private QueryModel _queryModelClone = null;

        public QueryModel QueryModel { get; set; }

        public FrequentItemsetJob(QueryModel queryModel, QueryModel queryModelClone, TuppleWareOriginModel orignModel, TimeSpan throttle)
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

                ProjectCommand projectCommand = new ProjectCommand();
                FrequentItemsetsCommand frequentItemsetsCommand = new FrequentItemsetsCommand();
                LookupCommand lookupCommand = new LookupCommand();
                SelectCommand selectCommand = new SelectCommand();

                List<FilterModel> filterModels = new List<FilterModel>();
                string labelsUuid = _originModel.DatasetConfiguration.BaseUUID;
                string select = FilterModel.GetFilterModelsRecursive(_queryModelClone, new List<QueryModel>(), filterModels, true);
                if (select != "")
                {
                    labelsUuid = (await selectCommand.Select(_originModel, labelsUuid, select))["uuid"].Value<string>();
                }
                labelsUuid = (await projectCommand.Project(_originModel, labelsUuid, labels))["uuid"].Value<string>();

                await Task.Delay(50);

                string frequentItemsetsUuid = (await frequentItemsetsCommand.Frequent(_originModel, _queryModelClone.TaskModel.Name, labelsUuid, _queryModelClone.MinimumSupport))["uuid"].Value<string>();

                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    QueryModel.GenerateCodeUuids = new List<string>() {frequentItemsetsUuid};
                });

                await Task.Delay(50);

                InputFieldModel pattern = new TuppleWareFieldInputModel("pattern", InputDataTypeConstants.NVARCHAR, InputVisualizationTypeConstants.ENUM)
                {
                    OriginModel = (_queryModelClone.SchemaModel.OriginModels[0] as TuppleWareOriginModel)
                };
                InputFieldModel minsupport = new TuppleWareFieldInputModel("support", InputDataTypeConstants.NVARCHAR, InputVisualizationTypeConstants.ENUM)
                {
                    OriginModel = (_queryModelClone.SchemaModel.OriginModels[0] as TuppleWareOriginModel)
                };

                DataPage dataPage = new DataPage();
                dataPage.DataRows = new List<DataRow>();
                JToken frequentItemsetToken = await lookupCommand.Lookup(_originModel, frequentItemsetsUuid, 0, 1000) as JToken;

                while (frequentItemsetToken is JObject && frequentItemsetToken["empty"] != null && frequentItemsetToken["empty"].Value<bool>())
                {
                    await Task.Delay(100);
                    frequentItemsetToken = await lookupCommand.Lookup(_originModel, frequentItemsetsUuid, 0, 1000) as JToken;
                }


                /*DataRow r1 = new DataRow();
                r1.Entries = new Dictionary<InputFieldModel, object>();
                r1.Entries.Add(pattern, "asdfj");
                r1.Entries.Add(minsupport, "0.5");
                dataPage.DataRows.Add(r1);
                r1 = new DataRow();
                r1.Entries = new Dictionary<InputFieldModel, object>();
                r1.Entries.Add(pattern, "asdfj");
                r1.Entries.Add(minsupport, "0.5");
                dataPage.DataRows.Add(r1);*/

                foreach (var child in frequentItemsetToken as JArray)
                {
                    string p = child["items"].Value<string>();
                    //p = p.Replace("(", "");
                    //p = p.Replace(",)", "");
                    double s = child["support"].Value<double>();

                    DataRow r1 = new DataRow();
                    r1.Entries = new Dictionary<InputFieldModel, object>();
                    r1.Entries.Add(pattern, p);
                    r1.Entries.Add(minsupport, s);
                    dataPage.DataRows.Add(r1);
                }

                var resultItems = dataPage.DataRows.Select(dr => dr as ResultItemModel).ToList();
                resultItems = resultItems.OrderByDescending(rs => (rs as DataRow).Entries[minsupport]).ToList();
                await fireUpdated(resultItems, 1.0, null);
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
