using System;
using GeoAPI.Geometries;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data
{
    public class TaskModel : ExtendedBindableBase
    {
        public static event EventHandler<TaskModelEventArgs> JobTypeViewModelMoved;
        public static event EventHandler<TaskModelEventArgs> JobTypeViewModelDropped;


        public void FireMoved(Rct bounds)
        {
            if (JobTypeViewModelMoved != null)
            {
                JobTypeViewModelMoved(this, new TaskModelEventArgs(bounds));
            }
        }

        public void FireDropped(Rct bounds)
        {
            if (JobTypeViewModelDropped != null)
            {
                JobTypeViewModelDropped(this, new TaskModelEventArgs(bounds));
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

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                this.SetProperty(ref _name, value);
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
    }

    
    public interface JobTypeViewModelEventHandler
    {
        IGeometry BoundsGeometry { get; }
        void JobTypeViewModelMoved(TaskModel sender, TaskModelEventArgs e, bool overElement);
        void JobTypeViewModelDropped(TaskModel sender, TaskModelEventArgs e, bool overElement);
    }


    public class TaskModelEventArgs : EventArgs
    {
        public Rct Bounds { get; set; }
        public TaskModelEventArgs(Rct bounds)
        {
            Bounds = bounds;
        }
    }
}
