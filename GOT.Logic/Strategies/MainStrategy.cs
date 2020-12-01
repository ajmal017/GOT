using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using GOT.Logic.Connectors;
using GOT.Logic.DTO;
using GOT.Logic.Enums;
using GOT.Logic.Models.Instruments;
using GOT.Logic.Strategies.Bases;
using GOT.Logic.Strategies.Hedges;
using GOT.Logic.Strategies.Options;
using GOT.Notification;
using GOT.SharedKernel;
using GOT.SharedKernel.Utils.Exceptions;
using Newtonsoft.Json;

namespace GOT.Logic.Strategies
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class MainStrategy : BaseStrategy<Future>
    {
        private IConnector _connector;

        private Future _instrument;

        private IGotLogger _logger;

        private string _name;

        private INotification _notification;

        public MainStrategy()
        {
            OptionHolder = new OptionHolder();
            HedgeHolder = new HedgeHolder();
        }

        public override IConnector Connector
        {
            get => _connector;
            set
            {
                _connector = value;
                OptionHolder.SetConnector(_connector);
                HedgeHolder.SetConnector(_connector);
            }
        }

        public override INotification Notification
        {
            get => _notification;
            set
            {
                _notification = value;
                OptionHolder.SetNotification(_notification);
                HedgeHolder.SetNotification(_notification);
            }
        }

        public override IGotLogger Logger
        {
            get => _logger;
            set
            {
                _logger = value;
                OptionHolder.SetLogger(_logger);
                HedgeHolder.SetLogger(_logger);
            }
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                OptionHolder.SetParentName(_name);
                HedgeHolder.SetParentName(_name);
                OnPropertyChanged();
            }
        }

        public override Future Instrument
        {
            get => _instrument;
            set
            {
                _instrument = value;
                OptionHolder.SetParentInstrument(_instrument);
                HedgeHolder.SetParentInstrument(_instrument);
                OnPropertyChanged();
            }
        }

        public bool IsExistsHolders()
        {
            var isOptionExists = OptionHolder.ExistsStrategies();
            var isHedgeExists = HedgeHolder.ExistsStrategies();
            return isOptionExists || isHedgeExists;
        }

        
        protected override void OnInstrumentChanged(InstrumentDTO inst)
        {
            if (Instrument.Id != inst.Id) {
                return;
            }
            RefreshAll();

            if (inst.LastPrice != 0) {
                Instrument.LastPrice = inst.LastPrice;
            }

            if (ClosingState == ClosingState.Reenter) {
                var position = Math.Abs(OptionHolder.PositionContainers) + Math.Abs(HedgeHolder.PositionContainers);
                if (position == 0) {
                    OnCloseAllPositions();
                }

                return;
            }

            if (PnlPercent < PercentAutoClosing) {
                return;
            }

            if (Instrument.LastPrice == 0) {
                return;
            }

            var isTimeToAutoClose = OptionHolder.CheckToAutoClose(Instrument.LastPrice, AutoClosingShift);
            if (isTimeToAutoClose) {
                SetClosingState(ClosingState.Reenter);
            }
        }

        private void RefreshAll()
        {
            OnPropertyChanged("PnlPercent");
            OnPropertyChanged("PnlOption");
            OnPropertyChanged("PnlHedge");
            OnPropertyChanged("Pnl");
            OnPropertyChanged("PnlOptionCurrency");
            OnPropertyChanged("PnlHedgeCurrency");
            OnPropertyChanged("PnlCurrency");
            OnPropertyChanged("OptionHolder");
            OnPropertyChanged("HedgeHolder");
            OnPropertyChanged("MainOptions");
            OnPropertyChanged("OptionBalance");
            OnPropertyChanged("OptionBalanceCurrency");
            OnPropertyChanged("CostOptionPosition");
        }

        private void StartHolders()
        {
            OptionHolder.StartAllContainers();
            HedgeHolder.StartAllContainers();
        }

        private void StopHolders()
        {
            OptionHolder.StopAllContainers();
            HedgeHolder.StopAllContainers();
        }

        #region properties

        private string _account;

        [JsonProperty("account")]
        public override string Account
        {
            get => _account;
            set
            {
                _account = value;
                OptionHolder.SetAccount(_account);
                HedgeHolder.SetAccount(_account);
            }
        }

        [JsonProperty("autoRestartCounter")]
        public int AutoRestartCounter { get; set; }

        [JsonProperty("creatingDate")]
        public DateTime CreatingDate { get; set; }

        [JsonProperty("autoRestartDate")]
        public DateTime AutoRestartDate { get; set; }

        /// <summary>
        ///     Cмещение диапазона относительно середины страйков
        /// </summary>
        [JsonProperty("autoClosingShift")]
        public decimal AutoClosingShift { get; set; }

        private decimal _percentAutoClosing;

        /// <summary>
        ///     Процент, при котором происходит автозакрытие позиций.
        ///     Задается пользователем, а также используется как процент для закрытия по умолчанию(сохраняется/загружается)
        /// </summary>
        [JsonProperty("percentAutoClosing")]
        public decimal PercentAutoClosing
        {
            get => _percentAutoClosing;
            set
            {
                if (_percentAutoClosing == value) {
                    return;
                }

                _percentAutoClosing = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Накопленный Pnl
        /// </summary>
        [JsonProperty("accruedPnl")]
        public decimal AccruedPnl { get; set; }

        private OptionHolder _optionHolder;

        [JsonProperty("optionHolder")]
        public OptionHolder OptionHolder
        {
            get => _optionHolder;
            set
            {
                _optionHolder = value;
                OnPropertyChanged();
            }
        }

        private HedgeHolder _hedgeHolder;

        [JsonProperty("hedgeHolder")]
        public HedgeHolder HedgeHolder
        {
            get => _hedgeHolder;
            set
            {
                _hedgeHolder = value;
                _hedgeHolder.SetConnector(Connector);
                _hedgeHolder.SetLogger(Logger);
                _hedgeHolder.SetNotification(Notification);
                _hedgeHolder.SetParentInstrument(Instrument);
                OnPropertyChanged();
            }
        }

        private ClosingState _closingState;

        public ClosingState ClosingState
        {
            get => _closingState;
            private set
            {
                _closingState = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Задает состояние закрытия стратегии.
        /// </summary>
        public void SetClosingState(ClosingState state)
        {
            ClosingState = state;
            switch (_closingState) {
                case ClosingState.Manual:
                    ClosePositions();
                    break;
                case ClosingState.Reenter:
                    StartReenter();
                    ClosePositions();
                    break;
            }
        }

        public IEnumerable<OptionStrategy> MainOptions => OptionHolder.GetMainStrategies();

        /// <summary>
        ///     Премия Рассчитывает стоимость проданных опционов.
        /// </summary>
        public decimal OptionBalance => OptionHolder.GetSellPositionCost();

        public decimal OptionBalanceCurrency => OptionBalance * Instrument.Multiplier;

        /// <summary>
        ///     Стоимость портфеля опционов. Рассчитывает стоимость проданных и купленных опционов.
        /// </summary>
        public decimal CostOptionPosition => OptionHolder.GetPositionCost();

        public decimal PnlOption => OptionHolder.PnlContainers;

        public decimal PnlOptionCurrency => PnlOption * Instrument.Multiplier;

        public decimal PnlHedge => HedgeHolder.PnlContainers;

        public decimal PnlHedgeCurrency => PnlHedge * Instrument.Multiplier;

        public override decimal Pnl => PnlOption + PnlHedge;
        public override decimal PnlCurrency => Pnl * Instrument.Multiplier;

        /// <summary>
        ///     Параметр для отображения процента (в %)
        /// </summary>
        public decimal PnlPercent => Math.Round(CostOptionPosition == 0 ? 0 : Pnl * 100 / CostOptionPosition);

        #endregion

        #region Override methods

        public override void Start()
        {
            try {
                var messageAboutChildVolumes = IsNotEqualsChildVolumes(OptionHolder, HedgeHolder);
                if (messageAboutChildVolumes != string.Empty) {
                    throw new StrategyException(messageAboutChildVolumes);
                }

                SubscribeInstrument(Instrument);
                SetClosingState(ClosingState.None);

                Connector.FutureChanged += OnInstrumentChanged;

                StartHolders();
                base.Start();
            }
            catch (StrategyException ex) {
                Stop();
                MessageBox.Show(ex.Message);
            }
        }

        public string IsNotEqualsChildVolumes(OptionHolder option, HedgeHolder hedge)
        {
            var sellPutOptions = option.GetVolumeMainPutOptions();
            var sellHedges = hedge.GetMainSellVolumes();

            if (sellPutOptions > sellHedges) {
                return "Объем Put-опционов больше, чем сумма объемов сетки!";
            }

            var sellCallOptions = option.GetVolumeMainCallOptions();
            var buyHedges = hedge.GetMainBuyVolumes();
            if (sellCallOptions > buyHedges) {
                return "Объем Call-опционов больше, чем сумма объемов сетки!";
            }

            return string.Empty;
        }

        public override void Stop()
        {
            StopHolders();
            Connector.FutureChanged -= OnInstrumentChanged;
            base.Stop();
        }

        public override bool Reset()
        {
            AutoRestartCounter = 0;
            CreatingDate = DateTime.Now;
            AccruedPnl = 0;
            OptionHolder.ClearContainers();
            HedgeHolder.ResetAllContainers();
            RefreshAll();
            return true;
        }

        public override void ClosePositions()
        {
            OptionHolder.CloseAllPositions();
            HedgeHolder.CloseAllPositions();
        }

        #endregion

        #region autoclosing

        private void StartReenter()
        {
            AccruedPnl += Pnl;
            AutoRestartDate = DateTime.Now.ToLocalTime();
            AutoRestartCounter++;

            SendInfoNotification($"Strategy: {Name}. Auto-restart! Count: {AutoRestartCounter.ToString()}");
        }

        private void OnCloseAllPositions()
        {
            var shiftStrikeStep = OptionHolder.GetShiftStrikeStep(Instrument.LastPrice);
            Dispatcher?.Invoke(() =>
            {
                OptionHolder.UpdateInstruments(shiftStrikeStep);
                HedgeHolder.UpdateInstruments(shiftStrikeStep);
                if (HedgeHolder.ResetAllContainers()) {
                    StartAfterReenter();
                }
            }, DispatcherPriority.Normal, CancellationToken.None);
        }

        private void StartAfterReenter()
        {
            ClosingState = ClosingState.None;
            StartHolders();
        }

        #endregion

        public override string ToString()
        {
            return Name + " " + Instrument?.FullName;
        }
    }
}