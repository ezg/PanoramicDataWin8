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
    public class ProjectCommand
    {
        public async Task<JToken> Project(TuppleWareOriginModel tuppleWareOriginModel, string sourceUuid, List<InputFieldModel> inputModels)
        {
            JObject data = new JObject(
                new JProperty("type", "execute"),
                new JProperty("task",
                    new JObject(
                        new JProperty("type", "project"),
                        new JProperty("source", sourceUuid),
                        new JProperty("attributes", 
                            new JArray(inputModels.Select(im => im.Name).Distinct())))));
            string response = await TuppleWareGateway.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, data);
            JObject jObject = JObject.Parse(response);
            return jObject;
        }
    }
}