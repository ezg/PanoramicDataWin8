using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.tuppleware;

namespace PanoramicDataWin8.controller.data.tuppleware.gateway
{
    public class ClassifyCommand
    {
        public async Task<JToken> Classify(TuppleWareOriginModel tuppleWareOriginModel, 
            JobType jobType, string labelsUuid, string featuresUuid)
        {
            JObject data = new JObject(
                new JProperty("type", "execute"),
                new JProperty("task",
                    new JObject(
                        new JProperty("type", "classify"),
                        new JProperty("classifier", jobType.ToString()),
                        new JProperty("params", new JObject()),
                        new JProperty("label", labelsUuid),
                        new JProperty("features", featuresUuid))));
            string response = await TuppleWareGateway.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, data);
            JObject jObject = JObject.Parse(response);
            return jObject;
        }
    }
}