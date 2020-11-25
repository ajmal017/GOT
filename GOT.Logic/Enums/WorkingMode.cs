using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GOT.Logic.Enums
{
    [JsonConverter(typeof(StringEnumConverter), new object[] { true })]
    public enum WorkingMode
    {
        /// <summary>
        /// Режим открытием позиций
        /// </summary>
        OpenPosition,
        /// <summary>
        /// Режим закрытия позиций
        /// </summary>
        ClosePosition
    }
}