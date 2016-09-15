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
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.data.progressive;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class ProgressiveQueryExecuter : QueryExecuter
    {
        public delegate void ExecuteQueryHandler(object sender, ExecuteOperationModelEventArgs e);
        public event ExecuteQueryHandler ExecuteQueryEvent;

        public ProgressiveQueryExecuter()
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
                                (histogramOperationModel.VisualizationType != VisualizationType.table && histogramOperationModel.GetUsageAttributeTransformationModel(InputUsage.X).Any() &&
                                histogramOperationModel.GetUsageAttributeTransformationModel(InputUsage.Y).Any()))
                            {
                                var queryModelClone = operationModel.Clone();
                                ProgressiveVisualizationJob progressiveVisualizationJob = new ProgressiveVisualizationJob(
                                    histogramOperationModel, (HistogramOperationModel)queryModelClone, TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis), (int)MainViewController.Instance.MainModel.SampleSize);

                                ActiveJobs.Add(operationModel, progressiveVisualizationJob);
                                progressiveVisualizationJob.JobUpdate += job_JobUpdate;
                                progressiveVisualizationJob.JobCompleted += job_JobCompleted;
                                progressiveVisualizationJob.Start();
                            }
                        }
                        else
                        {
                         /*   if (operationModel.GetUsageAttributeTransformationModel(InputUsage.Feature).Any() && operationModel.BrushQueryModels.Any())
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

        public override void HaltJob(OperationModel operationModel)
        {
            ActiveJobs[operationModel].Stop();
            ActiveJobs[operationModel].JobUpdate -= job_JobUpdate;
            ActiveJobs[operationModel].JobCompleted -= job_JobCompleted;
            ActiveJobs.Remove(operationModel);
        }

        public override void ResumeJob(OperationModel operationModel)
        {
            ExecuteOperationModel(operationModel);
        }

        public override void ExecuteOperationModel(OperationModel operationModel)
        {
            if (ExecuteQueryEvent != null)
            {
                ExecuteQueryEvent(this, new ExecuteOperationModelEventArgs(operationModel));
            }
        }

        public override void RemoveJob(OperationModel histogramOperationModel)
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
            HistogramOperationModel histogramOperationModel = null;
            if (sender is ProgressiveVisualizationJob)
            {
                ProgressiveVisualizationJob job = sender as ProgressiveVisualizationJob;
                histogramOperationModel = job.HistogramOperationModel;
            }
            else if (sender is ProgressiveClassifyJob)
            {
                ProgressiveClassifyJob job = sender as ProgressiveClassifyJob;
                histogramOperationModel = job.HistogramOperationModel;
            }
            histogramOperationModel.Result = jobEventArgs.Result;
            //operationModel.Result.Progress = 1.0;
            //operationModel.Result.FireResultModelUpdated(ResultType.Complete);
        }

        private void job_JobUpdate(object sender, JobEventArgs jobEventArgs)
        {
            HistogramOperationModel histogramOperationModel = null;
            if (sender is ProgressiveVisualizationJob)
            {
                ProgressiveVisualizationJob job = sender as ProgressiveVisualizationJob;
                histogramOperationModel = job.HistogramOperationModel;
            }
            else if (sender is ProgressiveClassifyJob)
            {
                ProgressiveClassifyJob job = sender as ProgressiveClassifyJob;
                histogramOperationModel = job.HistogramOperationModel;
            }
            histogramOperationModel.Result = jobEventArgs.Result;
        }
    }

    public class ExecuteOperationModelEventArgs : EventArgs
    {
        public OperationModel OperationModel { get; set; }

        public ExecuteOperationModelEventArgs(OperationModel operationModel)
            : base()
        {
            this.OperationModel = operationModel;
        }
    }
}
