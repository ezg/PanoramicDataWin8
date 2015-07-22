using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.tuppleware;

namespace PanoramicDataWin8.controller.data.tuppleware.gateway
{
    public class DataCommand
    {
        public async Task<JArray> GetData(TuppleWareOriginModel tuppleWareOriginModel, List<InputFieldModel> inputModels, string select, int page, int samples)
        {
            JObject data = new JObject(
                new JProperty("command", "data"),
                new JProperty("project", string.Join(" ", inputModels.Select(im => im.Name).Distinct())),
                new JProperty("limit", samples),
                new JProperty("page", page),
                new JProperty("filename", tuppleWareOriginModel.Name));
            if (!string.IsNullOrEmpty(select))
            {
                data.Add(new JProperty("select", select));
            }
            JToken response = await TuppleWareGateway.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, data);

            return response as JArray;
        }
    }
}