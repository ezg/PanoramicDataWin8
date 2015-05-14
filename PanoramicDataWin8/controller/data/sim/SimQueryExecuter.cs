using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.data.sim;

namespace PanoramicDataWin8.controller.data.sim
{
    public class SimQueryExecuter : QueryExecuter
    {
        private Dictionary<QueryModel, SimJob> _activeJobs = new Dictionary<QueryModel, SimJob>();
        
        public override void ExecuteQuery(QueryModel queryModel)
        {
            queryModel.ResultModel.ResultItemModels = new ObservableCollection<ResultItemModel>();

            if (_activeJobs.ContainsKey(queryModel))
            {
                _activeJobs[queryModel].Stop();
                _activeJobs[queryModel].JobUpdate -= simJob_JobUpdate;
                _activeJobs[queryModel].JobCompleted -= simJob_JobCompleted;
                _activeJobs.Remove(queryModel);
            }
            // determine if new job is even needed (i.e., are all relevant attributeModels set)
            if ((queryModel.VisualizationType == VisualizationType.table && queryModel.AttributeFunctionOperationModels.Count > 0) ||
                (queryModel.VisualizationType != VisualizationType.table && queryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).Any() &&  queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).Any()))
            {
                SimJob simJob = new SimJob(queryModel, TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis), (int)MainViewController.Instance.MainModel.SampleSize);
                _activeJobs.Add(queryModel, simJob);
                simJob.JobUpdate += simJob_JobUpdate;
                simJob.JobCompleted += simJob_JobCompleted;
                simJob.Start();
            }
            
        }

        void simJob_JobCompleted(object sender, EventArgs e)
        {
        }

        void simJob_JobUpdate(object sender, JobEventArgs jobEventArgs)
        {
            SimJob job = sender as SimJob;
            var oldItems = job.QueryModel.ResultModel.ResultItemModels;

            // do proper updateing if this is a table
            /*if (job.QueryModel.VisualizationType == VisualizationType.table)
            {
                var cache = _updateIndexCache[job.QueryModel];

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

            job.QueryModel.ResultModel.Progress = jobEventArgs.Progress;
            job.QueryModel.ResultModel.ResultDescriptionModel = jobEventArgs.ResultDescriptionModel;
            job.QueryModel.ResultModel.FireResultModelUpdated();
        }
    }

    public class DataEqualityComparer : IEqualityComparer<Dictionary<AttributeModel, object>>
    {
        private QueryModel _queryModel = null;
        public DataEqualityComparer(QueryModel queryModel)
        {
            _queryModel = queryModel;
        }
        public bool Equals(Dictionary<AttributeModel, object> x, Dictionary<AttributeModel, object> y)
        {
            return x[(_queryModel.SchemaModel.OriginModels[0] as SimOriginModel).IdAttributeModel].Equals(
                    y[(_queryModel.SchemaModel.OriginModels[0] as SimOriginModel).IdAttributeModel]);
        }
        public int GetHashCode(Dictionary<AttributeModel, object> x)
        {
            return x[(_queryModel.SchemaModel.OriginModels[0] as SimOriginModel).IdAttributeModel].GetHashCode();
        }
    }
}