namespace GOT.Logic.Connectors.InteractiveBrokers
{
    public static class IbCodes
    {
        #region Коды ошибок при работе с IB.

        // https://interactivebrokers.github.io/tws-api/message_codes.html
        public const int LOST_MARKET_CONNECTION = 2103;
        public const int CORRECT_DATA_CONNECTION = 2104;
        public const int LOST_CONNECTION = 1100;
        public const int RESTORE_WITHOUT_DATA_CONNECTION = 1101;
        public const int RESTORE_WITH_DATA_CONNECTION = 1102;
        public const int RESET_SOCKET = 1300;
        public const int ERROR_INSTRUMENT = 200;
        public const int ORDER_REJECTED = 201;
        public const int ORDER_CANCELLED = 202;

        #endregion

        #region Коды тиков при работе с IB.

        // https://interactivebrokers.github.io/tws-api/tick_types.html
        public const int BID_SIZE = 0;
        public const int BID_PRICE = 1;
        public const int ASK_PRICE = 2;
        public const int ASK_SIZE = 3;
        public const int LAST_PRICE = 4;
        public const int LAST_SIZE = 5;
        public const int HIGH_PRICE_OF_DAY = 6;
        public const int LOW_PRICE_OF_DAY = 7;
        public const int VOLUME_OF_DAY = 8;
        public const int CLOSE_PRICE = 9;
        public const int BID_OPTION_PRICE = 10;
        public const int ASK_OPTION_PRICE = 11;
        public const int LAST_OPTION_PRICE = 12;
        public const int MODEL_OPTION = 13; //maybe this is theory price.
        public const int OPEN_TICK = 14; //Current session's opening price
        public const int DELAYED_BID_PRICE = 66;
        public const int DELAYED_ASK_PRICE = 67;
        public const int DELAYED_LAST_PRICE = 68;
        public const int DELAYED_BID_SIZE = 69;
        public const int DELAYED_ASK_SIZE = 70;
        public const int DELAYED_LAST_SIZE = 71;
        public const int DELAYED_BID_OPTION = 80;
        public const int DELAYED_ASK_OPTION = 81;
        public const int DELAYED_LAST_PRICE_OPTION = 82;
        public const int DELAYED_MODEL_OPTION = 83; //maybe this is theory price.

        #endregion
    }
}