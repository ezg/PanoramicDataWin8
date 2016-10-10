using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDEA_common.catalog;
using IDEA_common.operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.input;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class ResultCommand
    {
        public async Task<Result> GetResult(OperationReference reference)
        {
            string response = await IDEAGateway.Request(JsonConvert.SerializeObject(reference, IDEAGateway.JsonSerializerSettings), "result");
            if (response != "null")
            {
                var result = JsonConvert.DeserializeObject<Result>(response, IDEAGateway.JsonSerializerSettings);
                return result;
            }
            return null;
        }
    }
}
