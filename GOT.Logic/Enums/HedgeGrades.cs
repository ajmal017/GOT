using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GOT.Logic.Enums
{
    [JsonConverter(typeof(StringEnumConverter), new object[] { true })]
    public enum HedgeGrades
    {
        None,
        /// <summary>
        /// Граничный уровень
        /// </summary>
        Border,
        /// <summary>
        /// Уровень выского приоритета
        /// </summary>
        High,
        /// <summary>
        /// Уровень среднего приоритета
        /// </summary>
        Middle,
        /// <summary>
        /// Уровень низкого приоритета
        /// </summary>
        Low
    }
}