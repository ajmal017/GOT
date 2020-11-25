using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using GOT.Logic.DataTransferObjects;
using GOT.Logic.Enums;
using GOT.Logic.Models;
using GOT.Logic.Models.Instruments;
using GOT.Logic.Utils.Helpers;
using GOT.Notification;
using GOT.SharedKernel;
using GOT.SharedKernel.Enums;
using IBApi;
using Order = GOT.Logic.Models.Order;

namespace GOT.Logic.Connectors.InteractiveBrokers
{
    public class IbConnector : DefaultEWrapper, IConnector
    {
        private readonly IList<Instrument> _cacheInstruments = new List<Instrument>();
        private readonly IbCodeHandler _codeHandler = new IbCodeHandler();
        private readonly IGotLogger _logger;
        private readonly INotification _notification;
        private readonly EReaderMonitorSignal _signal = new EReaderMonitorSignal();
        private IConfiguration _configuration;

        /// <summary>
        ///     Следующий id запроса. Не влияет на работу, необходим только для отправки нового запроса.
        /// </summary>
        private int _nextRequestId;

        public IbConnector(IGotLogger logger, INotification notification, IConfiguration configuration) : this(
            logger, notification)
        {
            UpdateConfig(configuration);
        }

        private IbConnector(IGotLogger logger, INotification notification)
        {
            _logger = logger;
            _notification = notification;
            ClientSocket = new EClientSocket(this, _signal);
        }

        private EClientSocket ClientSocket { get; }

        public ConnectionStates GatewayState { get; private set; }

        public ConnectorTypes ConnectorType { get; } = ConnectorTypes.IB;
        public event Action<InstrumentDTO> FutureChanged;
        public event Action<InstrumentDTO> OptionChanged;
        public event Action<Order> OrderChanged;

        public ConnectionStates ConnectionState { get; private set; }

        public event Action<ConnectionStates> ConnectionStateChanged;

        public event Action<ConnectionStates> GatewayStateChanged;

        public void Connect()
        {
            try {
                ClientSocket.eConnect(_configuration.IbHost, _configuration.IbPort, _configuration.IbClientId);

                var reader = new EReader(ClientSocket, _signal);
                reader.Start();

                new Thread(() =>
                {
                    while (ClientSocket.IsConnected()) {
                        _signal.waitForSignal();
                        reader.processMsgs();
                    }
                })
                {
                    IsBackground = true
                }.Start();
            }
            catch (Exception e) {
                _logger.AddLog($"Please check your connection attributes. {e.Message}", 2);
                _notification.SendMessageAsync($"Please check your connection attributes. {e.Message}");
            }
        }

        public void Disconnect()
        {
            ClientSocket.eDisconnect();
            OnConnectionStateChanged(ConnectionStates.Disconnected);
        }

        /// <inheritdoc />
        public void RemoveInstrument(Instrument instrument)
        {
            if (_cacheInstruments.All(i => i.Id != instrument.Id)) {
                return;
            }

            ClientSocket.cancelMktData(instrument.Id);
            _cacheInstruments.Remove(instrument);
        }

        public IReadOnlyList<Option> GetOptions(string baseInstrumentCode, string exchange = "")
        {
            IReadOnlyList<Option> cache = _cacheInstruments.Where(instr =>
                                                               instr.InstrumentType == InstrumentTypes.Options &&
                                                               instr.Exchange == exchange &&
                                                               instr.Symbol == baseInstrumentCode)
                                                           .Select(instr => instr as Option).ToList();
            return cache;
        }

        public Task<IReadOnlyList<Option>> GetOptionsAsync(string baseInstrumentCode, string exchange = "")
        {
            var cache = GetOptions(baseInstrumentCode, exchange);
            if (cache.Any()) {
                return Task.FromResult(cache);
            }

            var taskSource = new TaskCompletionSource<IReadOnlyList<Option>>();
            RequestEnded += _ =>
            {
                var options = GetOptions(baseInstrumentCode, exchange);
                taskSource.TrySetResult(options);
            };

            var token = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            token.Token.Register(() => taskSource.TrySetCanceled());
            RequestInstrument(baseInstrumentCode, InstrumentTypes.Options);
            return taskSource.Task;
        }

        public Task<IReadOnlyList<Future>> GetFuturesAsync(string instrumentSymbol)
        {
            var cached = GetFutures(instrumentSymbol);
            if (cached.Any()) {
                return Task.FromResult(cached);
            }

            var taskSource = new TaskCompletionSource<IReadOnlyList<Future>>();
            RequestEnded += _ =>
            {
                var newInstruments = GetFutures(instrumentSymbol);
                taskSource.TrySetResult(newInstruments);
            };

            var token = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            token.Token.Register(() => taskSource.TrySetCanceled());
            RequestInstrument(instrumentSymbol, InstrumentTypes.Futures);
            return taskSource.Task;
        }

        public IEnumerable<string> GetInstrumentCodes()
        {
            return new List<string> {"GE", "TN", "ES", "KE", "ZN", "ZC", "NG", "GC", "EUR", "GBP", "JPY", "ZF"};
        }

        private IReadOnlyList<Future> GetFutures(string instrumentSymbol)
        {
            IReadOnlyList<Future> cache = _cacheInstruments.Where(i =>
                                                               i.InstrumentType == InstrumentTypes.Futures &&
                                                               i.Symbol == instrumentSymbol)
                                                           .Select(i => i as Future).ToList();
            return cache;
        }

        public void UpdateConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private event Action<int> RequestEnded;

        private void OnConnectionStateChanged(ConnectionStates states)
        {
            ConnectionState = states;
            var action = ConnectionStateChanged;
            action?.Invoke(states);
        }

        private void OnGatewayStateChanged(ConnectionStates states)
        {
            GatewayState = states;
            var action = GatewayStateChanged;
            action?.Invoke(states);
        }

        #region contract info

        /// The security's type: STK - stock (or ETF) OPT - option FUT - future IND - index FOP - futures option
        /// Ответ на запрос см.
        /// <see cref="contractDetails" />
        public void RequestInstrument(string instrumentCode, InstrumentTypes type)
        {
            var secType = type switch
            {
                InstrumentTypes.Futures => "FUT",
                InstrumentTypes.Options => "FOP",
                _ => string.Empty
            };
            var contract = new Contract {Symbol = instrumentCode, SecType = secType};

            ClientSocket.reqContractDetails(_nextRequestId++, contract);
        }

        /// <summary>
        ///     Oтвет на запрос <see cref="RequestInstrument" />
        /// </summary>
        /// <param name="reqId">id запроса <see cref="RequestInstrument" /></param>
        /// <param name="contractDetails">Полное описание инструмента</param>
        public override void contractDetails(int reqId, ContractDetails contractDetails)
        {
            var contract = contractDetails.Contract;

            Instrument instrument = contract.SecType switch
            {
                "FUT" => new Future(contract, contractDetails.LongName) {PriceStep = (decimal) contractDetails.MinTick},
                "FOP" => new Option(contract) {PriceStep = (decimal) contractDetails.MinTick},
                _ => null
            };
            _cacheInstruments.Add(instrument);
        }

        public override void contractDetailsEnd(int reqId)
        {
            var action = RequestEnded;
            action?.Invoke(reqId);
        }

        #endregion

        #region subscrube instrument

        public override void tickPrice(int tickerId, int field, double price, TickAttrib attribs)
        {
            if (price == -1) {
                return;
            }

            var instrument = _codeHandler.ConvertToInstrumentDTO(tickerId, field, price);
            var action = FutureChanged;
            action?.Invoke(instrument);
        }

        public override void tickOptionComputation(int tickerId, int field, double impliedVolatility, double delta,
            double optPrice,
            double pvDividend, double gamma, double vega, double theta, double undPrice)
        {
            var option = _codeHandler.ConvertToInstrumentDTO(tickerId, field, optPrice);
            var action = OptionChanged;
            action?.Invoke(option);
        }

        /// <summary>
        ///     Подписка на инструмент. используется вместе с <see cref="RequestMarketDataType" />.
        /// </summary>
        /// <param name="instrument">
        ///     Инструмент на который необходимо подписаться.
        ///     (ВАЖНО! В целях избежания путаницы с инструментами использовать FullName (или LocalSymbol у Contract))
        /// </param>
        public void SubscribeInstrument(Instrument instrument)
        {
            try {
                if (instrument == null || string.IsNullOrEmpty(instrument.FullName)) {
                    return;
                }

                var contract = instrument.CreateNewIbContract();

                ClientSocket.reqMktData(contract.ConId, contract, "", false, false, null);
            }
            catch (Exception e) {
                _logger.AddLog(e.Message, 2);
            }
        }

        /// <summary>
        ///     Отписка от инструмента.
        /// </summary>
        /// <param name="id">id инструмента, по которому необходимо отменить подписку.</param>
        public void DescribeInstrument(int id)
        {
            if (id == 0) {
                return;
            }

            ClientSocket.cancelMktData(id);
        }

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
        public void RequestMarketDataType(int type)
        {
            ClientSocket.reqMarketDataType(type);
        }

        #endregion

        #region Orders

        private int _nextOrderId;

        public override void nextValidId(int orderId)
        {
            OnConnectionStateChanged(ConnectionStates.Connected);
            _nextOrderId = orderId;
        }

        private readonly List<Order> _orders = new List<Order>();

        public void SendOrder<T>(Guid strategyId, T instrument, Directions direction, int volume,
            decimal price = 0M, string description = "")
            where T : Instrument
        {
            var order = new Order(_nextOrderId)
            {
                StrategyId = strategyId,
                Direction = direction,
                Volume = Math.Abs(volume),
                Price = price,
                Instrument = instrument,
                Description = description,
                SendTime = DateTime.Now,
                OrderType = price == 0 ? OrderTypes.Market : OrderTypes.Limit
            };

            _orders.Add(order);
            var newOrder = order.ToIbOrder();
            var contract = instrument.CreateNewIbContract();
            ClientSocket.placeOrder(_nextOrderId++, contract, newOrder);
        }

        public override void orderStatus(int orderId, string status, double filled,
            double remaining, double avgFillPrice, int permId,
            int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice)
        {
            if (!_orders.Exists(ord => ord.Id == orderId)) {
                return;
            }

            var orderState = OrderHelper.SetOrderState(status);
            var order = _orders
                        .Where(o => o.Id == orderId)
                        .Select(o => new Order(o.Id)
                        {
                            StrategyId = o.StrategyId,
                            Description = o.Description,
                            Direction = o.Direction,
                            Price = o.Price,
                            Instrument = o.Instrument,
                            OrderType = o.OrderType,
                            Volume = o.Volume,
                            SendTime = o.SendTime,
                            ExecutionPrice = (decimal) lastFillPrice,
                            OrderState = orderState,
                            FilledVolume = (int) filled
                        }).Single();

            var action = OrderChanged;
            action?.Invoke(order);
        }

        /// <inheritdoc />
        /// <remarks>https://interactivebrokers.github.io/tws-api/cancel_order.html</remarks>
        public void CancelAllOrders(Guid id)
        {
            var listOrders = _orders.Where(o => o.StrategyId == id).ToList();
            for (var i = 0; i < listOrders.Count(); i++) {
                ClientSocket.cancelOrder(listOrders[i].Id);
            }
        }

        /// <inheritdoc />
        public void CancelOrder(int id)
        {
            ClientSocket.cancelOrder(id);
        }

        #endregion

        #region Account

        private readonly List<Account> _accounts = new List<Account>();

        public IEnumerable<Account> GetAccounts()
        {
            return _accounts;
        }

        public override void managedAccounts(string accountsList)
        {
            if (string.IsNullOrEmpty(accountsList)) {
                return;
            }

            var accounts = new List<string>(accountsList.Split(','));
            foreach (var account in accounts) {
                _accounts.Add(new Account(account));
            }
        }

        #endregion

        #region Error

        public override void error(int id, int errorCode, string message)
        {
            switch (errorCode) {
                case IbCodes.CORRECT_DATA_CONNECTION:
                case IbCodes.RESTORE_WITH_DATA_CONNECTION:
                    OnGatewayStateChanged(ConnectionStates.Connected);
                    break;
                case IbCodes.LOST_CONNECTION:
                case IbCodes.LOST_MARKET_CONNECTION:
                    OnGatewayStateChanged(ConnectionStates.LossConnect);
                    break;
                case IbCodes.RESTORE_WITHOUT_DATA_CONNECTION:
                    OnGatewayStateChanged(ConnectionStates.Connected);
                    _notification.SendMessageAsync(
                        "Внимание! Восстановление данных произошло с потерей данных. Перезапустите систему!");
                    break;
                case IbCodes.RESET_SOCKET:
                    _notification.SendMessageAsync("Внимание! Произошел сброс сокета TWS. Перезапустите систему!");
                    break;
            }

            #if DEBUG
            Debug.WriteLine($"code: {errorCode.ToString()}, message: {message}");
            #endif
        }

        public override void error(string message)
        {
            if (message.Contains("Чтение после конца потока невозможно")) {
                OnConnectionStateChanged(ConnectionStates.LossConnect);
                OnGatewayStateChanged(ConnectionStates.LossConnect);
            }
            #if DEBUG
            Debug.WriteLine($"message: {message}");
            #endif
            _logger.AddLog($"Ib requestId: error: {message}", 2);
        }

        public override void error(Exception e)
        {
            if (e is SocketException) {
                OnConnectionStateChanged(ConnectionStates.LossConnect);
                OnGatewayStateChanged(ConnectionStates.LossConnect);
            }
            #if DEBUG
            Debug.WriteLine($"message: {e.Message}");
            #endif
            _logger.AddLog($"Ib requestId: error: {e.Message}", 2);
        }

        #endregion
    }
}