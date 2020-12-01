namespace GOT.Logic.DTO
{
    public class InstrumentDTO
    {
        public int Id { get; set; }
        public decimal LastPrice { get; set; }
        public decimal Ask { get; set; }
        public decimal Bid { get; set; }
        public decimal TheoreticalPrice { get; set; }
    }
}