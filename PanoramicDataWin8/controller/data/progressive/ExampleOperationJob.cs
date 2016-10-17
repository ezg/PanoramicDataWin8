using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Composition;
using Windows.UI.Core;
using IDEA_common.aggregates;
using IDEA_common.binning;
using IDEA_common.operations;
using IDEA_common.operations.histogram;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.result;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class ExampleOperationJob : OperationJob
    {
        public ExampleOperationJob(OperationModel operationModel,  
            TimeSpan throttle, int sampleSize) : base(operationModel, throttle)
        {
            OperationParameters = IDEAHelpers.GetExampleOperationParameters((ExampleOperationModel)operationModel, sampleSize);
        }
       
    }
}
