using GeoAPI.Geometries;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicData.controller.view;
using PanoramicData.model.data;
using PanoramicData.utils;
using PanoramicDataWin8.model.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.view
{
    public class JobTypeViewModel : BindableBase
    {
        public static event EventHandler<JobTypeViewModelEventArgs> JobTypeViewModelMoved;
        public static event EventHandler<JobTypeViewModelEventArgs> JobTypeViewModelDropped;

        public JobTypeViewModel() { }


        public void FireMoved(Rct bounds)
        {
            if (JobTypeViewModelMoved != null)
            {
                JobTypeViewModelMoved(this, new JobTypeViewModelEventArgs(bounds));
            }
        }

        public void FireDropped(Rct bounds)
        {
            if (JobTypeViewModelDropped != null)
            {
                JobTypeViewModelDropped(this, new JobTypeViewModelEventArgs(bounds));
            }
        }

        private JobType _jobType;
        public JobType JobType
        {
            get
            {
                return _jobType;
            }
            set
            {
                _mainLabel = value.ToString();
                this.SetProperty(ref _jobType, value);
            }
        }

        private bool _isShadow = false;
        public bool IsShadow
        {
            get
            {
                return _isShadow;
            }
            set
            {
                this.SetProperty(ref _isShadow, value);
            }
        }

        private Vec _size = new Vec(50, 50);
        public Vec Size
        {
            get
            {
                return _size;
            }
            set
            {
                this.SetProperty(ref _size, value);
            }
        }
        
        private string _mainLabel = null;
        public string MainLabel
        {
            get
            {
                return _mainLabel;
            }
            set
            {
                this.SetProperty(ref _mainLabel, value);
            }
        }

        private string _sublabel = null;
        public string SubLabel
        {
            get
            {
                return _sublabel;
            }
            set
            {
                this.SetProperty(ref _sublabel, value);
            }
        }
    }



    public interface JobTypeViewModelEventHandler
    {
        IGeometry BoundsGeometry { get; }
        void JobTypeViewModelMoved(JobTypeViewModel sender, JobTypeViewModelEventArgs e, bool overElement);
        void JobTypeViewModelDropped(JobTypeViewModel sender, JobTypeViewModelEventArgs e, bool overElement);
    }


    public class JobTypeViewModelEventArgs : EventArgs
    {
        public Rct Bounds { get; set; }
        public JobTypeViewModelEventArgs(Rct bounds)
        {
            Bounds = bounds;
        }
    }
}
