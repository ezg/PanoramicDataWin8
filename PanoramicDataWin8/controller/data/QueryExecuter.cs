using System.Collections.Generic;
using System.Linq;
using PanoramicDataWin8.model.data;

namespace PanoramicDataWin8.controller.data
{
    public abstract class QueryExecuter
    {
        private Dictionary<QueryModel, Job> _activeJobs = new Dictionary<QueryModel, Job>();

        public Dictionary<QueryModel, Job> ActiveJobs
        {
            get { return _activeJobs; }
            set { _activeJobs = value; }
        }

        public virtual void RemoveJob(QueryModel queryModel)
        {
        }

        public virtual void HaltJob(QueryModel queryModel)
        {
        }

        public virtual void ResumeJob(QueryModel queryModel)
        {
        }

        public virtual void HaltAllJobs()
        {
            foreach (var key in ActiveJobs.Keys.ToArray())
            {
                HaltJob(key);
            }
        }

        public virtual bool IsJobRunning(QueryModel queryModel)
        {
            if (ActiveJobs.ContainsKey(queryModel))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public abstract void ExecuteQuery(QueryModel queryModel);
    }
}
