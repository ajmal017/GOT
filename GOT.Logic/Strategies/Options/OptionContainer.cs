using System;
using System.Collections.Generic;
using System.Linq;
using GOT.Logic.Connectors;
using GOT.Logic.Enums;
using GOT.Logic.Models.Instruments;
using GOT.Logic.Strategies.Bases;
using GOT.Notification;
using GOT.SharedKernel;
using GOT.SharedKernel.Utils.Exceptions;
using MoreLinq.Extensions;

namespace GOT.Logic.Strategies.Options
{
    public class OptionContainer : BaseContainer<OptionStrategy>
    {
        public readonly bool IsMain;

        public OptionContainer(string name) : base(name)
        {
        }

        public OptionContainer(bool isMainContainer, string name) : this(name)
        {
            IsMain = isMainContainer;
        }

        public override int Position => Strategies.Sum(s => s.Position);
        public override decimal Pnl => Strategies.Sum(s => s.Pnl);

        public override void StartContainer()
        {
            try {
                Strategies.ForEach(s =>
                {
                    if (s.StrategyState != StrategyStates.Started) {
                        s.Start();
                    }
                });
            }
            catch (Exception ex) {
                Logger.AddLog($"{ex.Message} / {ex.StackTrace}", 3);
                StopContainer();
            }
        }

        public override void StopContainer()
        {
            try {
                Strategies.ForEach(s =>
                {
                    s.Stop();
                    Connector.DescribeInstrument(s.Instrument.Id);
                });
            }
            catch (Exception ex) {
                Logger.AddLog($"{ex.StackTrace}", 3);
            }
        }

        public override void Remove(OptionStrategy strategy)
        {
            Connector.RemoveInstrument(strategy.Instrument);
            base.Remove(strategy);
        }

        public bool CanAddStrategy()
        {
            return Connector != null && ParentInstrument.Code != null;
        }

        public override void ClosePositions()
        {
            foreach (var strategy in Strategies) {
                strategy.ClosePositions();
            }
        }

        /// <summary>
        ///     Берет стратегии, отмеченные как базовые.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<OptionStrategy> GetBasisStrategies()
        {
            return Strategies.Where(s => s.IsBasis).ToList();
        }

        /// <summary>
        ///     Получить объем всех продажных базовых стратегий
        /// </summary>
        /// <param name="optionTypes">Тип опционов</param>
        /// <returns>Общий объем стратегий</returns>
        /// <remarks>Рисковыми считаются только продажные стратегии, которые являются основными.</remarks>
        public int GetSellVolumes(OptionTypes optionTypes)
        {
            return GetBasisStrategies()
                   .Where(s => s.Direction == Directions.Sell && s.OptionType == optionTypes)
                   .Sum(s => s.Volume);
        }

        public decimal GetPositionCost()
        {
            return Math.Abs(Strategies.Sum(s =>
            {
                return s.FilledOrders.Sum(o => o.Direction == Directions.Sell
                    ? -o.ExecutionPrice * o.FilledVolume
                    : o.ExecutionPrice * o.FilledVolume);
            }));
        }

        /// <summary>
        ///     Рассчитывает стоимость проданных опционов.
        /// </summary>
        public decimal GetSellPositionCost()
        {
            return Math.Abs(Strategies.Sum(s =>
            {
                return s.FilledOrders.Sum(o => o.Direction == Directions.Sell
                    ? -o.ExecutionPrice * o.FilledVolume
                    : 0);
            }));
        }

        public decimal GetShiftStrikeStep(decimal currentPrice)
        {
            decimal shiftStrikeStep = 0;

            var strikeStep = CalculateStrikeStep();
            if (strikeStep != 0) {
                var diffPrice = currentPrice - CalculateMiddlePrice();
                var quantityShiftSteps = (int) Math.Round(diffPrice / strikeStep);
                shiftStrikeStep = strikeStep * quantityShiftSteps;
            }

            return shiftStrikeStep;
        }

        /// <summary>
        ///     Проверяет, сдвинулась ли цена инструмента(фьючерса), относительно диапазона страйков.
        /// </summary>
        /// <param name="currentPrice">текущая цена фьючерса</param>
        /// <param name="shift">коэффециент сдвига.</param>
        public bool IsOptionPricesRangeShifted(decimal currentPrice, decimal shift)
        {
            if (!IsAllStrategiesDone()) {
                return false;
            }

            var middleStrikesRange = CalculateMiddleStrikesRange();
            var shiftedPrice = middleStrikesRange * shift;

            var middlePrice = CalculateMiddlePrice();
            var diff = currentPrice - middlePrice;

            //Цена смещения диапазона между опционами
            var priceOptionRange = middlePrice + shiftedPrice * Math.Sign(diff);

            var isMoreCurrentPrice = currentPrice > priceOptionRange && currentPrice > middlePrice;
            var isLessCurrentPrice = currentPrice < priceOptionRange && currentPrice < middlePrice;

            if (isMoreCurrentPrice || isLessCurrentPrice) {
                var info =
                    $"Auto_p: Диапазон: {priceOptionRange.ToString()}, сдвиг цены: {shiftedPrice.ToString()} "
                    + $"средняя цена: {middlePrice.ToString()}, разница цен: {diff.ToString()}";
                Logger.AddLog(info);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Высчитать шаг страйка
        /// </summary>
        public decimal CalculateStrikeStep()
        {
            try {
                var instruments = Connector.GetOptions(ParentInstrument.Code, ParentInstrument.Exchange);
                var currentInstrument = GetBasisStrategies().First().Instrument;
                var sortedList = instruments.Where(i =>
                {
                    var isExp = i.ExpirationDate.Equals(currentInstrument.ExpirationDate);
                    var isCall = i.OptionType == currentInstrument.OptionType;
                    return isExp && isCall;
                }).OrderBy(s => s.Strike).ToList();

                var avgQuantity = (int) Math.Round(sortedList.Count / 2d);
                var firstStrike = sortedList[avgQuantity].Strike;
                var secondStrike = sortedList[avgQuantity - 1].Strike;
                var strikeStep = Math.Abs((decimal) (firstStrike - secondStrike));
                return strikeStep;
            }
            catch (Exception e) {
                Logger.AddLog(e.Message, 3);
                return 0;
            }
        }

        /// <summary>
        ///     Высчитать среднюю цену базовых опционов.
        /// </summary>
        private decimal CalculateMiddlePrice()
        {
            return GetBasisStrategies().Average(s => s.Instrument.Strike);
        }

        /// <summary>
        ///     Высчитать середину диапазона между страйками.
        /// </summary>
        private decimal CalculateMiddleStrikesRange()
        {
            var range = 0.0m;
            if (!GetBasisStrategies().Any()) {
                return range;
            }

            var firstStrike = GetBasisStrategies().Max(s => s.Instrument.Strike);
            var lastStrike = GetBasisStrategies().Min(s => s.Instrument.Strike);
            range = Math.Abs(firstStrike - lastStrike) / 2;
            return range;
        }

        private bool IsAllStrategiesDone()
        {
            if (GetBasisStrategies().Count < 2) {
                return false;
            }

            return GetBasisStrategies().All(s => s.RemainVolume == 0);
        }

        public bool UpdateStrategy(decimal shiftStrikeStep)
        {
            try {
                if (!Strategies.Any()) {
                    return false;
                }

                var oldStrategies = Strategies.ToList();
                var instruments = Connector.GetOptions(ParentInstrument.Code, ParentInstrument.Exchange);

                foreach (var oldStrategy in oldStrategies) {
                    var newInstrument = UpdateInstrument(oldStrategy.Instrument, shiftStrikeStep, instruments);
                    var newStrategy = UpdateStrategy(oldStrategy, newInstrument);
                    Strategies.Remove(oldStrategy);
                    Strategies.Add(newStrategy);
                }
            }
            catch (Exception e) {
                Logger.AddLog("Error Create New Options:" + e.Message, 3);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Обновляет опционный инструмент
        /// </summary>
        /// <param name="oldInstrument">текущий инструмент, который необходимо обновить</param>
        /// <param name="shift">Значение, на которое должен сместиться страйк нового инструмента</param>
        /// <param name="options">Список инструментов, среди которых искать новый</param>
        /// <returns>Новый инструмент</returns>
        private Option UpdateInstrument(Option oldInstrument, decimal shift, IEnumerable<Option> options)
        {
            var newOption = oldInstrument;
            var filteredSecurities =
                options.Where(o => o.OptionType == oldInstrument.OptionType &&
                                   o.Strike == oldInstrument.Strike + shift)
                       .OrderBy(o => o.ExpirationDate)
                       .ToList();

            try {
                var oldDate = oldInstrument.ExpirationDate.Date;
                var nowDate = DateTimeOffset.Now.Date;
                if (oldDate < nowDate) {
                    throw new StrategyException("Дата экспирации дата не может быть меньше текущей");
                }

                var diffDays = oldDate.Subtract(nowDate);
                const int minimumMonthDays = 12;
                var isMonthExpiry = diffDays < TimeSpan.FromDays(minimumMonthDays);
                if (isMonthExpiry) {
                    var diffExpiryDates =
                        filteredSecurities.Where(o => o.ExpirationDate != oldInstrument.ExpirationDate);
                    newOption = GetNewMonthInstrument(oldInstrument.MonthNumber, diffExpiryDates);
                }
            }
            catch (StrategyException e) {
                Logger.AddLog(e.Message, 2);
                newOption = filteredSecurities.First(o => o.ExpirationDate == oldInstrument.ExpirationDate);
            }

            return newOption;
        }

        /// <summary>
        ///     Подбирает инструмент со следующим месяцем.
        /// </summary>
        /// <param name="monthNumber">текущий номер месяца(от 1 до 12)</param>
        /// <param name="instruments">Инструменты, среди которых вести отбор.</param>
        /// <returns>Инструмент с новым месяцем.</returns>
        private Option GetNewMonthInstrument(int monthNumber, IEnumerable<Option> instruments)
        {
            var nextMonthNumber = monthNumber + 1;
            var security = instruments.Where(s =>
            {
                if (monthNumber == 12) {
                    return s.MonthNumber == 1;
                }

                return s.MonthNumber == nextMonthNumber;
            }).OrderBy(s => s.ExpirationDate).Last();

            return security;
        }

        private OptionStrategy UpdateStrategy(OptionStrategy oldStrategy, Option newInstrument)
        {
            return new OptionStrategy(oldStrategy.Name)
            {
                Direction = oldStrategy.Direction,
                Volume = oldStrategy.Volume,
                PriceOffset = oldStrategy.PriceOffset,
                IsBasis = oldStrategy.IsBasis,
                OptionType = oldStrategy.OptionType,
                Lifetime = oldStrategy.Lifetime,
                Notifications = GotNotifications,
                Connector = Connector,
                Logger = Logger,
                Instrument = newInstrument
            };
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
    }
}