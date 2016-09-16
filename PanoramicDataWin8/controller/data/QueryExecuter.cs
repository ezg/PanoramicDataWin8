using System.Collections.Generic;
using System.Linq;
using PanoramicDataWin8.model.data;

namespace PanoramicDataWin8.controller.data
{
    public abstract class QueryExecuter
    {
        private Dictionary<IOperationModel, Job> _activeJobs = new Dictionary<IOperationModel, Job>();

        public Dictionary<IOperationModel, Job> ActiveJobs
        {
            get { return _activeJobs; }
            set { _activeJobs = value; }
        }

        public virtual void RemoveJob(IOperationModel operationModel)
        {
        }

        public virtual void HaltJob(IOperationModel operationModel)
        {
        }

        public virtual void ResumeJob(IOperationModel operationModel)
        {
        }

        public virtual void HaltAllJobs()
        {
            foreach (var key in ActiveJobs.Keys.ToArray())
            {
                HaltJob(key);
            }
        }

        public virtual bool IsJobRunning(IOperationModel operationModel)
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

        public abstract void ExecuteOperationModel(IOperationModel operationModel);
    }
}
