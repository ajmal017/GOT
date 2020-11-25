using System;
using System.Linq;
using GOT.Logic.DataTransferObjects;
using GOT.Logic.Enums;
using GOT.Logic.Models;
using GOT.Logic.Models.Instruments;
using GOT.Logic.Strategies.Bases;
using GOT.Logic.Utils;
using GOT.Logic.Utils.Helpers;
using GOT.SharedKernel.Utils.Exceptions;
using Newtonsoft.Json;

namespace GOT.Logic.Strategies.Options
{
    [JsonObject(MemberSerialization.OptIn)]
    public class OptionStrategy : BaseStrategy<Option>
    {
        private readonly PriceRange _priceRange = new PriceRange();
        private Directions _currentDirection;

        private bool _isBasis;
        private Order _lastOrder;

        private LifetimeOptions _lifetime;

        private Option _option;

        private OptionTypes? _optionType;

        private decimal _theoreticalPrice;

        private WorkingMode _workingMode;

        public OptionStrategy(string name)
            : base(name)
        {
            Lifetime = LifetimeOptions.Month;
        }

        public OptionStrategy()
        {
        }

        /// <summary>
        ///     Указывает, что опцион является основным в работе сетки.
        /// </summary>
        [JsonProperty("isBasis")]
        public bool IsBasis
        {
            get => _isBasis;
            set
            {
                if (_isBasis == value) {
                    return;
                }

                _isBasis = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Смещение цены от теоретической.
        /// </summary>
        [JsonProperty("priceOffset")]
        public decimal PriceOffset { get; set; }

        /// <summary>
        ///     Шаг цены. Минимальное значение, на которое цена может быть изменена.
        /// </summary>
        [JsonProperty("priceStep")]
        public decimal PriceStep { get; set; }

        [JsonProperty("instrument")]
        public override Option Instrument
        {
            get => _option;
            set
            {
                if (_option == value) {
                    return;
                }

                _option = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Режим работы: открытие позиции, закрытие позиции.
        /// </summary>
        [JsonProperty("workingMode")]
        public WorkingMode WorkingMode
        {
            get => _workingMode;
            set
            {
                if (_workingMode == value) {
                    return;
                }

                _workingMode = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Тип опциона
        /// </summary>
        [JsonProperty("optionType")]
        public OptionTypes? OptionType
        {
            get => _optionType;
            set
            {
                if (_optionType == value) {
                    return;
                }

                _optionType = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Период распада опциона.
        /// </summary>
        [JsonProperty("lifetime")]
        public LifetimeOptions Lifetime
        {
            get => _lifetime;
            set
            {
                if (_lifetime == value) {
                    return;
                }

                _lifetime = value;
                OnPropertyChanged();
            }
        }

        public decimal TheoreticalPrice
        {
            get => _theoreticalPrice;
            set
            {
                _theoreticalPrice = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Остаток объема, который нужно скотировать
        /// </summary>
        public int RemainVolume => Volume - Math.Abs(Position);

        public decimal AvgPrice => GetAvgPriceByFilledOrders();

        /// <summary>
        ///     Взять среднюю цену исполненных ордеров
        /// </summary>
        private decimal GetAvgPriceByFilledOrders()
        {
            var average = 0m;
            if (!FilledOrders.Any()) {
                return average;
            }

            average = FilledOrders.Average(order => order.ExecutionPrice);
            return MathHelper.RoundUp(average, PriceStep);
        }

        public override void Start()
        {
            try {
                _lastOrder = null;
                StrategyState = StrategyStates.Observe;
                SubscribeInstrument(Instrument);
                Connector.OptionChanged += OnInstrumentChanged;

                PrepareWorkingMode();

                Connector.OrderChanged += OnOrderChanged;
                base.Start();
            }
            catch (StrategyException e) {
                Logger.AddLog(e.Message, 2);
            }
        }

        /// <summary>
        ///     Подготавливает стратегию к работе, в зависимости от метода работы <see cref="WorkingMode" />
        /// </summary>
        private void PrepareWorkingMode()
        {
            switch (WorkingMode) {
                case WorkingMode.OpenPosition:
                    if (Volume == 0 || RemainVolume == 0) {
                        throw new StrategyException($"Strategy {Name} :Not enough volume or position");
                    }

                    _currentDirection = Direction;
                    break;
                case WorkingMode.ClosePosition:
                    if (Position == 0) {
                        throw new StrategyException($"Strategy {Name} :Not enough volume or position");
                    }

                    _currentDirection = Direction != Directions.Buy ? Directions.Buy : Directions.Sell;
                    Volume = Math.Abs(Position);
                    break;
            }
        }

        public override void Stop()
        {
            CancelOrders();
            Connector.OptionChanged -= OnInstrumentChanged;
            Connector.OrderChanged -= OnOrderChanged;
            base.Stop();
        }

        public override void ClosePositions()
        {
            if (_lastOrder != null) {
                Connector.CancelOrder(_lastOrder.Id);
            }

            WorkingMode = WorkingMode.ClosePosition;
            Start();
        }

        public override string ToString()
        {
            return $"Type: {OptionType}. Strike: {Instrument?.Strike.ToString()}. Name:{Name}";
        }

        private void InstrumentMap(InstrumentDTO dto)
        {
            if (dto.Ask != 0) {
                Instrument.Ask = dto.Ask;
            }

            if (dto.Bid != 0) {
                Instrument.Bid = dto.Bid;
            }

            if (dto.LastPrice != 0) {
                Instrument.LastPrice = dto.LastPrice;
            }
        }

        protected override void OnInstrumentChanged(InstrumentDTO inst)
        {
            if (Instrument.Id != inst.Id) {
                return;
            }

            InstrumentMap(inst);
            TheoreticalPrice = CalculateTheoreticalPrice();

            if (TheoreticalPrice == 0) {
                return;
            }

            SetPnl(TheoreticalPrice);

            if (StrategyState != StrategyStates.Started) {
                return;
            }

            _priceRange.UpdateRange(TheoreticalPrice, _currentDirection, PriceOffset, PriceStep);

            if (_lastOrder == null) {
                var price = _priceRange.CalculateNewPrice(TheoreticalPrice, _currentDirection, Instrument.Ask,
                    Instrument.Bid);
                SendOptionOrder(price);
            } else if (_lastOrder.OrderState == OrderState.Active && !_priceRange.InRange(_lastOrder.Price)) {
                CancelOrder(_lastOrder.Id);
            }
        }

        private decimal CalculateTheoreticalPrice()
        {
            if (Instrument.Ask == 0 || Instrument.Bid == 0) {
                return 0;
            }

            var theoreticalPrice = (Instrument.Ask + Instrument.Bid) / 2;
            return MathHelper.RoundUp(theoreticalPrice, PriceStep);
        }

        #region Basic logic

        protected override void OnOrderChanged(Order ord)
        {
            if (Id != ord.StrategyId || !ord.Description.Contains($"{Id.ToString()}")) {
                return;
            }

            switch (ord.OrderState) {
                case OrderState.Active:
                    CheckOrderToContains(ord);
                    _lastOrder = ord;
                    break;
                case OrderState.Failed:
                    CheckOrderToContains(ord);
                    _lastOrder = null;
                    SendInfoNotification($"Order is Failed: {ord.Description}");
                    break;
                case OrderState.Filled:
                    if (Orders.Last().OrderState == OrderState.Filled) {
                        break;
                    }

                    ReplaceOrder(ord);

                    var isOpeningMode = RemainVolume > 0 && WorkingMode == WorkingMode.OpenPosition;
                    var isClosingMode = Math.Abs(Position) > 0 && WorkingMode == WorkingMode.ClosePosition;

                    if (isOpeningMode || isClosingMode) {
                        var price = _priceRange.CalculateNewPrice(ord.ExecutionPrice, _currentDirection,
                            Instrument.Ask, Instrument.Bid);
                        SendOptionOrder(price);
                    } else {
                        _lastOrder = null;
                        SendInfoNotification($"Strategy {Name}, {Instrument.FullName}. is done!");
                        StrategyState = StrategyStates.Observe;
                        Connector.OrderChanged -= OnOrderChanged;
                    }

                    break;
                case OrderState.Cancelled:
                    ReplaceOrder(ord);
                    _lastOrder = null;
                    break;
            }

            OnPropertyChanged("AvgPrice");
            OnPropertyChanged("Position");
        }

        private void SendOptionOrder(decimal price = decimal.Zero)
        {
            if (price == 0) {
                return;
            }

            const int minVolume = 1;
            var desc = $"Option | Type: {OptionType} Strike: {Instrument.Strike.ToString()} id:{Id.ToString()}";
            _lastOrder = new Order();
            SendOrder(Instrument, _currentDirection, minVolume, price, desc);
        }

        #endregion
    }
}