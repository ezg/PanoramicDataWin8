using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.data.progressive;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.result;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class IDEAQueryExecuter : QueryExecuter
    {
        public delegate void ExecuteQueryHandler(object sender, ExecuteOperationModelEventArgs e);
        public event ExecuteQueryHandler ExecuteQueryEvent;

        public IDEAQueryExecuter()
        {
            var stream = Observable.FromEventPattern<ExecuteOperationModelEventArgs>(this, "ExecuteQueryEvent");
            stream.GroupByUntil(k => k.EventArgs.OperationModel, g => Observable.Timer(TimeSpan.FromMilliseconds(50)))
                .SelectMany(y => y.FirstAsync())
                .Subscribe((async (arg) =>
                {
                    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var operationModel = arg.EventArgs.OperationModel;
                        operationModel.ResultCauserClone = operationModel.Clone();
                        operationModel.Result = null;

                        if (ActiveJobs.ContainsKey(operationModel))
                        {
                            ActiveJobs[operationModel].Stop();
                            ActiveJobs[operationModel].JobUpdate -= job_JobUpdate;
                            ActiveJobs[operationModel].JobCompleted -= job_JobCompleted;
                            ActiveJobs.Remove(operationModel);
                        }
                        // determine if new job is even needed (i.e., are all relevant inputfieldmodels set)
                        if (operationModel is HistogramOperationModel)
                        {
                            var histogramOperationModel = (HistogramOperationModel) operationModel;
                            if ((histogramOperationModel.VisualizationType == VisualizationType.table && histogramOperationModel.AttributeTransformationModels.Count > 0) ||
                                (histogramOperationModel.VisualizationType != VisualizationType.table && histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.X).Any() &&
                                histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.Y).Any()))
                            {
                                HistogramOperationJob histogramOperationJob = new HistogramOperationJob(
                                    histogramOperationModel,  
                                    TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis), (int)MainViewController.Instance.MainModel.SampleSize);

                                ActiveJobs.Add(operationModel, histogramOperationJob);
                                histogramOperationJob.JobUpdate += job_JobUpdate;
                                histogramOperationJob.JobCompleted += job_JobCompleted;
                                histogramOperationJob.Start();
                            }
                        }
                        else if (operationModel is StatisticalComparisonOperationModel)
                        {
                            var statisticalComparisonOperationModel = (StatisticalComparisonOperationModel)operationModel;
                            StatisticalComparisonOperationJob statisticalComparisonOperationJob = new StatisticalComparisonOperationJob(
                                   statisticalComparisonOperationModel,
                                   TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis), (int)MainViewController.Instance.MainModel.SampleSize);

                            ActiveJobs.Add(operationModel, statisticalComparisonOperationJob);
                            statisticalComparisonOperationJob.JobUpdate += job_JobUpdate;
                            statisticalComparisonOperationJob.JobCompleted += job_JobCompleted;
                            statisticalComparisonOperationJob.Start();
                        }
                        else if (operationModel is ExampleOperationModel)
                        {
                            var exampleOperationModel = (ExampleOperationModel)operationModel;
                            ExampleOperationJob exampleOperationJob = new ExampleOperationJob(
                                   exampleOperationModel,
                                   TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis), (int)MainViewController.Instance.MainModel.SampleSize);

                            ActiveJobs.Add(operationModel, exampleOperationJob);
                            exampleOperationJob.JobUpdate += job_JobUpdate;
                            exampleOperationJob.JobCompleted += job_JobCompleted;
                            exampleOperationJob.Start();
                        }
                        else
                        {
                         /*   if (operationModel.GetAttributeUsageTransformationModel(AttributeUsage.Feature).Any() && operationModel.BrushQueryModels.Any())
                            {
                                var queryModelClone = operationModel.Clone();
                                ProgressiveClassifyJob progressiveClassifyJob = new ProgressiveClassifyJob(
                                    operationModel, queryModelClone, TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis), (int) MainViewController.Instance.MainModel.SampleSize);

                                ActiveJobs.Add(operationModel, progressiveClassifyJob);
                                progressiveClassifyJob.JobUpdate += job_JobUpdate;
                                progressiveClassifyJob.JobCompleted += job_JobCompleted;
                                progressiveClassifyJob.Start();
                            }*/
                        }
                    });
                }));
        }

        public override void HaltJob(IOperationModel operationModel)
        {
            ActiveJobs[operationModel].Stop();
            ActiveJobs[operationModel].JobUpdate -= job_JobUpdate;
            ActiveJobs[operationModel].JobCompleted -= job_JobCompleted;
            ActiveJobs.Remove(operationModel);
        }

        public override void ResumeJob(IOperationModel operationModel)
        {
            ExecuteOperationModel(operationModel);
        }

        public override void ExecuteOperationModel(IOperationModel operationModel)
        {
            if (ExecuteQueryEvent != null)
            {
                ExecuteQueryEvent(this, new ExecuteOperationModelEventArgs(operationModel));
            }
        }

        public override void RemoveJob(IOperationModel histogramOperationModel)
        {
            if (ActiveJobs.ContainsKey(histogramOperationModel))
            {
                ActiveJobs[histogramOperationModel].Stop();
                ActiveJobs[histogramOperationModel].JobUpdate -= job_JobUpdate;
                ActiveJobs[histogramOperationModel].JobCompleted -= job_JobCompleted;
                ActiveJobs.Remove(histogramOperationModel);
            }
        }

        private void job_JobCompleted(object sender, JobEventArgs jobEventArgs)
        {
            OperationJob operationJob = (OperationJob) sender;
            OperationModel operationModel = operationJob.OperationModel;
            operationModel.Result = jobEventArgs.Result;
        }

        private void job_JobUpdate(object sender, JobEventArgs jobEventArgs)
        {
            OperationJob operationJob = (OperationJob) sender;
            OperationModel operationModel = operationJob.OperationModel;
            operationModel.Result = jobEventArgs.Result;
        }
    }

    public class ExecuteOperationModelEventArgs : EventArgs
    {
        public IOperationModel OperationModel { get; set; }

        public ExecuteOperationModelEventArgs(IOperationModel operationModel)
            : base()
        {
            this.OperationModel = operationModel;
        }
    }
}
