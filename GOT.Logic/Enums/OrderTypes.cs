namespace GOT.Logic.Enums
{
    public enum OrderTypes : byte
    {
        /// A Market order is an order to buy or sell at the market bid or offer price. A market order may increase the likelihood of a fill 
        /// and the speed of execution, but unlike the Limit order a Market order provides no price protection and may fill at a price far 
        /// lower/higher than the current displayed bid/ask.
        Market,

        /// A Limit order is an order to buy or sell at a specified price or better. The Limit order ensures that if the order fills, 
        /// it will not fill at a price less favorable than your limit price, but it does not guarantee a fill.
        Limit,

        /// A Stop order is an instruction to submit a buy or sell market order if and when the user-specified stop trigger price is attained or 
        /// penetrated. A Stop order is not guaranteed a specific execution price and may execute significantly away from its stop price. A Sell 
        /// Stop order is always placed below the current market price and is typically used to limit a loss or protect a profit on a long stock 
        /// position. A Buy Stop order is always placed above the current market price. It is typically used to limit a loss or help protect a 
        /// profit on a short sale.
        Stop,

        /// A Market if Touched (MIT) is an order to buy (or sell) a contract below (or above) the market. Its purpose is to take advantage 
        /// of sudden or unexpected changes in share or other prices and provides investors with a trigger price to set an order in motion. 
        /// Investors may be waiting for excessive strength (or weakness) to cease, which might be represented by a specific price point. 
        /// MIT orders can be used to determine whether or not to enter the market once a specific price level has been achieved. This order 
        /// is held in the system until the trigger price is touched, and is then submitted as a market order. An MIT order is similar to a 
        /// stop order, except that an MIT sell order is placed above the current market price, and a stop sell order is placed below
        MarketIfTouched,

        /// A Market-to-Limit (MTL) order is submitted as a market order to execute at the current best market price. If the order is only 
        /// partially filled, the remainder of the order is canceled and re-submitted as a limit order with the limit price equal to the price 
        /// at which the filled portion of the order executed.
        MarketToLimit,

        /// A Stop-Limit order is an instruction to submit a buy or sell limit order when the user-specified stop trigger price is attained or 
        /// penetrated. The order has two basic components: the stop price and the limit price. When a trade has occurred at or through the stop 
        /// price, the order becomes executable and enters the market as a limit order, which is an order to buy or sell at a specified price or better.
        StopLimit
    }
}