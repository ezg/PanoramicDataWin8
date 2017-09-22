using System;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class StatisticalComparisonDecisionOperationJob : OperationJob
    {
        public StatisticalComparisonDecisionOperationJob(OperationModel operationModel) : base(operationModel)
        {
            var model = (StatisticalComparisonDecisionOperationModel)operationModel.ResultCauserClone;
            OperationParameters = IDEAHelpers.GetModelStateParameters(model);
        }
    }
}