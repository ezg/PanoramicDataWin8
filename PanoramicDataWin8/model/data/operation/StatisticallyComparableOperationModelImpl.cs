using System;
using IDEA_common.operations;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.operation
{
    public class StatisticallyComparableOperationModelImpl : ExtendedBindableBase,
        IStatisticallyComparableOperationModel
    {
        private readonly IOperationModel _host;

        private bool _includeDistribution;

        public StatisticallyComparableOperationModelImpl(IOperationModel host)
        {
            _host = host;
        }

        public event OperationModel.OperationModelUpdatedHandler OperationModelUpdated;

        public void FireOperationModelUpdated(OperationModelUpdatedEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void Cleanup() { }

        public IResult Result { get; set; }
        public int ExecutionId { get; set; } = 0;
        public ResultParameters ResultParameters { get; }
        public IOperationModel ResultCauserClone { get; set; }
        public SchemaModel SchemaModel { get; set; }

        public OperationModel Clone()
        {
            throw new NotImplementedException();
        }

        public bool IncludeDistribution
        {
            get { return _includeDistribution; }
            set
            {
                SetProperty(ref _includeDistribution, value);
               // _host.FireOperationModelUpdated(new VisualOperationModelUpdatedEventArgs());
            }
        }
    }
}