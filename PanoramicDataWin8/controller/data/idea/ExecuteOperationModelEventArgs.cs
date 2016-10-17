using System;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class ExecuteOperationModelEventArgs : EventArgs
    {
        public ExecuteOperationModelEventArgs(IOperationModel operationModel)
        {
            OperationModel = operationModel;
        }

        public IOperationModel OperationModel { get; set; }
    }
}