using System.Threading.Tasks;
using IDEA_common.operations;
using Newtonsoft.Json;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class CodeCommand
    {
        public async Task<CodeResult> CompileCode(CodeParameters codeParameters)
        {
            var ser = JsonConvert.SerializeObject(codeParameters, IDEAGateway.JsonSerializerSettings);
            var response = await IDEAGateway.Request(ser, "code");
            if (response != "null")
            {
                var result = JsonConvert.DeserializeObject<CodeResult>(response, IDEAGateway.JsonSerializerSettings);
                return result;
            }
            return null;
        }
    }
}