using System.Collections.Generic;
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

        public abstract void ExecuteQuery(QueryModel queryModel);
    }
}
