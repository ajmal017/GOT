using System;

namespace GOT.Logic.Utils.Helpers
{
    public static class MathHelper
    {
        public static decimal RoundUp(decimal value, decimal step)
        {
            if (step == 0) {
                return 0;
            }

            var multiplicand = Math.Round(value / step);
            return multiplicand * step;
        }
    }
}