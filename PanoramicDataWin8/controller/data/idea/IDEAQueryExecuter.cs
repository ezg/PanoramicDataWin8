using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using IDEA_common.operations;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class IDEAQueryExecuter : QueryExecuter
    {
        public delegate void ExecuteQueryHandler(object sender, ExecuteOperationModelEventArgs e);

        public IDEAQueryExecuter()
        {
            var stream = Observable.FromEventPattern<ExecuteOperationModelEventArgs>(this, "ExecuteQueryEvent");
            stream.GroupByUntil(k => k.EventArgs.OperationModel, g => Observable.Timer(TimeSpan.FromMilliseconds(50)))
                .SelectMany(y => y.LastAsync())
                .Subscribe(async arg =>
                {
                    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var operationModel = arg.EventArgs.OperationModel;
                        if (operationModel.ResultCauserClone != null)
                            operationModel.ResultCauserClone.Cleanup();
                        operationModel.ResultCauserClone = operationModel.Clone();
                        operationModel.Result = null;

                        if (ActiveJobs.ContainsKey(operationModel) && arg.EventArgs.StopPreviousExecution)
                        {
                            foreach (var executionId in ActiveJobs[operationModel].Keys)
                            {
                                ActiveJobs[operationModel][executionId].Stop();
                                ActiveJobs[operationModel][executionId].JobUpdate -= job_JobUpdate;
                                ActiveJobs[operationModel][executionId].JobCompleted -= job_JobCompleted;
                            }
                         
                            ActiveJobs.Remove(operationModel);
                        }
                        // determine if new job is even needed (i.e., are all relevant inputfieldmodels set)
                        OperationJob newJob = null;
                        if (operationModel is HistogramOperationModel)
                        {
                            var histogramOperationModel = (HistogramOperationModel) operationModel;
                            if (((histogramOperationModel.VisualizationType == VisualizationType.table) && (histogramOperationModel.AttributeTransformationModels.Count > 0)) ||
                                ((histogramOperationModel.VisualizationType != VisualizationType.table) && histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.X).Any() &&
                                 histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.Y).Any()))
                            {
                                newJob = new HistogramOperationJob(
                                    histogramOperationModel,
                                    (int) MainViewController.Instance.MainModel.SampleSize);
                            }
                        }
                        else if (operationModel is RawDataOperationModel)
                        {
                            var rawDataOperationModel = (RawDataOperationModel)operationModel;
                            if (rawDataOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.X).Any())
                            {
                                newJob = new RawDataOperationJob(
                                    rawDataOperationModel,
                                    (int)MainViewController.Instance.MainModel.SampleSize);
                            }
                        }
                        else if (operationModel is StatisticalComparisonOperationModel)
                        {
                            var statisticalComparisonOperationModel = (StatisticalComparisonOperationModel) operationModel;
                            newJob = new StatisticalComparisonOperationJob(
                                statisticalComparisonOperationModel,
                                (int) MainViewController.Instance.MainModel.SampleSize);
                        }
                        else if (operationModel is ExampleOperationModel)
                        {
                            var exampleOperationModel = (ExampleOperationModel) operationModel;
                            newJob = new ExampleOperationJob(
                                exampleOperationModel,
                                (int) MainViewController.Instance.MainModel.SampleSize);
                        }
                        else if (operationModel is AttributeGroupOperationModel)
                        {
                        }
                        else if (operationModel is RiskOperationModel)
                        {
                            var riskOperationModel = (RiskOperationModel)operationModel;
                            newJob = new RiskOperationJob(
                                riskOperationModel);
                        }
                        else if (operationModel is StatisticalComparisonDecisionOperationModel)
                        {
                            var riskOperationModel = (StatisticalComparisonDecisionOperationModel)operationModel;
                            newJob = new StatisticalComparisonDecisionOperationJob(riskOperationModel);
                        }
                        else if (operationModel is RecommenderOperationModel)
                        {
                            var recommenderOperationModel = (RecommenderOperationModel)operationModel;
                            newJob = new RecommenderOperationJob(
                                recommenderOperationModel,
                                (int)MainViewController.Instance.MainModel.SampleSize);
                        }
                        else if (operationModel is PredictorOperationModel)
                        {
                            var predictorOperationModel = (PredictorOperationModel)operationModel;
                            if (predictorOperationModel.TargetAttributeUsageTransformationModel != null)
                            {
                                newJob = new RecommenderOperationJob(
                                    predictorOperationModel,
                                    (int) MainViewController.Instance.MainModel.SampleSize);
                            }
                        }


                        if (newJob != null)
                        {
                            if (!ActiveJobs.ContainsKey(operationModel))
                            {
                                ActiveJobs.Add(operationModel, new Dictionary<int, OperationJob>());
                            }
                            ActiveJobs[operationModel][operationModel.ExecutionId] = newJob;
                            newJob.JobUpdate += job_JobUpdate;
                            newJob.JobCompleted += job_JobCompleted;
                            newJob.Start();
                        }
                    });
                });
        }

        public event ExecuteQueryHandler ExecuteQueryEvent;

        public override void HaltJob(IOperationModel operationModel)
        {
            if (ActiveJobs.ContainsKey(operationModel))
            {
                foreach (var executionId in ActiveJobs[operationModel].Keys)
                {
                    ActiveJobs[operationModel][executionId].Stop();
                    ActiveJobs[operationModel][executionId].JobUpdate -= job_JobUpdate;
                    ActiveJobs[operationModel][executionId].JobCompleted -= job_JobCompleted;
                }

                ActiveJobs.Remove(operationModel);
            }
        }

        public override void ResumeJob(IOperationModel operationModel)
        {
            ExecuteOperationModel(operationModel, true);
        }

        public override async void UpdateResultParameters(IOperationModel operationModel)
        {
            if (ActiveJobs.ContainsKey(operationModel))
            {
                var job = ActiveJobs[operationModel][operationModel.ExecutionId];
                var resultParams = operationModel.ResultParameters;
                resultParams.OperationReference = job.OperationReference;
                var resultCommand = new ResultCommand();
                var result = await resultCommand.GetResult(resultParams);
                if (result != null)
                {
                    operationModel.Result = result;
                }
            }
        }

        public override void ExecuteOperationModel(IOperationModel operationModel, bool stopPreviousExecutions)
        {
            if (ExecuteQueryEvent != null)
                ExecuteQueryEvent(this, new ExecuteOperationModelEventArgs(operationModel, stopPreviousExecutions));
        }

        private void job_JobCompleted(object sender, JobEventArgs jobEventArgs)
        {
            var operationJob = (OperationJob) sender;
            var operationModel = operationJob.OperationModel;
            operationModel.Result = jobEventArgs.Result;
        }

        private void job_JobUpdate(object sender, JobEventArgs jobEventArgs)
        {
            var operationJob = (OperationJob) sender;
            var operationModel = operationJob.OperationModel;
            operationModel.ResultExecutionId = jobEventArgs.ResultExecutionId;
            operationModel.Result = jobEventArgs.Result; // setting this causes the OperationModel's PropertyChanged handler to fire to update the display
        }
    }
}