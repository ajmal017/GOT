using CQG;
using GOT.Logic.Enums;
using GOT.Logic.Models;

namespace GOT.Logic.Utils.Helpers
{
    public static class OrderHelper
    {
        public static eOrderType ToCqgOrderType(this Order order)
        {
            return order.OrderType switch
            {
                OrderTypes.Stop => eOrderType.otStop,
                OrderTypes.Limit => eOrderType.otLimit,
                OrderTypes.Market => eOrderType.otMarket,
                OrderTypes.StopLimit => eOrderType.otStopLimit,
                _ => eOrderType.otMarket
            };
        }

        public static OrderState SetOrderState(string state)
        {
            switch (state.ToUpper()) {
                case "FILLED":
                    return OrderState.Filled;
                case "INACTIVE":
                case "APICANCELLED":
                    return OrderState.Failed;
                case "CANCELLED":
                    return OrderState.Cancelled;
                case "SUBMITTED":
                case "PRESUBMITTED":
                    return OrderState.Active;
                default:
                    return OrderState.None;
            }
        }

        public static IBApi.Order ToIbOrder(this Order order)
        {
            var ibOrder = new IBApi.Order
            {
                OrderId = order.Id,
                TotalQuantity = order.Volume,
                Account = order.Account,
                Action = order.ToIbDirection(),
                OrderType = order.ToIbOrderType()
            };
            switch (order.OrderType) {
                case OrderTypes.Stop:
                    ibOrder.AuxPrice = decimal.ToDouble(order.Price);
                    break;
                case OrderTypes.Market:
                case OrderTypes.Limit:
                    ibOrder.LmtPrice = decimal.ToDouble(order.Price);
                    break;
            }

            return ibOrder;
        }

        private static string ToIbDirection(this Order order)
        {
            return order.Direction == Directions.Buy ? "BUY" : "SELL";
        }

        private static string ToIbOrderType(this Order order)
        {
            return order.OrderType switch
            {
                OrderTypes.Stop => "STP",
                OrderTypes.Limit => "LMT",
                OrderTypes.Market => "MKT",
                OrderTypes.MarketIfTouched => "MIT",
                OrderTypes.MarketToLimit => "MTL",
                OrderTypes.StopLimit => "STP LMT",
                _ => "MKT"
            };
        }
    }
}