using System;

namespace GOT.SharedKernel.Utils.Exceptions
{
    public class StrategyException : Exception
    {
        public StrategyException()
        {
        }

        public StrategyException(string message)
            : base(message)
        {
        }

        public StrategyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}