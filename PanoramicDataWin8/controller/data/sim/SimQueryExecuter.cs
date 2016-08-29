using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PanoramicDataWin8.controller.data.virt;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.data.sim;

namespace PanoramicDataWin8.controller.data.sim
{
    public class SimQueryExecuter : QueryExecuter
    {
        public override void ExecuteQuery(QueryModel queryModel)
        {
            queryModel.ResultModel.ResultItemModels = new ObservableCollection<ResultItemModel>();
            queryModel.ResultModel.FireResultModelUpdated(ResultType.Clear);

            if (ActiveJobs.ContainsKey(queryModel))
            {
                ActiveJobs[queryModel].Stop();
                ActiveJobs[queryModel].JobUpdate -= simJob_JobUpdate;
                ActiveJobs[queryModel].JobCompleted -= simJob_JobCompleted;
                ActiveJobs.Remove(queryModel);
            }
            // determine if new job is even needed (i.e., are all relevant inputfieldmodels set)
            if ((queryModel.VisualizationType == VisualizationType.table && queryModel.InputOperationModels.Count > 0) ||
                (queryModel.VisualizationType != VisualizationType.table && queryModel.GetUsageInputOperationModel(InputUsage.X).Any() &&  queryModel.GetUsageInputOperationModel(InputUsage.Y).Any()))
            {
                var queryModelClone = queryModel.Clone();
                SimDataProvider dataProvider = new SimDataProvider(queryModelClone, (queryModel.SchemaModel.OriginModels[0] as SimOriginModel));
                DataJob dataJob = new DataJob(
                    queryModel, queryModelClone, dataProvider,
                    TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis), (int)MainViewController.Instance.MainModel.SampleSize);

                ActiveJobs.Add(queryModel, dataJob);
                dataJob.JobUpdate += simJob_JobUpdate;
                dataJob.JobCompleted += simJob_JobCompleted;
                dataJob.Start();
            }
            
        }

        void simJob_JobCompleted(object sender, EventArgs e)
        {
            DataJob dataJob = sender as DataJob;
            dataJob.QueryModel.ResultModel.FireResultModelUpdated(ResultType.Complete);
        }

        void simJob_JobUpdate(object sender, JobEventArgs jobEventArgs)
        {
            DataJob dataJob = sender as DataJob;
            var oldItems = dataJob.QueryModel.ResultModel.ResultItemModels;

            // do proper updateing if this is a table
            /*if (DataJob.QueryModelClone.VisualizationType == VisualizationType.table)
            {
                var cache = _updateIndexCache[DataJob.QueryModelClone];

                // update existing ones
                for (int i = 0; i < jobEventArgs.Samples.Count; i++)
                {
                    var sample = jobEventArgs.Samples[i];
                    if (cache.ContainsKey(sample.GroupingObject))
                    {
                        KeyValuePair<int, ResultItemModel> kvp = cache[sample.GroupingObject];
                        if (kvp.Key == i)
                        {
                            oldItems[i].Update(sample);
                        }
                        else
                        {
                            kvp.Value.Update(sample);
                            if (oldItems.Count <= i)
                            {
                                oldItems.Add(kvp.Value);
                            }
                            else
                            {
                                oldItems[i] = kvp.Value;
                            }
                        }
                        sample = oldItems[i];
                        cache[sample.GroupingObject] = new KeyValuePair<int, ResultItemModel>(i, sample);
                    }
                    else
                    {
                        if (oldItems.Count <= i)
                        {
                            oldItems.Add(sample);
                        }
                        else
                        {
                            oldItems[i] = sample;
                        }
                        cache.Add(sample.GroupingObject, new KeyValuePair<int, ResultItemModel>(i, sample));
                    }
                }
                // remove old ones
                for (int i = jobEventArgs.Samples.Count; i < oldItems.Count; i++)
                {
                    var oldItem = oldItems[i];
                    oldItems.RemoveAt(i);
                    if (cache.ContainsKey(oldItem.GroupingObject))
                    {
                        cache.Remove(oldItem.GroupingObject);
                    }
                }
            }*/
            // not a table
            //else
            {
                oldItems.Clear();
                foreach (var sample in jobEventArgs.Samples)
                {
                    oldItems.Add(sample);
                }
            }

            dataJob.QueryModel.ResultModel.Progress = jobEventArgs.Progress;
            dataJob.QueryModel.ResultModel.ResultDescriptionModel = jobEventArgs.ResultDescriptionModel;
            dataJob.QueryModel.ResultModel.FireResultModelUpdated(ResultType.Update);
        }
    }
}