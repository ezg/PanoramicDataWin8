using System;
using IDEA_common.operations.risk;
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

    public class RiskOperationJob : OperationJob
    {
        public RiskOperationJob(OperationModel operationModel,
            TimeSpan throttle) : base(operationModel, throttle)
        {
            OperationParameters = new NewModelOperationParameters()
            {
                //ModelType = ((RiskOperationModel)operationModel).ModelType
            };
        }
    }
}