using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using GOT.Logic.Connectors;
using GOT.Logic.Enums;
using GOT.Logic.Models.Instruments;
using GOT.Logic.Strategies.Bases;
using GOT.Logic.Utils;
using GOT.Notification;
using GOT.SharedKernel;
using MoreLinq;

namespace GOT.Logic.Strategies.Hedges
{
    public sealed class HedgeContainer : BaseContainer<HedgeStrategy>
    {
        private Future _parentInstrument;
        private Rubicon _rubicon;

        public bool HasActiveStrategy;

        public HedgeContainer(string name) : base(name)
        {
            _rubicon = new Rubicon(Strategies);
            Strategies.CollectionChanged += StrategiesOnCollectionChanged;
        }

        public override Future ParentInstrument
        {
            get => _parentInstrument;
            set
            {
                _parentInstrument = value;
                foreach (var s in Strategies) {
                    s.Instrument = _parentInstrument;
                }
            }
        }

        public override int Position => Strategies.Sum(s => s.IsRubiconStrategy ? 0 : s.Position);
        public override decimal Pnl => Strategies.Sum(s => s.Pnl);

        public HedgeSessionStatus HedgeSessionStatus => Strategies.GetStateStrategies();

        public string HedgeInfo => Strategies.GetHedgeInfoTemplate();

        internal void SetParentName(string newName)
        {
            ParentStrategyName = newName;
            Strategies.ForEach(s => s.Name = $"parent: {ParentStrategyName}, container:{Name} ");
        }

        public override void SetConnector(IConnector connector)
        {
            Connector = connector;
            Strategies.ForEach(s => s.Connector = Connector);
        }

        public override void SetNotifications(INotification[] notification)
        {
            GotNotifications = notification;
            Strategies.ForEach(s => s.Notifications = notification);
        }

        public override void SetLogger(IGotLogger logger)
        {
            Logger = logger;
            Strategies.ForEach(s => s.Logger = logger);
        }

        public void AddStrategy(Directions direction)
        {
            var name = $"parent: {ParentStrategyName}, container: {Name} ";
            var newInstrument = ParentInstrument.CreateNewInstance();
            var strategy = new HedgeStrategy(name)
            {
                Connector = Connector,
                Notifications = GotNotifications,
                Logger = Logger,
                Instrument = newInstrument,
                Direction = direction,
                Container = this
            };
            AddStrategy(strategy);
        }

        public override void StartContainer()
        {
            Strategies.ForEach(StartSingleStrategy);
        }

        public void StartSingleStrategy(HedgeStrategy hedgeStrategy)
        {
            if (hedgeStrategy.StrategyState == StrategyStates.Started) return;
            HasActiveStrategy = false;
            hedgeStrategy.Container ??= this;
            hedgeStrategy.Start();
        }

        public override void StopContainer()
        {
            Strategies.ForEach(StopSingleStrategy);
        }

        public void StopSingleStrategy(HedgeStrategy hedgeStrategy)
        {
            HasActiveStrategy = true;
            hedgeStrategy.Stop();
        }

        public override void ClosePositions()
        {
            foreach (var strategy in Strategies) {
                strategy.ClosePositions();
            }
        }

        public void StartRubicon(Directions direction, int volume)
        {
            if (_rubicon.IsStarted) {
                return;
            }

            _rubicon.Start(direction, volume);
            Logger.AddLog($"Rubicon is started on strategy: {ParentStrategyName}/container: {Name}");
        }

        public void StopRubicon(Directions directions)
        {
            if (!_rubicon.IsStarted) {
                return;
            }

            _rubicon.Stop(directions);
            Logger.AddLog($"Rubicon is stopped on strategy: {ParentStrategyName}/container: {Name}");
        }

        /// <summary>
        ///     Обнуляет всю информацию по сетках.
        /// </summary>
        public bool ResetStrategies()
        {
            return Strategies.All(s => s.Reset());
        }

        /// <summary>
        ///     Обновляет информацию о сетке, после изменения какого-либо параметра
        /// </summary>
        private void StrategiesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null) {
                foreach (var item in e.NewItems) {
                    ((INotifyPropertyChanged) item).PropertyChanged += OnPropertyChanged;
                }
            }

            if (e.OldItems != null) {
                foreach (var item in e.OldItems) {
                    ((INotifyPropertyChanged) item).PropertyChanged -= OnPropertyChanged;
                }
            }

            OnPropertyChanged("HedgeInfo");
            OnPropertyChanged("HedgeSessionStatus");
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case "StrategyState":
                    OnPropertyChanged("HedgeSessionStatus");
                    break;
                case "Position":
                    SetFilledVolume();
                    OnPropertyChanged("HedgeInfo");
                    OnPropertyChanged("Pnl");
                    break;
                case "Volume":
                case "ActivatePrice":
                    OnPropertyChanged("HedgeInfo");
                    UpdateLevels();
                    break;
            }
        }

        /// <summary>
        ///     Устанавливает исполненный объём
        /// </summary>
        private void SetFilledVolume()
        {
            foreach (var s in Strategies) {
                if (s.Position == 0) {
                    s.FilledVolume = 0;
                } else {
                    if (s.Direction == Directions.Sell) {
                        s.FilledVolume = GetHighLevelSellStrategies(s.Direction, s.ActivatePrice)
                            .Sum(str => str.Position);
                    } else {
                        s.FilledVolume = GetLowLevelBuyStrategies(s.Direction, s.ActivatePrice)
                            .Sum(str => str.Position);
                    }
                }
            }
        }

        /// <summary>
        ///     Сдвинуть дочерние стратегии.
        /// </summary>
        /// <param name="step">Шаг для сдвига.</param>
        public bool ShiftChildStrategies(decimal step)
        {
            if (step != 0) {
                Strategies.ForEach(s => s.ActivatePrice += step);
            }

            return true;
        }

        /// <summary>
        ///     Переназначает номера уровней
        /// </summary>
        public void UpdateLevels()
        {
            if (!Strategies.Any()) {
                return;
            }

            UpdateLevels(Directions.Buy, OrderByDirection.Ascending);
            UpdateLevels(Directions.Sell, OrderByDirection.Descending);
            SetFilledVolume();
        }

        private void UpdateLevels(Directions direction, OrderByDirection sequence)
        {
            var hedgeStrategies =
                GetStrategiesByDirection(direction).OrderBy(s => s.ActivatePrice, sequence);
            var levelCounter = 1;
            foreach (var s in hedgeStrategies) {
                s.Level = s.Direction + $" {levelCounter.ToString()}";
                levelCounter++;
            }
        }

        /// <summary>
        ///     Берется оставшийся на нижних(buy) или верхних(sell) уровнях объем для переноса на след. стратегию.
        /// </summary>
        /// <param name="direction">Направление</param>
        /// <param name="activatePrice">Цена, от которой вести счёт</param>
        public int GetTradableVolume(Directions direction, decimal activatePrice)
        {
            int volume;

            if (direction == Directions.Sell) {
                volume = GetHighLevelSellStrategies(direction, activatePrice)
                    .Sum(s => s.Volume);
                volume *= -1;
            } else {
                volume = GetLowLevelBuyStrategies(direction, activatePrice)
                    .Sum(s => s.Volume);
            }

            var tradedVolume = volume - Position;
            return tradedVolume;
        }

        private IEnumerable<HedgeStrategy> GetHighLevelSellStrategies(Directions direction, decimal activatePrice)
        {
            var sells = GetStrategiesByDirection(direction)
                .Where(s => s.ActivatePrice >= activatePrice);
            return sells;
        }

        private IEnumerable<HedgeStrategy> GetLowLevelBuyStrategies(Directions direction, decimal activatePrice)
        {
            var buys = GetStrategiesByDirection(direction)
                .Where(s => s.ActivatePrice <= activatePrice);
            return buys;
        }

        public bool CheckStrategiesState(StrategyStates states)
        {
            return Strategies.All(s => s.StrategyState == states);
        }

        public int GetAllStrategiesVolumes(Directions directions)
        {
            return GetStrategiesByDirection(directions).Sum(s => s.Volume);
        }

        private IEnumerable<HedgeStrategy> GetStrategiesByDirection(Directions directions)
        {
            return Strategies.Where(s => s.Direction == directions && !s.IsRubiconStrategy);
        }

        #region ContextMenuCommands

        /// <summary>
        ///     Меняем цену активации у стратегий с направлением Sell
        /// </summary>
        public void ChangeSellActivatePrices(decimal valueForCorrection, decimal stepPrice)
        {
            Strategies.SellPriceChange(valueForCorrection, stepPrice);
        }

        /// <summary>
        ///     Меняем цену активации у стратегий с направлением Buy
        /// </summary>
        public void ChangeBuyActivatePrices(decimal valueForCorrection, decimal stepPrice)
        {
            Strategies.BuyPriceChange(valueForCorrection, stepPrice);
        }

        /// <summary>
        ///     В зависимости от выбранного параметра сетки, изменяет его значение.
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="valueForCorrection"></param>
        public void StopStrategyFieldChange(HedgeStrategyFields fields, decimal valueForCorrection)
        {
            Strategies.SetStrategiesFieldValues(fields, valueForCorrection);
        }

        #endregion
    }
}