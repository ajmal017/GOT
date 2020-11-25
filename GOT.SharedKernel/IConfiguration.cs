using System;
using GOT.SharedKernel.Enums;

namespace GOT.SharedKernel
{
    public interface IConfiguration
    {
        public ConnectorTypes ConnectorType { get; set; }
        public string Email { get; set; }

        /// <summary>
        ///     "Interactive broker" host address.
        /// </summary>
        public string IbHost { get; set; }

        /// <summary>
        ///     "Interactive brokers" host port
        /// </summary>
        public int IbPort { get; set; }

        /// <summary>
        ///     "Interactive brokers" client Id
        /// </summary>
        public int IbClientId { get; set; }

        /// <summary>
        ///     "Interactive brokers" DataType
        ///     тип рыночных данных
        ///     1 - Live (Текущие рыночные данные, предоставляются по подписке)
        ///     2 - Frozen (Данные замороженного рынка - это последние данные, записанные при закрытии рынка)
        ///     3 - Delayed (Бесплатные данные с задержкой - 15–20 минут.)
        ///     4 - Delayed Frozen (Запросы задержанных «замороженных» данных для пользователя без подписки на рыночные данные)
        /// </summary>
        public int DataType { get; set; }

        public long TelegramId { get; set; }
        public string TelegramProxy { get; set; }
        public int TelegramHost { get; set; }
        public bool IsAutoConnect { get; set; }
        public event Action<IConfiguration> ConfigurationChanged;
        public void OnConfigurationChange(IConfiguration configuration);
    }
}