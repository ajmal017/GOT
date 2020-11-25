using System;
using GOT.SharedKernel.Enums;
using GOT.SharedKernel.Utils.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GOT.SharedKernel
{
    /// <summary>
    ///     Отвечает за основные настройки.
    /// </summary>
    public class Configuration : IConfiguration
    {
        [JsonProperty("connectorType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ConnectorTypes ConnectorType { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; } = "donaldpolaroid@hotmail.com";

        [JsonProperty("ib_host")]
        public string IbHost { get; set; } = "127.0.0.1";

        [JsonProperty("ib_port")]
        public int IbPort { get; set; } = 7497;

        [JsonProperty("ib_clientId")]
        public int IbClientId { get; set; }

        [JsonProperty("data_type")]
        public int DataType { get; set; } = 3;

        [JsonProperty("telegramId")]
        public long TelegramId { get; set; }

        public string TelegramProxy { get; set; }
        public int TelegramHost { get; set; }

        [JsonProperty("isAutoConnect")]
        public bool IsAutoConnect { get; set; }

        public event Action<IConfiguration> ConfigurationChanged;

        public void OnConfigurationChange(IConfiguration configuration)
        {
            JsonHelper.SerializeToJsonFile(configuration, FolderBuilder.GetConfigurationPath());
            ConfigurationChanged?.Invoke(configuration);
        }
    }
}