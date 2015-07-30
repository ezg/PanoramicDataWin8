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
        public async void Classify(TuppleWareOriginModel tuppleWareOriginModel, 
            JobType jobType, long labelsUuid, long featuresUuid, long uuid)
        {
            JObject data = new JObject(
                new JProperty("type", "execute"),
                new JProperty("task",
                    new JObject(
                        new JProperty("type", "classify"),
                        new JProperty("classifier", jobType.ToString()),
                        new JProperty("params", new JObject()),
                        new JProperty("labels", labelsUuid),
                        new JProperty("features", featuresUuid),
                        new JProperty("uuid", uuid))));
            await TuppleWareGateway.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, data);
        }
    }
}