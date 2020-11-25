using System;
using System.Collections.Generic;
using System.Linq;
using GOT.Logic.Enums;
using GOT.Logic.Strategies.Hedges;
using MoreLinq;

namespace GOT.Logic.Utils
{
    public static class HedgeContainerEx
    {
        public static void SellPriceChange(this ICollection<HedgeStrategy> strategies,
            decimal valueForCorrection, decimal stepPrice)
        {
            var mainValue = valueForCorrection;
            var sellStrategies = strategies.Where(s => s.ActivatePrice >= 0
                                                       && s.Direction == Directions.Sell);
            sellStrategies.ForEach(s =>
            {
                s.ActivatePrice = mainValue;
                mainValue = s.ActivatePrice - stepPrice;
            });
        }

        public static void BuyPriceChange(this ICollection<HedgeStrategy> strategies,
            decimal valueForCorrection, decimal stepPrice)
        {
            var mainValue = valueForCorrection;
            var buyStrategies = strategies.Where(s => s.ActivatePrice >= 0
                                                      && s.Direction == Directions.Buy);
            buyStrategies.ForEach(s =>
            {
                s.ActivatePrice = mainValue;
                mainValue = s.ActivatePrice + stepPrice;
            });
        }

        public static void SetStrategiesFieldValues(this ICollection<HedgeStrategy> strategies,
            HedgeStrategyFields fields, decimal value)
        {
            switch (fields) {
                case HedgeStrategyFields.ActivatePriceAll:
                    strategies.ForEach(s => s.ActivatePrice += value);
                    break;
                case HedgeStrategyFields.ActivatePriceSell:
                    var activatePriceSell = strategies.Where(d => d.Direction == Directions.Sell);
                    activatePriceSell.ForEach(s => s.ActivatePrice += value);
                    break;
                case HedgeStrategyFields.ActivatePriceBuy:
                    var activatePriceBuy = strategies.Where(d => d.Direction == Directions.Buy);
                    activatePriceBuy.ForEach(s => s.ActivatePrice += value);
                    break;
                case HedgeStrategyFields.AntiOffsetAntistop:
                    strategies.ForEach(s => s.AntiOffset = value);
                    break;
                case HedgeStrategyFields.AntiBreakAntistop:
                    strategies.ForEach(s => s.AntiBreakEvenOffset = value);
                    break;
                case HedgeStrategyFields.RestartStop:
                    strategies.ForEach(s => s.StopRestartOffset = value);
                    break;
                case HedgeStrategyFields.ShiftStepPrice:
                    strategies.ForEach(s => s.ShiftStepPrice = (int) value);
                    break;
                case HedgeStrategyFields.VolumeHedge:
                    strategies.ForEach(s => s.Volume = (int) value);
                    break;
                case HedgeStrategyFields.ChangeVolume:
                    strategies.ForEach(s => s.Volume += (int) value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fields), fields, null);
            }
        }

        public static HedgeSessionStatus GetStateStrategies(this ICollection<HedgeStrategy> strategies)
        {
            if (!strategies.Any() || strategies.All(s => s.StrategyState == StrategyStates.Stopped)) {
                return HedgeSessionStatus.Stopped;
            }

            if (strategies.Any() && strategies.All(s => s.StrategyState == StrategyStates.Started)) {
                return HedgeSessionStatus.Started;
            }

            return HedgeSessionStatus.PartialStarted;
        }

        /// <summary>
        ///     Шаблон для колонки с информацией по сетке.
        /// </summary>
        /// <returns></returns>
        public static string GetHedgeInfoTemplate(this ICollection<HedgeStrategy> strategies)
        {
            var hedgeInfo = "Хедж отсутствует";

            if (!strategies.Any()) {
                return hedgeInfo;
            }

            var buyVolCount = strategies.GetSumVolumeStrategies(Directions.Buy);
            var sellVolCount = strategies.GetSumVolumeStrategies(Directions.Sell);

            var buyPosition = strategies.GetSumPositionStrategies(Directions.Buy);
            var sellPosition = strategies.GetSumPositionStrategies(Directions.Sell);

            var avgBuyActivatePrice = strategies.GetAvgActivatePrice(Directions.Buy);
            var avgSellActivatePrice = strategies.GetAvgActivatePrice(Directions.Sell);

            hedgeInfo = $"Buy: vol: {buyVolCount} / pos: {buyPosition} / price: {avgBuyActivatePrice}\n" +
                        $"Sell: vol: {sellVolCount} / pos: {sellPosition} / price: {avgSellActivatePrice}";
            return hedgeInfo;
        }


        /// <summary>
        ///     Средняя цена активации по указанному направлению
        /// </summary>
        /// <returns></returns>
        private static decimal GetAvgActivatePrice(this ICollection<HedgeStrategy> strategies,
            Directions direction)
        {
            var hedgeStrategies = strategies.Where(s => s.Direction == direction && !s.IsRubiconStrategy).ToList();
            if (!hedgeStrategies.Any()) {
                return 0;
            }

            var averageActivatePrices = hedgeStrategies.Average(s => s.ActivatePrice);
            return averageActivatePrices;
        }

        private static decimal GetSumVolumeStrategies(this ICollection<HedgeStrategy> strategies,
            Directions direction)
        {
            var hedgeStrategies = strategies.Where(s => s.Direction == direction && !s.IsRubiconStrategy).ToList();
            return hedgeStrategies.Any() ? hedgeStrategies.Sum(s => s.Volume) : 0;
        }

        private static int GetSumPositionStrategies(this ICollection<HedgeStrategy> strategies,
            Directions direction)
        {
            var hedgeStrategies = strategies.Where(s => s.Direction == direction && !s.IsRubiconStrategy).ToList();
            return hedgeStrategies.Any() ? hedgeStrategies.Sum(s => s.Position) : 0;
        }
    }
}