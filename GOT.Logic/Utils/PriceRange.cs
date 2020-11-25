using System;
using GOT.Logic.Enums;
using GOT.Logic.Utils.Helpers;

namespace GOT.Logic.Utils
{
    /// <summary>
    ///     Диапазон цен, в пределах которых будет торговаться инструмент
    /// </summary>
    public class PriceRange
    {
        private decimal _max;

        private decimal _min;

        /// <summary>
        ///     Шаг цены инструмента
        /// </summary>
        private decimal _priceStep;

        public PriceRange()
        {
            _max = decimal.MinValue;
            _min = decimal.MaxValue;
        }

        /// <summary>
        ///     Обновление диапазона цен
        /// </summary>
        /// <param name="price">Базовая цена</param>
        /// <param name="direction">Направление</param>
        /// <param name="offset">Шаг смещения цены от базовой</param>
        /// <param name="priceStep">Шаг цены <see cref="_priceStep" /></param>
        public void UpdateRange(decimal price, Directions direction, decimal offset, decimal priceStep)
        {
            _priceStep = priceStep;
            if (offset >= 0) {
                if (direction == Directions.Sell) {
                    _max = price;
                    _min = price - _priceStep * offset;
                } else {
                    _max = price + _priceStep * offset;
                    _min = price;
                }
            } else {
                if (direction == Directions.Sell) {
                    _max = price - _priceStep * offset;
                    _min = price;
                } else {
                    _max = price;
                    _min = price + _priceStep * offset;
                }
            }
        }

        public decimal CalculateNewPrice(decimal currentPrice, Directions direction, decimal ask, decimal bid)
        {
            var price = 0m;
            if (InRange(currentPrice)) {
                if (direction == Directions.Sell) {
                    var askToMinRange = currentPrice > ask && ask >= _min;
                    if (askToMinRange) {
                        price = Math.Max(ask - _priceStep, _min);
                    }
                } else {
                    var bidToMaxRange = currentPrice < bid && bid <= _max;
                    if (bidToMaxRange) {
                        price = Math.Min(bid + _priceStep, _max);
                    }
                }
            }

            if (price == 0) {
                price = GetStartPrice(direction);
            }

            return MathHelper.RoundUp(price, _priceStep);
        }

        public bool InRange(decimal price)
        {
            return price >= _min && price <= _max;
        }

        /// <summary>
        ///     Получить начальную цену. При покупке котируем по максимальной при продаже по минимальной.
        /// </summary>
        private decimal GetStartPrice(Directions currentDirection)
        {
            return currentDirection == Directions.Sell ? _min : _max;
        }
    }
}