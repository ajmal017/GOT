using CQG;
using GOT.Logic.Enums;
using IBApi;
using Newtonsoft.Json;

namespace GOT.Logic.Models.Instruments
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Option : Instrument
    {
        public Option()
        {
            InstrumentType = InstrumentTypes.Options;
        }

        public Option(int id)
        {
            TickerId = id;
            InstrumentType = InstrumentTypes.Options;
        }

        public Option(CQGInstrument cqgInstrument) : base(cqgInstrument.InstrumentID)
        {
            var contract = cqgInstrument;
            Strike = contract.Strike;
            MonthNumber = contract.Month;
            switch (contract.InstrumentType) {
                case eInstrumentType.itOptionPut:
                    OptionType = OptionTypes.Put;
                    InstrumentType = InstrumentTypes.OptionPut;
                    break;
                case eInstrumentType.itOptionCall:
                    OptionType = OptionTypes.Call;
                    InstrumentType = InstrumentTypes.OptionCall;
                    break;
            }
        }

        [JsonProperty("optionType")]
        public OptionTypes? OptionType { get; set; }

        [JsonProperty("strike")]
        public decimal Strike { get; set; }
        
        [JsonProperty("tradingClass")]
        public string TradingClass { get; set; }

        public override Contract CreateNewIbContract()
        {
            return new Contract
            {
                Strike = (double) Strike,
                Currency = Currency,
                Exchange = Exchange,
                TradingClass = TradingClass,
                Symbol = Symbol,
                SecType = "FOP",
                Multiplier = Multiplier.ToString(),
                LastTradeDateOrContractMonth = ExpirationDate.ToString("yyyyMMdd"),
                Right = OptionType switch
                {
                    OptionTypes.Put => "P",
                    OptionTypes.Call => "C",
                    _ => ""
                }
            };
        }
        
        public override string ToString()
        {
            return OptionType + " " + base.ToString();
        }
    }
}