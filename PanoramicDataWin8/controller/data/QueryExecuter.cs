using System.Collections.Generic;
using System.Linq;
using PanoramicDataWin8.model.data;

namespace PanoramicDataWin8.controller.data
{
    public abstract class QueryExecuter
    {
        private Dictionary<OperationModel, Job> _activeJobs = new Dictionary<OperationModel, Job>();

        public Dictionary<OperationModel, Job> ActiveJobs
        {
            get { return _activeJobs; }
            set { _activeJobs = value; }
        }

        public virtual void RemoveJob(OperationModel operationModel)
        {
        }

        public virtual void HaltJob(OperationModel operationModel)
        {
        }

        public virtual void ResumeJob(OperationModel operationModel)
        {
        }

        public virtual void HaltAllJobs()
        {
            foreach (var key in ActiveJobs.Keys.ToArray())
            {
                HaltJob(key);
            }
        }

        public virtual bool IsJobRunning(OperationModel operationModel)
        {
            if (ActiveJobs.ContainsKey(operationModel))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public abstract void ExecuteOperationModel(OperationModel operationModel);
    }
}
