using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDEA_common.operations;
using PanoramicDataWin8.model.data.result;

namespace PanoramicDataWin8.controller.data
{
    public abstract class Job
    {
        public event EventHandler<JobEventArgs> JobUpdate;
        public event EventHandler<JobEventArgs> JobCompleted;

        public abstract void Start();
        public abstract void Stop();

        protected void FireJobUpdated(JobEventArgs jobEventArgs)
        {
            if (JobUpdate != null)
            {
                JobUpdate(this, jobEventArgs);
            }
        }

        protected void FireJobCompleted(JobEventArgs jobEventArgs)
        {
            if (JobCompleted != null)
            {
                JobCompleted(this, jobEventArgs);
            }
        }
    }


    public class JobEventArgs : EventArgs
    {
        public IResult Result { get; set; }
    }
}
