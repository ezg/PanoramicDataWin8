using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PanoramicDataWin8.model.data.result
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResultType
    {
        Clear,
        Update,
        Complete
    }
}