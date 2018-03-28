using System.ComponentModel;
using IDEA_common.operations;
using System.Collections.ObjectModel;

namespace PanoramicDataWin8.model.data.operation
{
    public interface IOperationModel : INotifyPropertyChanged
    {
        IResult Result { get; set; }
        int ExecutionId { get; set; }
        ResultParameters ResultParameters { get; }

        void Dispose();

        IOperationModel ResultCauserClone { get; set; }
        
        OriginModel OriginModel { get; set; }
        event OperationModel.OperationModelUpdatedHandler OperationModelUpdated;
        void FireOperationModelUpdated(OperationModelUpdatedEventArgs args);

        OperationModel Clone();
    }
}