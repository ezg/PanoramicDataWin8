using System.Threading.Tasks;
using IDEA_common.catalog;
using IDEA_common.operations.risk;
using Newtonsoft.Json;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class ModelWealthCommand
    {
        public async Task<ModelWealthResult> GetModelWealth(ModelId modelId, RiskControlType riskControlType)
        {
            var response = await IDEAGateway.Request(new ModelWealthParameters()
            {
                ModelId =  modelId,
                RiskControlType = riskControlType
            }, "modelWealth");
            var modelWealthResult = JsonConvert.DeserializeObject<ModelWealthResult>(response, IDEAGateway.JsonSerializerSettings);
            return modelWealthResult;
        }
    }
}