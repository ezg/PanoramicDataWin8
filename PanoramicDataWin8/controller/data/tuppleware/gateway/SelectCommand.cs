using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.data.tuppleware.json;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.tuppleware;

namespace PanoramicDataWin8.controller.data.tuppleware.gateway
{
    public class SelectCommand
    {
        public async Task<JToken> Select(TuppleWareOriginModel tuppleWareOriginModel, string sourceUuid, string predicate)
        {
            JObject data = new JObject(
                new JProperty("type", "execute"),
                new JProperty("task",
                    new JObject(
                        new JProperty("type", "select"),
                        new JProperty("source", sourceUuid),
                        new JProperty("predicate", predicate))));
            string response = await TuppleWareGateway.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, data);
            JObject jObject = JObject.Parse(response);
            return jObject;
        }
    }
}