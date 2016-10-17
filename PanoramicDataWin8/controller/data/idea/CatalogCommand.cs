using System.Threading.Tasks;
using IDEA_common.catalog;
using Newtonsoft.Json;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class CatalogCommand
    {
        public async Task<Catalog> GetCatalog()
        {
            var response = await IDEAGateway.Request(null, "catalog");
            var catalog = JsonConvert.DeserializeObject<Catalog>(response, IDEAGateway.JsonSerializerSettings);
            return catalog;
        }
    }
}