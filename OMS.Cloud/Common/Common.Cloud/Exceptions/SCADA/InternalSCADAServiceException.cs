using System;
using System.Runtime.Serialization;

namespace OMS.Common.Cloud.Exceptions.SCADA
{
    [DataContract]
    public class InternalSCADAServiceException : Exception
    {
        public InternalSCADAServiceException()
            : base()
        {
        }

        public InternalSCADAServiceException(string message)
            : base(message)
        {
        }

        public InternalSCADAServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
