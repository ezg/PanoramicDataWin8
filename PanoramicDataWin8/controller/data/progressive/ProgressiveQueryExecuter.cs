﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using PanoramicDataWin8.controller.data.progressive;
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
                            if (queryModel.TaskModel.Name != "frequent_itemsets")
                            {
                                /*if (queryModel.GetUsageInputOperationModel(InputUsage.Feature).Any() && queryModel.GetUsageInputOperationModel(InputUsage.Label).Any())
                                {
                                    var queryModelClone = queryModel.Clone();
                                    ClassifierJob classifierJob = new ClassifierJob(queryModel, queryModelClone, (queryModel.SchemaModel.OriginModels[0] as ProgressiveOriginModel),
                                        TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis));

                                    ActiveJobs.Add(queryModel, classifierJob);
                                    classifierJob.JobUpdate += job_JobUpdate;
                                    classifierJob.JobCompleted += job_JobCompleted;
                                    classifierJob.Start();
                                }*/
                            }
                            else if (queryModel.TaskModel.Name == "frequent_itemsets")
                            {
                                /*if (queryModel.GetUsageInputOperationModel(InputUsage.Label).Any())
                                {
                                    var queryModelClone = queryModel.Clone();
                                    FrequentItemsetJob frequentItemsetJob = new FrequentItemsetJob(queryModel, queryModelClone, (queryModel.SchemaModel.OriginModels[0] as ProgressiveOriginModel),
                                        TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis));

                                    ActiveJobs.Add(queryModel, frequentItemsetJob);
                                    frequentItemsetJob.JobUpdate += job_JobUpdate;
                                    frequentItemsetJob.JobCompleted += job_JobCompleted;
                                    frequentItemsetJob.Start();
                                }*/
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

        private void job_JobCompleted(object sender, EventArgs e)
        {
            QueryModel queryModel = null;
            if (sender is ProgressiveVisualizationJob)
            {
                ProgressiveVisualizationJob job = sender as ProgressiveVisualizationJob;
                queryModel = job.QueryModel;
            }
            /*else if (sender is ClassifierJob)
            {
                ClassifierJob job = sender as ClassifierJob;
                queryModel = job.QueryModel;
            }*/
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
            /*else if (sender is ClassifierJob)
            {
                ClassifierJob job = sender as ClassifierJob;
                queryModel = job.QueryModel;
            }*/
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
