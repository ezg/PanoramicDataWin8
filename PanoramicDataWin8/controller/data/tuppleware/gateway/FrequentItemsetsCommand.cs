using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.tuppleware;

namespace PanoramicDataWin8.controller.data.tuppleware.gateway
{
    public class FrequentItemsetsCommand
    {
        public async Task<JToken> Frequent(TuppleWareOriginModel tuppleWareOriginModel,
            string taskType, string sourceUuid, double support)
        {
            JObject data = new JObject(
                new JProperty("type", "execute"),
                new JProperty("task",
                    new JObject(
                        new JProperty("type", "frequent_itemsets"),
                        new JProperty("source", sourceUuid),
                        new JProperty("support", support))));
            string response = await TuppleWareGateway.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, data);
            JToken jToken = JToken.Parse(response);
            return jToken;
        }
    }
}