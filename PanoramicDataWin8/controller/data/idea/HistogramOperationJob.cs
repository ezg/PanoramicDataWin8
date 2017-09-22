using System;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class HistogramOperationJob : OperationJob
    {
        public HistogramOperationJob(OperationModel operationModel,
             int sampleSize) : base(operationModel)
        {
            OperationParameters = IDEAHelpers.GetHistogramOperationParameters((HistogramOperationModel) operationModel, sampleSize);
        }
    }
}