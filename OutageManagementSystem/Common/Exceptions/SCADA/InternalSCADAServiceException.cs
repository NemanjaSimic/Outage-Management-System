using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.Exceptions.SCADA
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
