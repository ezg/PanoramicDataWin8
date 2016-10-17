using System.ComponentModel;
using IDEA_common.operations;

namespace PanoramicDataWin8.model.data.operation
{
    public interface IOperationModel : INotifyPropertyChanged
    {
        IResult Result { get; set; }

        IOperationModel ResultCauserClone { get; set; }

        SchemaModel SchemaModel { get; set; }
        event OperationModel.OperationModelUpdatedHandler OperationModelUpdated;
        void FireOperationModelUpdated(OperationModelUpdatedEventArgs args);

        OperationModel Clone();
    }
}