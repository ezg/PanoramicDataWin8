using System;
using GeoAPI.Geometries;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data
{
    public class OperationTypeModel : ExtendedBindableBase
    {
        public static event EventHandler<OperationTypeModelEventArgs> OperationTypeModelMoved;
        public static event EventHandler<OperationTypeModelEventArgs> OperationTypeModelDropped;


        public void FireMoved(Rct bounds)
        {
            OperationTypeModelMoved?.Invoke(this, new OperationTypeModelEventArgs(bounds));
        }

        public void FireDropped(Rct bounds)
        {
            OperationTypeModelDropped?.Invoke(this, new OperationTypeModelEventArgs(bounds));
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

        private OperationType _operationType;
        public OperationType OperationType
        {
            get
            {
                return _operationType;
            }
            set
            {
                this.SetProperty(ref _operationType, value);
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

    public enum OperationType
    {
        Group, Histogram, Example
    }


    public interface OperationTypeModelEventHandler
    {
        IGeometry BoundsGeometry { get; }
        void OperationTypeModelMoved(OperationTypeModel sender, OperationTypeModelEventArgs e, bool overElement);
        void OperationTypeModelDropped(OperationTypeModel sender, OperationTypeModelEventArgs e, bool overElement);
    }


    public class OperationTypeModelEventArgs : EventArgs
    {
        public Rct Bounds { get; set; }
        public OperationTypeModelEventArgs(Rct bounds)
        {
            Bounds = bounds;
        }
    }
}
