using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GOT.Logic.Enums
{
    [JsonConverter(typeof(StringEnumConverter), new object[] { true })]
    public enum LifetimeOptions
    {
        /// <summary>
        /// Месячный опцион
        /// </summary>
        Month,
        /// <summary>
        /// Недельный опцион
        /// </summary>
        Week
    }
}