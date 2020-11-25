using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GOT.Logic.Enums
{
    [JsonConverter(typeof(StringEnumConverter), new object[] {true})]
    public enum Directions : byte
    {
        Sell,
        Buy
    }
}