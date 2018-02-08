using System;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.model.data
{
    public class OperationTypeModel : ExtendedBindableBase
    {
        private bool _isShadow;

        private string _name;

        private OperationType _operationType;

        private FunctionOperationModel _functionTypeModel;

        private Vec _size = new Vec(50, 50);

        public bool IsShadow
        {
            get { return _isShadow; }
            set { SetProperty(ref _isShadow, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public OperationType OperationType
        {
            get { return _operationType; }
            set { SetProperty(ref _operationType, value); }
        }

        public FunctionOperationModel FunctionType
        {
            get { return _functionTypeModel; }
            set { SetProperty(ref _functionTypeModel, value); }
        }

        public Vec Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

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
    }
}