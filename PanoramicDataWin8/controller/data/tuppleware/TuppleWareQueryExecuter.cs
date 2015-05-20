using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.data.sim;
using PanoramicDataWin8.controller.data.virt;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.data.sim;
using PanoramicDataWin8.model.data.tuppleware;

namespace PanoramicDataWin8.controller.data.tuppleware
{
    public class TuppleWareQueryExecuter : QueryExecuter
    {
        public override void ExecuteQuery(QueryModel queryModel)
        {
            queryModel.ResultModel.ResultItemModels = new ObservableCollection<ResultItemModel>();
            queryModel.ResultModel.ResultDescriptionModel = null;
            queryModel.ResultModel.FireResultModelUpdated();

            if (ActiveJobs.ContainsKey(queryModel))
            {
                ActiveJobs[queryModel].Stop();
                ActiveJobs[queryModel].JobUpdate -= job_JobUpdate;
                ActiveJobs[queryModel].JobCompleted -= job_JobCompleted;
                ActiveJobs.Remove(queryModel);
            }
            // determine if new job is even needed (i.e., are all relevant inputfieldmodels set)
            if (queryModel.JobType == JobType.DB)
            {
                if ((queryModel.VisualizationType == VisualizationType.table && queryModel.InputOperationModels.Count > 0) ||
                    (queryModel.VisualizationType != VisualizationType.table && queryModel.GetUsageInputOperationModel(InputUsage.X).Any() && queryModel.GetUsageInputOperationModel(InputUsage.Y).Any()))
                {
                    var queryModelClone = queryModel.Clone();
                    TuppleWareDataProvider dataProvider = new TuppleWareDataProvider(queryModelClone, (queryModel.SchemaModel.OriginModels[0] as TuppleWareOriginModel));
                    DataJob dataJob = new DataJob(
                        queryModel, queryModelClone, dataProvider,
                        TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis), (int) MainViewController.Instance.MainModel.SampleSize);

                    ActiveJobs.Add(queryModel, dataJob);
                    dataJob.JobUpdate += job_JobUpdate;
                    dataJob.JobCompleted += job_JobCompleted;
                    dataJob.Start();
                }
            }
            else
            {
                if (queryModel.GetUsageInputOperationModel(InputUsage.Feature).Any() && queryModel.GetUsageInputOperationModel(InputUsage.Label).Any())
                {
                    var queryModelClone = queryModel.Clone();
                    ClassifierJob classifierJob = new ClassifierJob(queryModel, queryModelClone, (queryModel.SchemaModel.OriginModels[0] as TuppleWareOriginModel), TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis));

                    ActiveJobs.Add(queryModel, classifierJob);
                    classifierJob.JobUpdate += job_JobUpdate;
                    classifierJob.JobCompleted += job_JobCompleted;
                    classifierJob.Start();
                }
            }

        }

        private void job_JobCompleted(object sender, EventArgs e)
        {
        }

        private void job_JobUpdate(object sender, JobEventArgs jobEventArgs)
        {
            QueryModel queryModel = null;
            if (sender is DataJob)
            {
                DataJob job = sender as DataJob;
                queryModel = job.QueryModel;
            }
            else if (sender is ClassifierJob)
            {
                ClassifierJob job = sender as ClassifierJob;
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
            queryModel.ResultModel.FireResultModelUpdated();
        }
    }
}
