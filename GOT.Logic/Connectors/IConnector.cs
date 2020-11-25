using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GOT.Logic.DataTransferObjects;
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
        ///     Получить список доступных опцион-инструментов.
        /// </summary>
        /// <param name="baseInstrumentCode">Код базового инструмента</param>
        /// <param name="exchange">Биржа базового инструмента</param>
        /// <returns></returns>
        IReadOnlyList<Option> GetOptions(string baseInstrumentCode, string exchange = "");
        Task<IReadOnlyList<Option>> GetOptionsAsync(string baseInstrumentCode, string exchange = "");
        void RequestInstrument(string instrumentCode, InstrumentTypes type);

        /// <summary>
        ///     Получить список доступных фьючерс-инструментов
        /// </summary>
        /// <param name="instrumentSymbol">Код(символ) базового инструмента</param>
        /// <returns></returns>
        Task<IReadOnlyList<Future>> GetFuturesAsync(string instrumentSymbol);

        IEnumerable<string> GetInstrumentCodes();

        /// <summary>
        ///     Получить список доступных счетов
        /// </summary>
        IEnumerable<Account> GetAccounts();

        void RequestMarketDataType(int type);
        /// <summary>
        ///     Подписывается на данные по инструменту
        /// </summary>
        /// <param name="instrument">Инструмент.</param>
        void SubscribeInstrument(Instrument instrument);

        /// <summary>
        ///     Отписывается от данных по инструменту
        /// </summary>
        /// <param name="id">id инструмента</param>
        void DescribeInstrument(int id);

        /// <summary>
        ///     Создает заявку и отправляет на биржу
        /// </summary>
        /// <param name="strategyId"></param>
        /// <param name="instrument">Инструмент <see cref="Future" />,<see cref="Option" /></param>
        /// <param name="direction">Направление заявки</param>
        /// <param name="volume">Объем заявки</param>
        /// <param name="price">Цена заявки</param>
        /// <param name="description">Описание заявки</param>
        void SendOrder<T>(Guid strategyId, T instrument, Directions direction, int volume, decimal price = decimal.Zero,
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