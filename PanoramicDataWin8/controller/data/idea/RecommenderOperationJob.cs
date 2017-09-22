using System;
using IDEA_common.operations.risk;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class RecommenderOperationJob : OperationJob
    {
        public RecommenderOperationJob(OperationModel operationModel,
             int sampleSize) : base(operationModel)
        {
            OperationParameters = IDEAHelpers.GetOptimizerOperationParameters((PredictorOperationModel)operationModel, sampleSize);
        }
    }
}