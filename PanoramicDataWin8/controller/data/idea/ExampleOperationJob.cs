using System;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class ExampleOperationJob : OperationJob
    {
        public ExampleOperationJob(OperationModel operationModel,
            TimeSpan throttle, int sampleSize) : base(operationModel, throttle)
        {
            OperationParameters = IDEAHelpers.GetExampleOperationParameters((ExampleOperationModel) operationModel, sampleSize);
        }
    }
}