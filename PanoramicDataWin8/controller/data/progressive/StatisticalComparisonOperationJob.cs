using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDEA_common.aggregates;
using IDEA_common.binning;
using IDEA_common.operations.histogram;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
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
