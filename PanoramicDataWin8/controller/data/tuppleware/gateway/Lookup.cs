using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.data.tuppleware.json;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.model.data.tuppleware;

namespace PanoramicDataWin8.controller.data.tuppleware.gateway
{
    public class LookupCommand
    {
        public async Task<JToken> Lookup(TuppleWareOriginModel tuppleWareOriginModel, string uuid, int page, int samples)
        {
            //{'type':'lookup','uuid':uuid,'page_size':page_size,'page_num':page_num}
            JObject data = new JObject(
                new JProperty("type", "lookup"),
                new JProperty("uuid", uuid),
                new JProperty("page_size", samples),
                new JProperty("page_num", page));
            string response = await TuppleWareGateway.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, data);
            JToken token = JToken.Parse(response);
            return token;
        }
    }
}