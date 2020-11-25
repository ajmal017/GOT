using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GOT.Logic.Enums
{
    [JsonConverter(typeof(StringEnumConverter), new object[] { true })]
    public enum OptionTypes
    {
        /// <summary>
        /// Call option
        /// </summary>
        Call,
        /// <summary>
        /// Put option
        /// </summary>
        Put,
    }
}