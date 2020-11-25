namespace GOT.SharedKernel.Enums
{
    public enum GotKeyCommand: byte
    {
        None,
        SingleStart,
        SingleStop,
        AllStart,
        AllStop,
        OpenOptionWindow,
        OpenStopStrategyWindow,
        SingleDelete,
        AllDelete,
        AddStrategy,
        AddSellStrategy,
        AddBuyStrategy,
        InfoStrategy,
        Save
    }
}