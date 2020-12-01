using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using GOT.Logic.Enums;
using GOT.Logic.Strategies.Hedges;

namespace GOT.Logic.Utils
{
    public class Rubicon
    {
        private const int RUBICON_STRATEGY_COUNT = 2;
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private readonly ICollection<HedgeStrategy> _strategies;

        public Rubicon(ICollection<HedgeStrategy> strategies)
        {
            _strategies = strategies;
        }

        public bool IsStarted { get; private set; }

        /// <summary>
        ///     Запускает функцию рубикона
        /// </summary>
        /// <param name="direction">Сторона, на которой будут созданы стратегии рубикона</param>
        /// <param name="volume">Объем для распределения на рубиконовых стратегиях</param>
        public void Start(Directions direction, decimal volume)
        {
            IsStarted = true;
            AddRubiconStrategies(direction, volume);
        }

        /// <summary>
        ///     Останавливает функцию рубикона
        /// </summary>
        /// <param name="direction">Сторона, на которой рубикон будет остановлен</param>
        public void Stop(Directions direction)
        {
            IsStarted = false;
            CloseAndRemoveStrategies(direction);
        }

        /// <summary>
        ///     Добавляет в сетку новые строки для "Рубикона"
        ///     Высчитывая необходимые параметры на основе последних двух стратегий в сетке по указанному направлению.
        /// </summary>
        private void AddRubiconStrategies(Directions direction, decimal volume)
        {
            IOrderedEnumerable<HedgeStrategy> orderedCollection = null;
            switch (direction) {
                case Directions.Sell:
                    orderedCollection = _strategies.OrderBy(s => s.ActivatePrice);
                    break;
                case Directions.Buy:
                    orderedCollection = _strategies.OrderByDescending(s => s.ActivatePrice);
                    break;
            }

            var gotStrategies = orderedCollection?.Take(RUBICON_STRATEGY_COUNT).ToList();

            var rubiconStrategies = AddTempStrategies(gotStrategies, Math.Abs(volume));
            AverageVolume(rubiconStrategies, Math.Abs(volume));

            _dispatcher.Invoke(() =>
            {
                foreach (var strategy in rubiconStrategies) {
                    _strategies.Add(strategy);
                    strategy.Start();
                }
            });
        }


        private IList<HedgeStrategy> AddTempStrategies(IList<HedgeStrategy> strategies, decimal volume)
        {
            var rubiconStrategies = new List<HedgeStrategy>();
            var levelCounter = 1;

            var stepActivatePrice = GetStepActivatePrice(strategies);

            var newStrategy = CreateRubiconStrategy(strategies[0], levelCounter, stepActivatePrice);
            rubiconStrategies.Add(newStrategy);

            if (Math.Abs(volume) > 1) {
                var secondNewStrategy = CreateRubiconStrategy(newStrategy, ++levelCounter, stepActivatePrice);
                rubiconStrategies.Add(secondNewStrategy);
            }

            return rubiconStrategies;
        }

        private static decimal GetStepActivatePrice(IList<HedgeStrategy> strategies)
        {
            var step = 0m;
            for (var i = 1; i < strategies.Count; i++) {
                step = strategies[i].ActivatePrice - strategies[i - 1].ActivatePrice;
            }

            return Math.Abs(step);
        }

        private HedgeStrategy CreateRubiconStrategy(HedgeStrategy baseStrategy, int levelCounter,
            decimal stepActivatePrice)
        {
            var newStrategy = new HedgeStrategy(baseStrategy.Name)
            {
                IsRubiconStrategy = true,
                Volume = 0,
                ActivatePrice = baseStrategy.ActivatePrice,
                AntiOffset = baseStrategy.AntiOffset,
                AntiBreakEvenOffset = baseStrategy.AntiBreakEvenOffset,
                StopRestartOffset = baseStrategy.StopRestartOffset,
                Direction = baseStrategy.Direction,
                ShiftStepPrice = baseStrategy.ShiftStepPrice,
                Notification = baseStrategy.Notification,
                Logger = baseStrategy.Logger,
                Connector = baseStrategy.Connector,
                Instrument = baseStrategy.Instrument,
                Container = baseStrategy.Container,
                Level = $"Temp {levelCounter}"
            };

            if (newStrategy.Direction == Directions.Sell) {
                newStrategy.ActivatePrice -= stepActivatePrice;
            } else {
                newStrategy.ActivatePrice += stepActivatePrice;
            }

            return newStrategy;
        }

        private static void AverageVolume(IList<HedgeStrategy> strategies, decimal? volume)
        {
            var count = strategies.Count;
            for (var i = 0; i < count; i++) {
                if (volume != null) {
                    var avg = (int) (volume / (count - i));
                    volume -= Math.Abs(avg);
                    strategies[i].Volume += Math.Abs(avg);
                }
            }
        }

        /// <summary>
        ///     Закрывает позиции по временным стратегиям и удаляет их из сетки.
        /// </summary>
        private void CloseAndRemoveStrategies(Directions direction)
        {
            var usingStrategies = _strategies
                                  .Where(s => s.Direction == direction && s.IsRubiconStrategy).ToList();
            usingStrategies.ForEach(strategy =>
            {
                if (strategy.Position != 0) {
                    strategy.ClosePositions();
                }
            });
            RemoveRubiconStrategies(usingStrategies);
        }

        private void RemoveRubiconStrategies(IEnumerable<HedgeStrategy> strategies)
        {
            _dispatcher.Invoke(() =>
            {
                foreach (var strategy in strategies) {
                    _strategies.Remove(strategy);
                }
            });
        }
    }
}