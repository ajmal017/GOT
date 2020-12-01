using System;
using GOT.Logic.DTO;

namespace GOT.Logic.Connectors.InteractiveBrokers
{
    public class IbCodeHandler
    {
        public InstrumentDTO ConvertToInstrumentDTO(int ticketId, int code, double value)
        {
            var instrumentDto = new InstrumentDTO {Id = ticketId};

            switch (code) {
                case IbCodes.ASK_PRICE:
                case IbCodes.ASK_OPTION_PRICE:
                case IbCodes.DELAYED_ASK_PRICE:
                case IbCodes.DELAYED_ASK_OPTION:
                    instrumentDto.Ask = ConvertDoubleToDecimal(value);
                    break;
                case IbCodes.BID_PRICE:
                case IbCodes.BID_OPTION_PRICE:
                case IbCodes.DELAYED_BID_PRICE:
                case IbCodes.DELAYED_BID_OPTION:
                    instrumentDto.Bid = ConvertDoubleToDecimal(value);
                    break;
                case IbCodes.LAST_PRICE:
                case IbCodes.LAST_OPTION_PRICE:
                case IbCodes.DELAYED_LAST_PRICE:
                case IbCodes.DELAYED_LAST_PRICE_OPTION:
                    instrumentDto.LastPrice = ConvertDoubleToDecimal(value);
                    break;
                case IbCodes.MODEL_OPTION:
                case IbCodes.DELAYED_MODEL_OPTION:
                    instrumentDto.TheoreticalPrice = ConvertDoubleToDecimal(value);
                    break;
            }

            return instrumentDto;
        }

        private static decimal ConvertDoubleToDecimal(double value)
        {
            var newValue = 0m;
            try {
                newValue = (decimal) value;
            }
            catch (OverflowException) {
                newValue = 0;
            }

            return newValue;
        }
    }
}