using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GOT.Logic.DTO;
using GOT.Logic.Enums;
using GOT.Logic.Models;
using GOT.Logic.Models.Instruments;
using GOT.SharedKernel.Enums;

namespace GOT.Logic.Connectors
{
    public interface IConnector
    {
        /// <summary>
        ///     Состояние подключения к поставщику данных
        /// </summary>
        ConnectionStates ConnectionState { get; }

        /// <summary>
        ///     Тип поставщика данных
        /// </summary>
        ConnectorTypes ConnectorType { get; }

        event Action<InstrumentDTO> FutureChanged;
        event Action<InstrumentDTO> OptionChanged;

        event Action<Order> OrderChanged;

        event Action<ConnectionStates> ConnectionStateChanged;

        event Action<ConnectionStates> GatewayStateChanged;

        /// <summary>
        ///     Подключение к поставщику данных
        /// </summary>
        void Connect();

        /// <summary>
        ///     Отключение от поставщика данных
        /// </summary>
        void Disconnect();

        /// <summary>
        ///     Отменяет подписку и удаляет инструмент.
        /// </summary>
        /// <param name="instrument">Инструмент для удаления</param>
        void RemoveInstrument(Instrument instrument);

        /// <summary>
        /// Сделать запрос информации по базовому инструменту.
        /// </summary>
        /// <param name="instrumentCode"></param>
        void RequestInstrument(string instrumentCode);

        /// <summary>
        ///     Получить цепочку доступных опцион-инструментов.
        /// </summary>
        /// <param name="baseInstrument">базовый инструмент</param>
        /// <returns></returns>
        IEnumerable<Option> GetOptions(Future baseInstrument);

        /// <summary>
        ///     Получить список доступных фьючерс-инструментов
        /// </summary>
        /// <param name="instrumentSymbol">Код(символ) базового инструмента</param>
        /// <returns></returns>
        Task<IReadOnlyList<Future>> GetFuturesAsync(string instrumentSymbol);

        /// <summary>
        /// Получить список кодов доступных инструментов
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetInstrumentCodes();

        /// <summary>
        ///     Получить список доступных счетов
        /// </summary>
        IEnumerable<Account> GetAccounts();

        /// <summary>
        ///     Запрос на тип рыночных данных. Необходимо указать перед использованием <see cref="SubscribeInstrument" />
        ///     ClientSocket.reqMktData();
        /// </summary>
        /// <param name="type">
        ///     тип рыночных данных
        ///     1 - Live (Текущие рыночные данные, предоставляются по подписке)
        ///     2 - Frozen (Данные замороженного рынка - это последние данные, записанные при закрытии рынка)
        ///     3 - Delayed (Бесплатные данные с задержкой - 15–20 минут.)
        ///     4 - Delayed Frozen (Запросы задержанных «замороженных» данных для пользователя без подписки на рыночные данные)
        /// </param>
        /// <remarks>https://interactivebrokers.github.io/tws-api/market_data_type.html</remarks>
        void RequestMarketDataType(int type);
       
        /// <summary>
        ///     Подписка на данные по инструменту. используется вместе с <see cref="RequestMarketDataType" />.
        /// </summary>
        /// <param name="instrument">Инструмент на который необходимо подписаться.</param>
        void SubscribeInstrument(Instrument instrument);

        /// <summary>
        ///     Отписывается от данных по инструменту
        /// </summary>
        /// <param name="id">id инструмента</param>
        void UnsubscribeInstrument(int id);

        /// <summary>
        ///     Создает заявку и отправляет на биржу
        /// </summary>
        /// <param name="strategyId"></param>
        /// <param name="instrument">Инструмент <see cref="Future" />,<see cref="Option" /></param>
        /// <param name="account">Account <see cref="Account" /></param>
        /// <param name="direction">Направление заявки</param>
        /// <param name="volume">Объем заявки</param>
        /// <param name="price">Цена заявки</param>
        /// <param name="description">Описание заявки</param>
        void SendOrder<T>(Guid strategyId, T instrument, string account, Directions direction, int volume,
            decimal price = decimal.Zero,
            string description = "")
            where T : Instrument;

        /// <summary>
        ///     Отменяет все заявки у указанной стратегии
        /// </summary>
        /// <param name="strategyId">идентификатор стратегии, по которой искать заявку <see cref="Order" /></param>
        void CancelAllOrders(Guid strategyId);

        /// <summary>
        ///     Отменяет указанную заявку
        /// </summary>
        /// <param name="id">идентификатор заявки<see cref="Order" /></param>
        void CancelOrder(int id);
    }
}