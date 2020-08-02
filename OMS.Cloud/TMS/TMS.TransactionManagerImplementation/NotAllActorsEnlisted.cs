using System;

namespace TMS.TransactionManagerImplementation
{
    public class NotAllActorsEnlistedException : Exception
    {
        public NotAllActorsEnlistedException()
            : base()
        {
        }

        public NotAllActorsEnlistedException(string message)
            : base(message)
        {
        }

        public NotAllActorsEnlistedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
