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
        public async Task<JArray> Classify(TuppleWareOriginModel tuppleWareOriginModel, List<InputFieldModel> features, List<InputFieldModel> labels, JobType jobType)
        {
            JObject data = new JObject(
                new JProperty("command", "classify"),
                new JProperty("classifier", jobType.ToString()),
                new JProperty("project", string.Join(" ", features.Concat(labels).Select(im => im.Name))),
                new JProperty("labels", string.Join(" ", labels.Select(im => im.Name))),
                new JProperty("filename", tuppleWareOriginModel.Name));
            JToken response = await TuppleWareGateway.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, data);
            if (response is JObject)
            {
                JArray arr = new JArray(response);
                return arr;
            }
            else
            {
                return response as JArray;
            }
        }
    }
}