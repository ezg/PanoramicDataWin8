using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using PanoramicDataWin8.controller.data.progressive;
using PanoramicDataWin8.controller.data.tuppleware;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.data.progressive;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class ProgressiveQueryExecuter : QueryExecuter
    {
        public delegate void ExecuteQueryHandler(object sender, ExecuteQueryEventArgs e);
        public event ExecuteQueryHandler ExecuteQueryEvent;

        public ProgressiveQueryExecuter()
        {
            var stream = Observable.FromEventPattern<ExecuteQueryEventArgs>(this, "ExecuteQueryEvent");
            stream.GroupByUntil(k => k.EventArgs.QueryModel, g => Observable.Timer(TimeSpan.FromMilliseconds(20)))
                .SelectMany(y => y.FirstAsync())
                .Subscribe((async (arg) =>
                {
                    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var queryModel = arg.EventArgs.QueryModel;
                        queryModel.ResultModel.ResultItemModels = new ObservableCollection<ResultItemModel>();
                        queryModel.ResultModel.Progress = 0.0;
                        queryModel.ResultModel.ResultDescriptionModel = null;
                        queryModel.ResultModel.FireResultModelUpdated(ResultType.Clear);

                        if (ActiveJobs.ContainsKey(queryModel))
                        {
                            ActiveJobs[queryModel].Stop();
                            ActiveJobs[queryModel].JobUpdate -= job_JobUpdate;
                            ActiveJobs[queryModel].JobCompleted -= job_JobCompleted;
                            ActiveJobs.Remove(queryModel);
                        }
                        // determine if new job is even needed (i.e., are all relevant inputfieldmodels set)
                        if (queryModel.TaskModel == null)
                        {
                            if ((queryModel.VisualizationType == VisualizationType.table && queryModel.InputOperationModels.Count > 0) ||
                                (queryModel.VisualizationType != VisualizationType.table && queryModel.GetUsageInputOperationModel(InputUsage.X).Any() && queryModel.GetUsageInputOperationModel(InputUsage.Y).Any()))
                            {
                                var queryModelClone = queryModel.Clone();
                                ProgressiveVisualizationJob progressiveVisualizationJob = new ProgressiveVisualizationJob(
                                    queryModel, queryModelClone, TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis), (int)MainViewController.Instance.MainModel.SampleSize);

                                ActiveJobs.Add(queryModel, progressiveVisualizationJob);
                                progressiveVisualizationJob.JobUpdate += job_JobUpdate;
                                progressiveVisualizationJob.JobCompleted += job_JobCompleted;
                                progressiveVisualizationJob.Start();
                            }
                        }
                        else
                        {
                            if (queryModel.GetUsageInputOperationModel(InputUsage.Feature).Any() && queryModel.BrushQueryModels.Any())
                            {
                                var queryModelClone = queryModel.Clone();
                                ProgressiveClassifyJob progressiveClassifyJob = new ProgressiveClassifyJob(
                                    queryModel, queryModelClone, TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis), (int) MainViewController.Instance.MainModel.SampleSize);

                                ActiveJobs.Add(queryModel, progressiveClassifyJob);
                                progressiveClassifyJob.JobUpdate += job_JobUpdate;
                                progressiveClassifyJob.JobCompleted += job_JobCompleted;
                                progressiveClassifyJob.Start();
                            }
                        }
                    });
                }));
        }

        public override void ExecuteQuery(QueryModel queryModel)
        {
            if (ExecuteQueryEvent != null)
            {
                ExecuteQueryEvent(this, new ExecuteQueryEventArgs(queryModel));
            }
        }

        public override void RemoveJob(QueryModel queryModel)
        {
            if (ActiveJobs.ContainsKey(queryModel))
            {
                ActiveJobs[queryModel].Stop();
                ActiveJobs[queryModel].JobUpdate -= job_JobUpdate;
                ActiveJobs[queryModel].JobCompleted -= job_JobCompleted;
                ActiveJobs.Remove(queryModel);
            }
        }

        private void job_JobCompleted(object sender, EventArgs e)
        {
            QueryModel queryModel = null;
            if (sender is ProgressiveVisualizationJob)
            {
                ProgressiveVisualizationJob job = sender as ProgressiveVisualizationJob;
                queryModel = job.QueryModel;
            }
            else if (sender is ProgressiveClassifyJob)
            {
                ProgressiveClassifyJob job = sender as ProgressiveClassifyJob;
                queryModel = job.QueryModel;
            }
            queryModel.ResultModel.Progress = 1.0;
            queryModel.ResultModel.FireResultModelUpdated(ResultType.Complete);
        }

        private void job_JobUpdate(object sender, JobEventArgs jobEventArgs)
        {
            QueryModel queryModel = null;
            if (sender is ProgressiveVisualizationJob)
            {
                ProgressiveVisualizationJob job = sender as ProgressiveVisualizationJob;
                queryModel = job.QueryModel;
            }
            else if (sender is ProgressiveClassifyJob)
            {
                ProgressiveClassifyJob job = sender as ProgressiveClassifyJob;
                queryModel = job.QueryModel;
            }
            var oldItems = queryModel.ResultModel.ResultItemModels;
            {
                oldItems.Clear();
                foreach (var sample in jobEventArgs.Samples)
                {
                    oldItems.Add(sample);
                }
            }
            queryModel.ResultModel.Progress = jobEventArgs.Progress;
            queryModel.ResultModel.ResultDescriptionModel = jobEventArgs.ResultDescriptionModel;
            queryModel.ResultModel.FireResultModelUpdated(ResultType.Update);
        }
    }

    public class ExecuteQueryEventArgs : EventArgs
    {
        public QueryModel QueryModel { get; set; }

        public ExecuteQueryEventArgs(QueryModel queryModel)
            : base()
        {
            this.QueryModel = queryModel;
        }
    }
}
