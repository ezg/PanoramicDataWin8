using System;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class StatisticalComparisonOperationJob : OperationJob
    {
        public StatisticalComparisonOperationJob(OperationModel operationModel,
            TimeSpan throttle, int sampleSize) : base(operationModel, throttle)
        {
            var model = (StatisticalComparisonOperationModel) operationModel.ResultCauserClone;
            if (model.StatistalComparisonType == StatistalComparisonType.distribution)
            {
            }
            else if (model.StatistalComparisonType == StatistalComparisonType.histogram)
            {
                OperationParameters = IDEAHelpers.GetChiSquaredTestOperationParameters((StatisticalComparisonOperationModel) operationModel.ResultCauserClone, sampleSize);
            }
        }
    }
}