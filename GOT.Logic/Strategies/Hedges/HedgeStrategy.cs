using System;
using System.Linq;
using System.Windows;
using GOT.Logic.DTO;
using GOT.Logic.Enums;
using GOT.Logic.Models;
using GOT.Logic.Models.Instruments;
using GOT.Logic.Strategies.Bases;
using GOT.SharedKernel.Utils.Exceptions;
using Newtonsoft.Json;

namespace GOT.Logic.Strategies.Hedges
{
    [JsonObject(MemberSerialization.OptIn)]
    public class HedgeStrategy : BaseStrategy<Future>
    {
        [JsonConstructor]
        public HedgeStrategy(string name)
            : base(name)
        {
        }

        public override void Start()
        {
            try {
                if (!CanStartStrategy()) {
                    throw new StrategyException($"User is cancel start strategy:{Name}");
                }

                SetStartLogic();
                Connector.FutureChanged += OnInstrumentChanged;
                Connector.OrderChanged += OnOrderChanged;
                base.Start();
            }
            catch (StrategyException e) {
                Stop();
                SendInfoNotification(e.Message);
            }
        }

        private bool CanStartStrategy()
        {
            var startedParameter = IsNullStartedParameters();
            if (startedParameter != string.Empty) {
                var message = $"{startedParameter} \nВы уверены, что хотите запустить стратегию?";
                var mbr = MessageBox.Show(message, "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (mbr == MessageBoxResult.No) {
                    return false;
                }
            }

            return true;
        }

        private string IsNullStartedParameters()
        {
            if (Volume <= 0) {
                return "Объем меньше или равен 0!";
            }

            if (ActivatePrice <= 0) {
                return "Цена активации меньше или равна 0!";
            }

            if (AntiBreakEvenOffset <= 0) {
                return "Безубыток антистопа меньше или равен 0!";
            }

            if (StopRestartOffset <= 0) {
                return "Рестарт стопа меньше или равен 0!";
            }

            if (AntiStopPrice <= 0) {
                return "Сигнальная цена антистопа меньше или равен 0!";
            }

            if (AntiOffset <= 0) {
                return "Смещение антистопа меньше или равно 0!";
            }

            return string.Empty;
        }

        private void SetStartLogic()
        {
            Container.HasActiveStrategy = false;
            if (Position != 0 && !IsRubiconStrategy) {
                LogicType = StopLogicType.AntiStop;
                SetWithoutLossPrice();
            } else {
                LogicType = StopLogicType.Stop;
            }
        }

        public override void Stop()
        {
            CancelOrders();

            Connector.FutureChanged -= OnInstrumentChanged;
            Connector.OrderChanged -= OnOrderChanged;
            base.Stop();
        }

        public override bool Reset()
        {
            ExecutionPrice = 0;
            FilledVolume = 0;
            WithoutLossPrice = 0;
            Pnl = 0;
            LogicType = StopLogicType.Stop;
            Orders.Clear();
            return true;
        }

        public override void ClosePositions()
        {
            StrategyState = StrategyStates.ClosePositions;
            CancelOrders();

            if (Position == 0) {
                return;
            }

            var newDirection = Direction != Directions.Buy ? Directions.Buy : Directions.Sell;
            var volume = Math.Abs(Position);
            var description = $"Close Position | level: {Level}. {Id.ToString()}";

            SendOrder(Instrument, Account, newDirection, volume, description: description);
        }

        public bool CanStartRubicon(int volume)
        {
            if (volume == 0 || HedgeGrade != HedgeGrades.Border || IsRubiconStrategy) {
                return false;
            }

            var isCanStart = LogicType switch
            {
                StopLogicType.AntiStop when !Container.HasActiveStrategy => true,
                StopLogicType.RestartStop => true,
                _ => false
            };

            return isCanStart;
        }

        public bool CanStopRubicon()
        {
            if (HedgeGrade != HedgeGrades.Border || IsRubiconStrategy) {
                return false;
            }

            return LogicType == StopLogicType.Stop && !Container.HasActiveStrategy;
        }

        protected override void OnInstrumentChanged(InstrumentDTO instrument)
        {
            if (Instrument.Id != instrument.Id) {
                return;
            }

            if (instrument.LastPrice == 0 || Instrument.LastPrice == instrument.LastPrice) {
                return;
            }

            Instrument.LastPrice = instrument.LastPrice;
            SetPnl(Instrument.LastPrice);

            if (StrategyState != StrategyStates.Started) {
                return;
            }

            var volume = Container.GetTradableVolume(Direction, ActivatePrice);

            Work(volume);

            if (CanStartRubicon(volume)) {
                Container.StartRubicon(Direction, volume);
            }

            if (CanStopRubicon()) {
                Container.StopRubicon(Direction);
            }
        }

        private void UpdateAntiStopPrice()
        {
            if (AntiOffset <= 0) {
                return;
            }

            if (Direction == Directions.Sell) {
                AntiStopPrice = ActivatePrice + AntiOffset;
            } else {
                AntiStopPrice = ActivatePrice - AntiOffset;
            }
        }

        /// <summary>
        ///     Устанавливает цену безубытка антистопа.
        /// </summary>
        private void SetWithoutLossPrice()
        {
            if (AntiBreakEvenOffset <= 0) {
                return;
            }

            if (Direction == Directions.Buy) {
                WithoutLossPrice = ActivatePrice + AntiBreakEvenOffset;
            } else {
                WithoutLossPrice = ActivatePrice - AntiBreakEvenOffset;
            }
        }

        public void Work(int volume)
        {
            if (!Container.HasActiveStrategy) {
                var price = CalculateNewPrice();

                switch (LogicType) {
                    case StopLogicType.Stop:
                        SendStopOrder(price, volume);
                        break;
                    case StopLogicType.AntiStop:
                        SendAntiStopOrder(price);
                        break;
                }
            }

            if (LogicType == StopLogicType.RestartStop) {
                TryToStopLogicType();
            }
        }

        private decimal CalculateNewPrice()
        {
            decimal price = 0;

            var isSellStop = Direction == Directions.Sell && LogicType == StopLogicType.Stop;
            var isBuyAntiStop = Direction == Directions.Buy && LogicType == StopLogicType.AntiStop;
            if (isSellStop || isBuyAntiStop) {
                price = Instrument.LastPrice - Instrument.PriceStep * ShiftStepPrice;
            }

            var isSellNotStop = Direction == Directions.Sell && LogicType != StopLogicType.Stop;
            var isBuyNotAntiStop = Direction == Directions.Buy && LogicType != StopLogicType.AntiStop;
            if (isSellNotStop || isBuyNotAntiStop) {
                price = Instrument.LastPrice + Instrument.PriceStep * ShiftStepPrice;
            }

            return price;
        }

        protected override void OnOrderChanged(Order ord)
        {
            if (Id != ord.StrategyId) {
                return;
            }

            if (!ord.Description.Contains($"{Id.ToString()}")) {
                return;
            }

            switch (ord.OrderState) {
                case OrderState.Active:
                    CheckOrderToContains(ord);
                    break;
                case OrderState.Failed:
                case OrderState.Cancelled:
                    CheckOrderToContains(ord);
                    Container.HasActiveStrategy = false;
                    SendInfoNotification($"Order was cancelled or failed: {ord.Description}");
                    break;
                case OrderState.Filled:
                    if (Orders.Last().OrderState == OrderState.Filled) {
                        break;
                    }

                    ReplaceOrder(ord);
                    ExecutionPrice = ord.ExecutionPrice;
                    Container.HasActiveStrategy = false;
                    SendInfoNotification($"Order was filled: {ord.Description}");
                    if (IsRubiconStrategy) {
                        Stop();
                        break;
                    }

                    ChangeLogicType();
                    break;
            }

            OnPropertyChanged("Position");
        }

        private void ChangeLogicType()
        {
            if (LogicType == StopLogicType.Stop) {
                UpdateAntiStopPrice();
                SetWithoutLossPrice();
                LogicType = StopLogicType.AntiStop;
            } else {
                LogicType = StopLogicType.RestartStop;
            }
        }

        public override string ToString()
        {
            return $"{Level} | {LogicType} | {ActivatePrice.ToString()}. Name: {Name}.";
        }

        #region Properties

        public HedgeContainer Container { get; set; }

        private decimal _withoutLossPrice;

        /// <summary>
        ///     Цена перехода в безубыток
        /// </summary>
        public decimal WithoutLossPrice
        {
            get => _withoutLossPrice;
            set
            {
                _withoutLossPrice = value;
                OnPropertyChanged();
            }
        }

        private HedgeGrades? _hedgeGrade;

        /// <summary>
        ///     Градация уровней в сетке.
        /// </summary>
        [JsonProperty("hedgeGrade")]
        public HedgeGrades? HedgeGrade
        {
            get => _hedgeGrade;
            set
            {
                if (value == HedgeGrades.None) {
                    _hedgeGrade = null;
                    return;
                }

                _hedgeGrade = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("isRubiconStrategy")]
        public bool IsRubiconStrategy { get; set; }

        private decimal _activatePrice;

        [JsonProperty("activatePrice")]
        public decimal ActivatePrice
        {
            get => _activatePrice;
            set
            {
                if (_activatePrice == value) {
                    return;
                }

                _activatePrice = value;
                AntiStopPrice = Direction == Directions.Sell
                    ? ActivatePrice + AntiOffset
                    : ActivatePrice - AntiOffset;
                OnPropertyChanged();
            }
        }

        private decimal _antiOffset;

        /// <summary>
        ///     Смещение антистопа
        /// </summary>
        [JsonProperty("antiOffset")]
        public decimal AntiOffset
        {
            get => _antiOffset;
            set
            {
                if (_antiOffset == value) {
                    return;
                }

                _antiOffset = value;
                AntiStopPrice = Direction == Directions.Sell
                    ? ActivatePrice + AntiOffset
                    : ActivatePrice - AntiOffset;
                OnPropertyChanged();
            }
        }

        private decimal _antiStopPrice;

        /// <summary>
        ///     Цена активации антистопа
        /// </summary>
        [JsonProperty("antiStopPrice")]
        public decimal AntiStopPrice
        {
            get => _antiStopPrice;
            set
            {
                if (_antiStopPrice == value) {
                    return;
                }

                _antiStopPrice = value;
                OnPropertyChanged();
            }
        }

        private decimal _stopRestartOffset;

        /// <summary>
        ///     Смещение рестарта стопа
        /// </summary>
        [JsonProperty("stopRestartOffset")]
        public decimal StopRestartOffset
        {
            get => _stopRestartOffset;
            set
            {
                if (_stopRestartOffset == value) {
                    return;
                }

                _stopRestartOffset = value;
                OnPropertyChanged();
            }
        }

        private decimal _antiBreakEvenOffset;

        /// <summary>
        ///     Безубыток Антистопа
        /// </summary>
        [JsonProperty("antiBreakEvenOffset")]
        public decimal AntiBreakEvenOffset
        {
            get => _antiBreakEvenOffset;
            set
            {
                if (_antiBreakEvenOffset == value) {
                    return;
                }

                _antiBreakEvenOffset = value;
                OnPropertyChanged();
            }
        }

        private string _level;

        [JsonProperty("level")]
        public string Level
        {
            get => _level;
            set
            {
                if (_level == value) {
                    return;
                }

                _level = value;
                OnPropertyChanged();
            }
        }

        private StopLogicType _logicType;

        [JsonProperty("logicType")]
        public StopLogicType LogicType
        {
            get => _logicType;
            set
            {
                if (_logicType == value) {
                    return;
                }

                _logicType = value;
                OnPropertyChanged();
            }
        }

        private decimal _executionPrice;

        /// <summary>
        ///     Цена исполнения сделки
        /// </summary>
        [JsonProperty("executionPrice")]
        public decimal ExecutionPrice
        {
            get => _executionPrice;
            set
            {
                if (_executionPrice == value) {
                    return;
                }

                _executionPrice = value;
                OnPropertyChanged();
            }
        }

        private int _shiftStepPrice;

        /// <summary>
        ///     Сдвиг шага цены
        /// </summary>
        [JsonProperty("shiftStepPrice")]
        public int ShiftStepPrice
        {
            get => _shiftStepPrice;
            set
            {
                if (_shiftStepPrice == value) {
                    return;
                }

                _shiftStepPrice = value;
                OnPropertyChanged();
            }
        }

        private decimal _filledVolume;

        public decimal FilledVolume
        {
            get => _filledVolume;
            set
            {
                if (_filledVolume == value) {
                    return;
                }

                _filledVolume = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region trade logic

        private void SendStopOrder(decimal price, int tradedVolume)
        {
            var volume = IsRubiconStrategy ? Volume : tradedVolume;
            var lastPrice = Instrument.LastPrice;

            var description =
                $"STOP | Level:{Level}. last price: {lastPrice.ToString()}. volume:{volume.ToString()}. strategyId:{Id.ToString()}";

            switch (Direction) {
                case Directions.Sell:
                {
                    if (lastPrice <= ActivatePrice && volume < 0) {
                        SendOrder(Instrument, Account, Directions.Sell, Math.Abs(volume), price, description);
                    }

                    break;
                }
                case Directions.Buy:
                {
                    if (lastPrice >= ActivatePrice && volume > 0) {
                        SendOrder(Instrument, Account, Directions.Buy, volume, price, description);
                    }

                    break;
                }
            }
        }

        private void SendAntiStopOrder(decimal price)
        {
            var volume = -Position;
            var lastPrice = Instrument.LastPrice;
            var description =
                $"ANTI-STOP | Level:{Level}. last price: {lastPrice.ToString()}. volume:{volume.ToString()}. strategyId:{Id.ToString()}";

            switch (Direction) {
                case Directions.Sell:
                {
                    if (WithoutLossPrice != 0 && lastPrice <= WithoutLossPrice) {
                        AntiStopPrice = ActivatePrice;
                        WithoutLossPrice = 0;
                    }

                    if (volume > 0 && lastPrice >= AntiStopPrice) {
                        SendOrder(Instrument, Account, Directions.Buy, volume, price, description);
                    }

                    break;
                }
                case Directions.Buy:
                {
                    if (WithoutLossPrice != 0 && lastPrice >= WithoutLossPrice) {
                        AntiStopPrice = ActivatePrice;
                        WithoutLossPrice = 0;
                    }

                    if (volume < 0 && lastPrice <= AntiStopPrice) {
                        SendOrder(Instrument, Account, Directions.Sell, Math.Abs(volume), price, description);
                    }

                    break;
                }
            }
        }

        private void TryToStopLogicType()
        {
            if (Direction == Directions.Sell && Instrument.LastPrice > ActivatePrice + StopRestartOffset ||
                Direction == Directions.Buy && Instrument.LastPrice < ActivatePrice - StopRestartOffset) {
                LogicType = StopLogicType.Stop;
            }
        }

        protected override void SendOrder(Instrument instrument, string account, Directions direction, int volume,
            decimal price = decimal.Zero,
            string description = "")
        {
            Container.HasActiveStrategy = true;
            base.SendOrder(instrument, account, direction, volume, price, description);
        }

        #endregion
    }
}