using System.Collections.Generic;
using System.Linq;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data
{
    public abstract class QueryExecuter
    {
        public Dictionary<IOperationModel, Dictionary<int, OperationJob>> ActiveJobs { get; set; } = new Dictionary<IOperationModel, Dictionary<int, OperationJob>>();
        
        public virtual void HaltJob(IOperationModel operationModel)
        {
        }

        public virtual void ResumeJob(IOperationModel operationModel)
        {
        }
        
        public virtual void UpdateResultParameters(IOperationModel operationModel)
        {
        }

        public virtual void HaltAllJobs()
        {
            foreach (var key in ActiveJobs.Keys.ToArray())
                HaltJob(key);
        }

        public virtual bool IsJobRunning(IOperationModel operationModel)
        {
            if (ActiveJobs.ContainsKey(operationModel))
                return true;
            return false;
        }

        public abstract void ExecuteOperationModel(IOperationModel operationModel, bool stopPreviousExecutions);
    }
}