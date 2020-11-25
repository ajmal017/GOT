using System;
using System.Diagnostics;
using System.Globalization;
using CQG;
using GOT.Logic.Enums;
using IBApi;
using Newtonsoft.Json;

namespace GOT.Logic.Models.Instruments
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Future : Instrument
    {
        public Future()
        {
            InstrumentType = InstrumentTypes.Futures;
        }

        private Future(int id) : base(id)
        {
            InstrumentType = InstrumentTypes.Futures;
        }

        public Future(Contract contract, string longName = "") : this(contract.ConId)
        {
            Exchange = contract.Exchange;
            Currency = contract.Currency;
            FullName = contract.LocalSymbol;
            Code = contract.Symbol;
            Symbol = contract.Symbol;
            Multiplier = decimal.Parse(contract.Multiplier);
            Description = longName;
            ExpirationDate = DateTime.ParseExact(contract.LastTradeDateOrContractMonth, "yyyyMMdd",
                CultureInfo.CurrentCulture);
        }

        public Future(CQGInstrument cqgInstrument) : this(cqgInstrument.InstrumentID)
        {
            try {
                FullName = cqgInstrument.FullName;
                Code = cqgInstrument.Commodity;
                Description = cqgInstrument.Description;
                ExpirationDate = cqgInstrument.ExpirationDate;
                Currency = cqgInstrument.Currency;
                Exchange = cqgInstrument.ExchangeAbbreviation;
                PriceStep = (decimal) cqgInstrument.TickSize;
            }
            catch (Exception e) {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        ///     For IB using as LocalSymbol.
        /// </summary>
        public override string FullName { get; set; }

        public override Contract CreateNewIbContract()
        {
            return new Contract
            {
                ConId = Id,
                Currency = Currency,
                Exchange = Exchange,
                LocalSymbol = FullName,
                Symbol = Symbol,
                SecType = "FUT"
            };
        }

        public Future CreateNewInstance()
        {
            return new Future(Id)
            {
                Code = Code,
                FullName = FullName,
                Symbol = Symbol,
                PriceStep = PriceStep,
                Multiplier = Multiplier,
                Exchange = Exchange,
                Currency = Currency
            };
        }
    }
}