using System;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class ExecuteOperationModelEventArgs : EventArgs
    {
        public ExecuteOperationModelEventArgs(IOperationModel operationModel, bool stopPreviousExecution)
        {
            OperationModel = operationModel;
            StopPreviousExecution = stopPreviousExecution;
        }

        public IOperationModel OperationModel { get; set; }
        public bool StopPreviousExecution { get; set; }
    }
}