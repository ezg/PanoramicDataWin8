using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDEA_common.catalog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.input;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class CatalogCommand
    {
        public async Task<Catalog> GetCatalog()
        {
            string response = await IDEAGateway.Request(null, "catalog");
            Catalog catalog = JsonConvert.DeserializeObject<Catalog>(response, IDEAGateway.JsonSerializerSettings);
            return catalog;
        }
    }
}
