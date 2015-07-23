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
    public class ProjecCommand
    {
        public async void Project(TuppleWareOriginModel tuppleWareOriginModel, long uuid, long sourceUuid, List<InputFieldModel> inputModels)
        {
            JObject data = new JObject(
                new JProperty("type", "execute"),
                new JProperty("task",
                    new JObject(
                        new JProperty("type", "project"),
                        new JProperty("uuid", uuid),
                        new JProperty("source", sourceUuid),
                        new JProperty("attributes", 
                            new JArray(inputModels.Select(im => im.Name).Distinct())))));
            await TuppleWareGateway.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, data);
        }
    }
}