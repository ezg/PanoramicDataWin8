using System;
using System.Linq;
using IDEA_common.operations.risk;
using IDEA_common.util;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class RiskOperationJob : OperationJob
    {
        public RiskOperationJob(OperationModel operationModel,
            TimeSpan throttle) : base(operationModel, throttle)
        {
            OperationParameters = new NewModelOperationParameters()
            {
                //RiskControlTypes = ((RiskOperationModel)operationModel).RiskControlTypes,
                RiskControlTypes = HypothesesViewController.SupportedRiskControlTypes,
                Alpha = ((RiskOperationModel)operationModel).Alpha
            };
        }
    }
}