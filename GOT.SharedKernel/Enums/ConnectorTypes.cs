using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GOT.SharedKernel.Enums
{
    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum ConnectorTypes : byte
    {
        IB = 0,
        CQG = 1,
        Quik = 2
    }
}