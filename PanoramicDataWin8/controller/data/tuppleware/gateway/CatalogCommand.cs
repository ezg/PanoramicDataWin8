using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.data.tuppleware.json;
using PanoramicDataWin8.controller.input;

namespace PanoramicDataWin8.controller.data.tuppleware.gateway
{
    public class CatalogCommand
    {
        public async Task<List<DatasetConfiguration>> GetCatalog(string url)
        {
            JObject data = new JObject(
                new JProperty("type", "catalog"));
            string response = await TuppleWareGateway.Request(data);

            JToken jToken = JToken.Parse(response);

            List<DatasetConfiguration> dataSets = new List<DatasetConfiguration>();
            foreach (var child in jToken)
            {
                var schemaName = ((JProperty) child).Name;
                var schemaJson = ((JProperty) child).Value;
                var dataSetConfig = new DatasetConfiguration
                {
                    Name = schemaName,
                    SchemaJson = schemaJson,
                    Backend = "tuppleware",
                    BaseUUID = ((JProperty) child).Value["uuid"].Value<string>()
                };
                dataSets.Add(dataSetConfig);
            }
            return dataSets;
        }
    }
}