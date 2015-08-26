using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.data.tuppleware.json;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.model.data.tuppleware;

namespace PanoramicDataWin8.controller.data.tuppleware.gateway
{
    public class CodeGenCommand
    {
        public async Task<string> CodeGen(TuppleWareOriginModel tuppleWareOriginModel, string uuid)
        {
            //curl -H "Content-Type: application/json" -X POST -d '{"type":"codegen","uuid":"X"}' localhost:8080

            JObject data = new JObject(
                new JProperty("type", "codegen"), 
                new JProperty("uuid", uuid));
            string response = await TuppleWareGateway.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, data);
            return response;
        }
    }
}