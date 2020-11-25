using System;
using System.Collections.Generic;
using System.Linq;
using GOT.Logic.Enums;
using GOT.Logic.Models;
using MoreLinq;

namespace GOT.Logic.Utils.Helpers
{
    public static class PnlCalculator
    {
        /// <summary>
        ///     Расчитывает Pnl по ордерам.
        /// </summary>
        /// <param name="filledOrders">Исполненные ордера</param>
        /// <param name="lastPrice">Последняя цена актива, для корректного пересчёта</param>
        /// <returns></returns>
        public static decimal CalculatePnlByOrders(IReadOnlyList<Order> filledOrders, decimal lastPrice)
        {
            decimal sumPnl;
            var buysFilled = filledOrders.Where(o => o.Direction == Directions.Buy).ToList();
            var sellsFilled = filledOrders.Where(o => o.Direction == Directions.Sell).ToList();

            var open = buysFilled.Sum(o => o.FilledVolume) - sellsFilled.Sum(o => o.FilledVolume);

            if (open == 0) {
                sumPnl = CalculatePnl(filledOrders);
                return sumPnl;
            }

            decimal openedPnl = 0, closedPnl = 0;
            if (open > 0) {
                var openTrades = buysFilled.TakeLast(open).ToList();
                closedPnl = CalculatePnl(filledOrders.Except(openTrades));

                var sumPrices = openTrades.Sum(t => t.ExecutionPrice * t.FilledVolume);
                openedPnl = lastPrice * open - sumPrices;
            }

            if (open < 0) {
                var openTrades = sellsFilled.TakeLast(Math.Abs(open)).ToList();
                closedPnl = CalculatePnl(filledOrders.Except(openTrades));

                var sumPrices = openTrades.Sum(t => t.ExecutionPrice * t.FilledVolume);
                openedPnl = sumPrices - lastPrice * Math.Abs(open);
            }

            sumPnl = openedPnl + closedPnl;
            return sumPnl;
        }

        private static decimal CalculatePnl(IEnumerable<Order> orders)
        {
            return orders.Sum(t => t.Direction == Directions.Sell
                ? t.ExecutionPrice * t.FilledVolume
                : -t.ExecutionPrice * t.FilledVolume);
        }
    }
}