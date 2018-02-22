using System;
using PanoramicDataWin8.model.data.operation;
using System.Linq;
using PanoramicDataWin8.controller.data.progressive;
using PanoramicDataWin8.model.data.operation.computational;

namespace PanoramicDataWin8.controller.data.idea
{
    class AttributeOperationJob : OperationJob
    {
        public AttributeOperationJob(OperationModel operationModel, int sampleSize, int rawDataSize) : base(operationModel)
        {
            OperationParameters = IDEAHelpers.GetAttributeOperationParameters((AttributeOperationModel)operationModel, sampleSize);
        }
    }
}
