﻿using PanoramicData.model.data;
using PanoramicData.model.data.sim;
using PanoramicData.model.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Dynamic;
using System.Diagnostics;
using PanoramicDataWin8.utils;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Serialization;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using PanoramicData.controller.view;
using PanoramicDataWin8.controller.data.sim;

namespace PanoramicData.controller.data.sim
{
    public class SimQueryExecuter : QueryExecuter
    {
        private Dictionary<QueryModel, SimJob> _activeJobs = new Dictionary<QueryModel, SimJob>();
        private Dictionary<QueryModel, Dictionary<GroupingObject, KeyValuePair<int, QueryResultItemModel>>> _updateIndexCache = new Dictionary<QueryModel, Dictionary<GroupingObject, KeyValuePair<int, QueryResultItemModel>>>();
        
        public override void ExecuteQuery(QueryModel queryModel)
        {
            queryModel.QueryResultModel.QueryResultItemModels = new ObservableCollection<QueryResultItemModel>();

            if (_activeJobs.ContainsKey(queryModel))
            {
                _activeJobs[queryModel].Stop();
                _activeJobs[queryModel].JobUpdate -= simJob_JobUpdate;
                _activeJobs[queryModel].JobCompleted -= simJob_JobCompleted;
                _activeJobs.Remove(queryModel);
                _updateIndexCache.Remove(queryModel);
            }
            // determine if new job is even needed (i.e., are all relevant attributeModels set)
            if ((queryModel.VisualizationType == VisualizationType.table && queryModel.AttributeFunctionOperationModels.Count > 0) ||
                (queryModel.VisualizationType != VisualizationType.table && queryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).Any() &&  queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).Any()))
            {
                SimJob simJob = new SimJob(queryModel, TimeSpan.FromMilliseconds(MainViewController.Instance.MainModel.ThrottleInMillis), (int)MainViewController.Instance.MainModel.SampleSize);
                _activeJobs.Add(queryModel, simJob);
                _updateIndexCache.Add(queryModel, new Dictionary<GroupingObject, KeyValuePair<int, QueryResultItemModel>>());
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
            var oldItems = job.QueryModel.QueryResultModel.QueryResultItemModels;

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
                        KeyValuePair<int, QueryResultItemModel> kvp = cache[sample.GroupingObject];
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
                        cache[sample.GroupingObject] = new KeyValuePair<int, QueryResultItemModel>(i, sample);
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
                        cache.Add(sample.GroupingObject, new KeyValuePair<int, QueryResultItemModel>(i, sample));
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

            job.QueryModel.QueryResultModel.Progress = jobEventArgs.Progress;
            job.QueryModel.QueryResultModel.XNullCount = jobEventArgs.XNullCount;
            job.QueryModel.QueryResultModel.YNullCount = jobEventArgs.YNullCount;
            job.QueryModel.QueryResultModel.XAndYNullCount = jobEventArgs.XAndYNullCount;
            job.QueryModel.QueryResultModel.FireQueryResultModelUpdated();
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

    public class ItemComparer : IComparer<QueryResultItemModel>
    {
        private QueryModel _queryModel = null;
        public ItemComparer(QueryModel queryModel)
        {
            _queryModel = queryModel;
        }
        public int Compare(QueryResultItemModel x, QueryResultItemModel y)
        {
            var attributeOperationModels = _queryModel.AttributeOperationModels.Where(aom => aom.SortMode != SortMode.None);
            foreach (var aom in attributeOperationModels)
            {
                int factor = aom.SortMode == SortMode.Asc ? 1 : -1;
                if (x.AttributeValues[aom].Value is string &&
                   ((string)x.AttributeValues[aom].Value).CompareTo((string)y.AttributeValues[aom].Value) != 0)
                {
                    return (x.AttributeValues[aom].Value as string).CompareTo(y.AttributeValues[aom].Value as string) * factor;
                }
                else if (x.AttributeValues[aom].Value is double &&
                        ((double)x.AttributeValues[aom].Value).CompareTo((double)y.AttributeValues[aom].Value) != 0)
                {
                    return ((double)x.AttributeValues[aom].Value).CompareTo((double)y.AttributeValues[aom].Value) * factor;
                }
                else if (x.AttributeValues[aom].Value is int &&
                        ((int)x.AttributeValues[aom].Value).CompareTo((int)y.AttributeValues[aom].Value) != 0)
                {
                    return ((int)x.AttributeValues[aom].Value).CompareTo((int)y.AttributeValues[aom].Value) * factor;
                }
                else if (x.AttributeValues[aom].Value is DateTime &&
                        ((DateTime)x.AttributeValues[aom].Value).CompareTo((DateTime)y.AttributeValues[aom].Value) != 0)
                {
                    return ((DateTime)x.AttributeValues[aom].Value).CompareTo((DateTime)y.AttributeValues[aom].Value) * factor;
                }
            }
            return 0;
        }
    }
}