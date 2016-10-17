using System.Threading.Tasks;
using IDEA_common.operations;
using Newtonsoft.Json;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class ResultCommand
    {
        public async Task<Result> GetResult(OperationReference reference)
        {
            var response = await IDEAGateway.Request(JsonConvert.SerializeObject(reference, IDEAGateway.JsonSerializerSettings), "result");
            if (response != "null")
            {
                var result = JsonConvert.DeserializeObject<Result>(response, IDEAGateway.JsonSerializerSettings);
                return result;
            }
            return null;
        }
    }
}