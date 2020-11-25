using System;
using System.Globalization;
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

        public Option(int id) : base(id)
        {
            InstrumentType = InstrumentTypes.Options;
        }

        public Option(Contract contract) : this(contract.ConId)
        {
            Strike = Convert.ToDecimal(contract.Strike);
            Currency = contract.Currency;
            Exchange = contract.Exchange;
            FullName = contract.LocalSymbol;
            Code = contract.Symbol;
            Symbol = contract.Symbol;
            Multiplier = decimal.Parse(contract.Multiplier);
            ExpirationDate = DateTime.ParseExact(contract.LastTradeDateOrContractMonth, "yyyyMMdd",
                CultureInfo.CurrentCulture);
            MonthNumber = ExpirationDate.Month;
            OptionType = contract.Right switch
            {
                "P" => OptionTypes.Put,
                "C" => OptionTypes.Call,
                _ => OptionType
            };
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

        /// <summary>
        ///     Номер месяца, от 1 до 12 соответственно.
        /// </summary>
        [JsonProperty("monthNumber")]
        public int MonthNumber { get; set; }

        public override Contract CreateNewIbContract()
        {
            return new Contract
            {
                ConId = Id,
                Strike = (double) Strike,
                Currency = Currency,
                Exchange = Exchange,
                LocalSymbol = FullName,
                Symbol = Symbol,
                SecType = "FOP",
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