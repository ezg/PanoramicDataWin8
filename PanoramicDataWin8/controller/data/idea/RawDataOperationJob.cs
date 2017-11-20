using System;
using PanoramicDataWin8.model.data.operation;
using System.Linq;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class RawDataOperationJob : OperationJob
    {
        public RawDataOperationJob(OperationModel operationModel, int sampleSize) : base(operationModel)
        {
            bool computeAsHistogram = OperationModel.AttributeTransformationModelParameters.Where((atm) => atm.GroupBy).Any();
            if (computeAsHistogram)
                OperationParameters = IDEAHelpers.GetRawDataComputedOperationParameters((RawDataOperationModel)operationModel, sampleSize);
            else OperationParameters = IDEAHelpers.GetRawDataOperationParameters((RawDataOperationModel)operationModel, sampleSize);
        }
    }
}