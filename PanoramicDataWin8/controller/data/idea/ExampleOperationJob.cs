using System;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class ExampleOperationJob : OperationJob
    {
        public ExampleOperationJob(OperationModel operationModel,
             int sampleSize) : base(operationModel)
        {
            OperationParameters = IDEAHelpers.GetExampleOperationParameters((ExampleOperationModel) operationModel, sampleSize);
        }
    }
}