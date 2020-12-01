using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using CQG;
using GOT.Logic.DTO;
using GOT.Logic.Enums;
using GOT.Logic.Models;
using GOT.Logic.Models.Instruments;
using GOT.Logic.Utils.Helpers;
using GOT.SharedKernel;
using GOT.SharedKernel.Enums;

namespace GOT.Logic.Connectors.Cqg
{
    [Obsolete("данный коннектор CQG является устаревшим. " +
              "В нём описаны методы для работы с их API. " +
              "Поэтому используйте его только при возобновлении поддержки данного коннектора.")]
    public class CqgConnector
    {
        private readonly List<Account> _accounts = new List<Account>();
        private readonly List<string> _fullNameCodes = new List<string>();
        private readonly List<string> _instrumentCodes = new List<string>();
        private readonly List<Instrument> _instruments = new List<Instrument>();

        private readonly IGotLogger _logger;

        // private readonly List<CqgOrder> _orders = new List<CqgOrder>(); //to do
        private readonly CQGCEL _provider;
        private CQGAccount _account;

        public CqgConnector(IGotLogger logger)
        {
            try {
                _logger = logger;
                _provider = new CQGCEL();
                //instrument properties
                _provider.InstrumentResolved += CqgOnInstrumentResolved;
                _provider.TradableCommoditiesResolved += OnTradableCommoditiesResolved;
                _provider.CommodityInstrumentsResolved += OnCommodityInstrumentsResolved;
                _provider.InstrumentChanged += CqgOnInstrumentChanged;
                _provider.OrderChanged += OnCqgOrderChanged;
                // events
                _provider.DataError += CqgOnDataError;
                _provider.DataConnectionStatusChanged += CqgOnDataConnectionStatusChanged;
                _provider.GWConnectionStatusChanged += CqgOnGatewayConnectionStatusChanged;
                _provider.AccountChanged += CqgOnAccountChanged;
                // Configure CQG API. Based on this configuration CQG API works differently.
                _provider.APIConfiguration.CollectionsThrowException = false;
                _provider.APIConfiguration.DefPositionSubscriptionLevel =
                    ePositionSubscriptionLevel.pslSnapshotAndUpdates;
                _provider.APIConfiguration.FireEventOnChangedPrices = true;
                _provider.APIConfiguration.UseOrderSide = true;
                _provider.APIConfiguration.IncludeOrderTransactions = true;
                _provider.APIConfiguration.PositionDetailing = ePositionDetailing.pdAllTrades;
                _provider.APIConfiguration.DefaultInstrumentSubscriptionLevel = eDataSubscriptionLevel.dsQuotesAndBBA;
                _provider.APIConfiguration.ReadyStatusCheck = eReadyStatusCheck.rscOff;
                _provider.APIConfiguration.TimeZoneCode = eTimeZone.tzCentral;
                _provider.APIConfiguration.AccountMarginAndPositionsThrottleInterval = 300;
            }
            catch (COMException e) {
                _logger.AddLog("Error occurred during CEL initialization. " + e.Message, 3);
            }
        }

        private CQGInstrument GetCurrentInstrument(string fullName)
        {
            return fullName == null ? null : _provider.Instruments[fullName];
        }

        private static eInstrumentType GetInstrumentType(InstrumentTypes type)
        {
            switch (type) {
                case InstrumentTypes.Futures:
                    return eInstrumentType.itFuture;
                case InstrumentTypes.OptionCall:
                    return eInstrumentType.itOptionCall;
                case InstrumentTypes.OptionPut:
                    return eInstrumentType.itOptionPut;
                case InstrumentTypes.Options:
                    return eInstrumentType.itAllOptions;
                default:
                    return eInstrumentType.itUndefined;
            }
        }

        private void CqgOnAccountChanged(eAccountChangeType changeType, CQGAccount cqgAccount,
            CQGPosition cqgPosition)
        {
            try {
                switch (changeType) {
                    case eAccountChangeType.actAccountsReloaded:
                        _accounts.Clear();

                        foreach (CQGAccount acc in _provider.Accounts) {
                            _account = acc;
                            _accounts.Add(new Account(_account.GWAccountName));
                            _account.AutoSubscribeInstruments = true;
                            _provider.RequestManualFills(acc.GWAccountID, eManualFillsDataLevel.mfdlSnapshotAndUpdates);
                            _provider.RequestTradableCommodities(acc.GWAccountID);
                        }

                        break;
                    case eAccountChangeType.actAccountChanged:
                        _account = cqgAccount;
                        break;
                }
            }
            catch (Exception e) {
                _logger.AddLog($"On Account changed: {e.Message}", 3);
            }
        }

        /// <summary>
        ///     Добавление кода инструмента (первая колонка)
        /// </summary>
        private void OnTradableCommoditiesResolved(int gwAccountId, CQGCommodities cqgCommodities,
            CQGError cqgError)
        {
            if (cqgError == null) {
                for (var i = 0; i <= cqgCommodities.Count - 1; i++) {
                    if (!_instrumentCodes.Contains(cqgCommodities[i])) {
                        _instrumentCodes.Add(cqgCommodities[i]);
                    }
                }
            } else {
                MessageBox.Show("Error " + cqgError.Code + ": " + cqgError.Description, "Historical Data",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CqgOnDataConnectionStatusChanged(eConnectionStatus newStatus)
        {
            switch (newStatus) {
                case eConnectionStatus.csConnectionUp:
                    ConnectionState = ConnectionStates.Connected;
                    _provider.AccountSubscriptionLevel = eAccountSubscriptionLevel.aslAccountUpdatesAndOrders;
                    break;
                case eConnectionStatus.csConnectionDown:
                    ConnectionState = ConnectionStates.Disconnected;
                    break;
                case eConnectionStatus.csConnectionDelayed:
                    ConnectionState = ConnectionStates.Delay;
                    break;
                case eConnectionStatus.csConnectionTrouble:
                    ConnectionState = ConnectionStates.LossConnect;
                    break;
                default:
                    ConnectionState = ConnectionStates.Disconnected;
                    break;
            }

            OnConnectionStateChanged(ConnectionState);
        }

        private void CqgOnGatewayConnectionStatusChanged(eConnectionStatus newStatus)
        {
            switch (newStatus) {
                case eConnectionStatus.csConnectionUp:
                    GatewayState = ConnectionStates.Connected;
                    _provider.AccountSubscriptionLevel = eAccountSubscriptionLevel.aslAccountUpdatesAndOrders;
                    break;
                case eConnectionStatus.csConnectionDown:
                    GatewayState = ConnectionStates.Disconnected;
                    break;
                case eConnectionStatus.csConnectionDelayed:
                    GatewayState = ConnectionStates.Delay;
                    break;
                case eConnectionStatus.csConnectionTrouble:
                    GatewayState = ConnectionStates.LossConnect;
                    break;
                default:
                    GatewayState = ConnectionStates.Disconnected;
                    break;
            }

            OnGatewayStateChanged(GatewayState);
        }

        private void CqgOnDataError(object cqgError, string errorDescription)
        {
            if (cqgError is CQGError cqgErr) {
                _logger.AddLog($"code: {cqgErr.Code}. description: {errorDescription}", 3);
            }
        }

        #region props

        public void Connect()
        {
            _provider.Startup();
        }

        public void Disconnect()
        {
            _provider.Shutdown();
        }

        public void RemoveInstrument(Instrument instrument)
        {
            if (instrument?.Id == null || _instruments.All(i => i.Id != instrument.Id)) {
                return;
            }

            var cqgInstrument = GetCurrentInstrument(instrument.FullName);
            _provider.RemoveInstrument(cqgInstrument);
            _instruments.Remove(instrument);
        }

        public IReadOnlyList<Option> GetOptions(string baseInstrumentCode, string exchange = "")
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Option>> GetOptionsAsync(string baseInstrumentCode, string exchange = "")
        {
            IReadOnlyList<Option> options = _instruments.Where(instr =>
                                                            instr.InstrumentType != InstrumentTypes.Futures &&
                                                            instr.Code == baseInstrumentCode)
                                                        .Select(i => i as Option)
                                                        .ToList();
            return Task.FromResult(options);
        }

        public event Action<IEnumerable<string>> InstrumentNameChanged;

        private void OnInstrumentNameChanged(IEnumerable<string> names)
        {
            InstrumentNameChanged?.Invoke(names);
        }

        public IEnumerable<string> GetInstrumentCodes()
        {
            return _instrumentCodes;
        }

        public IEnumerable<Account> GetAccounts()
        {
            return _accounts;
        }

        public void RequestMarketDataType(int type)
        {
            throw new NotImplementedException();
        }

        public event Action<ConnectionStates> ConnectionStateChanged;

        private void OnConnectionStateChanged(ConnectionStates states)
        {
            ConnectionStateChanged?.Invoke(states);
        }

        public event Action<ConnectionStates> GatewayStateChanged;

        private void OnGatewayStateChanged(ConnectionStates states)
        {
            GatewayStateChanged?.Invoke(states);
        }

        public event Action<Instrument> InstrumentResolved;

        private void OnInstrumentResolved(Instrument instrument)
        {
            if (!_instruments.Contains(instrument)) {
                _instruments.Add(instrument);
            }

            InstrumentResolved?.Invoke(instrument);
        }

        public event Action<InstrumentDTO> OptionChanged;

        private void OnOptionChanged(InstrumentDTO instrument)
        {
            OptionChanged?.Invoke(instrument);
        }

        public event Action<InstrumentDTO> FutureChanged;

        private void OnInstrumentChanged(InstrumentDTO instrument)
        {
            FutureChanged?.Invoke(instrument);
        }

        public event Action<Order> OrderChanged;

        private void OnOrderChanged(Order order)
        {
            OrderChanged?.Invoke(order);
        }

        public ConnectorTypes ConnectorType { get; } = ConnectorTypes.CQG;

        public ConnectionStates ConnectionState { get; set; }

        public ConnectionStates GatewayState { get; private set; }

        #endregion

        #region Orders

        public void CancelAllOrders(Guid strategyId)
        {
            try {
                //if (order is CqgOrder cqgOrder) { //to do
                // var cqg = _provider.Orders[cqgOrder.CqgOrderId]; //to do
                // cqg?.Cancel(); //to do
                //} //to do
            }
            catch (Exception e) {
                _logger.AddLog(e.Message, 3);
            }
        }

        public void CancelOrder(int id)
        {
            try {
                //if (order is CqgOrder cqgOrder) { //to do
                // var cqg = _provider.Orders[cqgOrder.CqgOrderId]; //to do
                // cqg?.Cancel(); //to do
                //} //to do
            }
            catch (Exception e) {
                _logger.AddLog(e.Message, 3);
            }
        }

        public void DescribeInstrument(int id)
        {
            throw new NotImplementedException();
        }

        public void SendOrder<T>(Guid strategyId, T instrument, Directions direction, int volume,
            decimal price = 0M,
            string description = "")
            where T : Instrument
        {
            var order = new Order();

            var orderType = order.ToCqgOrderType();
            var cqgInstrument = GetCurrentInstrument(instrument.FullName);

            if (cqgInstrument == null || order.Volume == 0) {
                return;
            }

            var cqgAccount = _account;
            var side = direction == Directions.Buy ? eOrderSide.osdBuy : eOrderSide.osdSell;

            CQGOrder newOrder;
            switch (orderType) {
                case eOrderType.otMarket:
                    newOrder = _provider.CreateOrder(orderType, cqgInstrument, cqgAccount, order.Volume, side);
                    break;
                case eOrderType.otLimit:
                    newOrder = _provider.CreateOrder(orderType, cqgInstrument, cqgAccount, order.Volume, side,
                        (double) order.Price);
                    break;
                case eOrderType.otStop:
                    newOrder = _provider.CreateOrder(orderType, cqgInstrument, cqgAccount, order.Volume, side,
                        stop_price: (double) order.Price);
                    break;
                default:
                    newOrder = null;
                    break;
            }

            newOrder.Properties[eOrderProperty.opDurationType].Value = eOrderDuration.odDay;
            newOrder.Properties[eOrderProperty.opOpenCloseInstruction].Value = 0;
            newOrder.Properties[eOrderProperty.opDescription].Value = order.Description;
            newOrder.Place();
        }

        private void OnCqgOrderChanged(eChangeType changeType, CQGOrder cqgOrder, CQGOrderProperties oldProperties,
            CQGFill cqgFill, CQGError cqgError)
        {
            // switch (changeType) { //to do
            // case eChangeType.ctAdded: //to do
            // if (_orders.All(ord => ord.CqgOrderId != cqgOrder.OriginalOrderID)) { //to do
            // var order = new Order(cqgOrder); //to do
            // if (cqgError != null) { //to do
            //     order.ErrorInfo = cqgError.Description; //to do
            // } //to do 
            // //to do
            // _orders.Add(order); //to do
            // OnOrderChanged(order); //to do
            // } //to do

            // break; //to do
            // case eChangeType.ctChanged: //to do
            // var modifyOrder = _orders.First(o => o.CqgOrderId == cqgOrder.OriginalOrderID); //to do
            // modifyOrder.ModifyLimitOrder(cqgOrder); //to do
            // OnOrderChanged(modifyOrder); //to do
            // break; //to do
            // case eChangeType.ctRemoved: //to do
            // if (_orders.Any(ord => ord.CqgOrderId == cqgOrder.OriginalOrderID)) { //to do
            // var removeOrder = _orders.FirstOrDefault(o => o.CqgOrderId == cqgOrder.OriginalOrderID); //to do
            // _orders.Remove(removeOrder); //to do
            // } //to do

            // break; //to do
            // } //to do
        }

        #endregion

        #region Instruments

        public Task<IReadOnlyList<Future>> GetFuturesAsync(string instrumentSymbol)
        {
            IReadOnlyList<Future> futures = _instruments.Where(instr =>
                                                            instr.InstrumentType == InstrumentTypes.Futures &&
                                                            instr.Symbol == instrumentSymbol)
                                                        .Select(i => i as Future)
                                                        .ToList();
            return Task.FromResult(futures);
        }

        public void RequestInstrument(string instrumentCode, InstrumentTypes types)
        {
            try {
                var code = _instrumentCodes.SingleOrDefault(s => s.Equals(instrumentCode));
                var type = types == InstrumentTypes.AllInstrument
                    ? eInstrumentType.itAllInstruments
                    : GetInstrumentType(types);
                _provider.RequestCommodityInstruments(code, type);
            }
            catch (Exception e) {
                _logger.AddLog($"Request commodity: {e.Message}", 3);
            }
        }

        public void SubscribeInstrument(Instrument instrument)
        {
            try {
                var isRegistered = _instruments.Any(i => i.Symbol.Equals(instrument.Symbol));
                if (string.IsNullOrEmpty(instrument.Symbol) || isRegistered) {
                    return;
                }

                // Subscribe instrument in the collection
                _provider.NewInstrument(instrument.Symbol);
            }
            catch (Exception e) {
                _logger.AddLog($"Subscribe instrument: {e.Message}", 3);
            }
        }

        /// <summary>
        ///     Добавление списка имён (вторая колонка)
        /// </summary>
        private async void OnCommodityInstrumentsResolved(string commodityName, eInstrumentType instrumentType,
            CQGCommodityInstruments comInstruments)
        {
            if (instrumentType != eInstrumentType.itFuture) {
                await Task.Run(() =>
                {
                    foreach (string comInstrument in comInstruments) {
                        _provider.NewInstrument(comInstrument);
                    }
                });
                return;
            }

            _fullNameCodes.Clear();
            for (var i = comInstruments.Count - 1; i >= 0; i--) {
                if (!_fullNameCodes.Contains(comInstruments[i])) {
                    _fullNameCodes.Add(comInstruments[i]);
                }

                if (i == 0) {
                    OnInstrumentNameChanged(_fullNameCodes);
                }
            }
        }

        /// <summary>
        ///     Добавление зарегистрированных инструментов.
        /// </summary>
        /// <param name="symbol">Симовол регистрируемого инструмента</param>
        /// <param name="cqgInstrument">Зарегистрированный инструмент</param>
        private void CqgOnInstrumentResolved(string symbol, CQGInstrument cqgInstrument, CQGError cqgError)
        {
            if (_instruments.Any(i => i.Symbol.Contains(symbol))) {
                return;
            }

            Instrument instrument = null;
            switch (cqgInstrument.InstrumentType) {
                case eInstrumentType.itFuture:
                    instrument = new Future(cqgInstrument) {Symbol = symbol};
                    break;
                case eInstrumentType.itAllOptions:
                case eInstrumentType.itOptionCall:
                case eInstrumentType.itOptionPut:
                    instrument = new Option(cqgInstrument) {Symbol = symbol};
                    break;
            }

            OnInstrumentResolved(instrument);
        }

        private void CqgOnInstrumentChanged(CQGInstrument cqgInstrument, CQGQuotes cqgQuotes,
            CQGInstrumentProperties props)
        {
            var instr = _instruments.Where(inst => inst.FullName == cqgInstrument.FullName)
                                    .Select(i => new InstrumentDTO
                                    {
                                        Ask = (decimal) cqgInstrument.Ask.Price,
                                        Bid = (decimal) cqgInstrument.Bid.Price,
                                        LastPrice = (decimal) cqgInstrument.Trade.Price
                                    }).Single();

            OnInstrumentChanged(instr);
        }

        #endregion
    }
}