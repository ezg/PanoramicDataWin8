using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.controller.data
{
    public abstract class Job
    {
        public event EventHandler<JobEventArgs> JobUpdate;
        public event EventHandler<EventArgs> JobCompleted;

        public abstract void Start();
        public abstract void Stop();

        protected void FireJobUpdated(JobEventArgs jobEventArgs)
        {
            if (JobUpdate != null)
            {
                JobUpdate(this, jobEventArgs);
            }
        }

        protected void FireJobCompleted(EventArgs eventArgs)
        {
            if (JobCompleted != null)
            {
                JobCompleted(this, eventArgs);
            }
        }
    }
}
