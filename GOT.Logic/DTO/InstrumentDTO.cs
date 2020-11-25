namespace GOT.Logic.DataTransferObjects
{
    public class InstrumentDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public decimal PriceStep { get; set; }
        public decimal LastPrice { get; set; }
        public decimal Ask { get; set; }
        public decimal Bid { get; set; }
        public decimal TheoreticalPrice { get; set; }
    }
}