using System;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class RawDataOperationJob : OperationJob
    {
        public RawDataOperationJob(OperationModel operationModel, int sampleSize) : base(operationModel)
        {
            OperationParameters = IDEAHelpers.GetRawDataOperationParameters((RawDataOperationModel)operationModel, sampleSize);
        }
    }
}