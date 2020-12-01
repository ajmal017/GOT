using System;
using GOT.Logic.Enums;
using GOT.SharedKernel;
using IBApi;
using Newtonsoft.Json;

namespace GOT.Logic.Models.Instruments
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Instrument : Entity
    {
        public Instrument()
        {
        }

        public Instrument(int id)
        {
            Id = id;
        }

        [JsonProperty("code")]
        public virtual string Code { get; set; }

        [JsonProperty("fullName")]
        public virtual string FullName { get; set; }

        [JsonProperty("description")]
        public virtual string Description { get; set; }

        [JsonProperty("exchange")]
        public virtual string Exchange { get; set; }

        [JsonProperty("currency")]
        public virtual string Currency { get; set; }

        [JsonProperty("multiplier")]
        public virtual decimal Multiplier { get; set; }

        private DateTime _expirationDate;

        [JsonProperty("expirationDate")]
        public virtual DateTime ExpirationDate
        {
            get => _expirationDate;
            set
            {
                _expirationDate = value;
                MonthNumber = ExpirationDate.Month;
            }
        }
        
        /// <summary>
        ///     Номер месяца, от 1 до 12 соответственно.
        /// </summary>
        [JsonProperty("monthNumber")]
        public int MonthNumber { get; set; }

        [JsonProperty("symbol")]
        public virtual string Symbol { get; set; }

        [JsonProperty("tickerId")]
        public int TickerId { get; set; }

        [JsonProperty("priceStep")]
        public virtual decimal PriceStep { get; set; }

        public virtual decimal LastPrice { get; set; }

        public virtual decimal Ask { get; set; }

        public virtual decimal Bid { get; set; }

        public InstrumentTypes InstrumentType { get; protected set; }

        public virtual Contract CreateNewIbContract()
        {
            return new Contract();
        }

        public override string ToString()
        {
            return InstrumentType + " " + FullName + " " + Currency + " " + Exchange;
        }
    }
}