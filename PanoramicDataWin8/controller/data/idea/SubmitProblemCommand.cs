using System.Threading.Tasks;
using IDEA_common.catalog;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class SubmitProblemCommand
    {
        public async Task SumbitResult(PredictorOperationModel model)
        {
            var ser = JsonConvert.SerializeObject(model, IDEAGateway.JsonSerializerSettings);
            var response = await IDEAGateway.Request(ser, "submitProblem");
        }
    }
}