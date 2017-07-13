using System.Threading.Tasks;
using IDEA_common.operations;
using Newtonsoft.Json;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class CodeCommand
    {
        public async Task<CompileResults> CompileCode(CodeParameters codeParameters)
        {
            var ser = JsonConvert.SerializeObject(codeParameters, IDEAGateway.JsonSerializerSettings);
            var response = await IDEAGateway.Request(ser, "compile");
            if (response != "null")
            {
                var result = JsonConvert.DeserializeObject<CompileResults>(response, IDEAGateway.JsonSerializerSettings);
                return result;
            }
            return null;
        }
    }
}