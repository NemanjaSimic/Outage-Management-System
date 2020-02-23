using Outage.Common.ServiceContracts.OMS;
using System;

namespace Outage.Common.ServiceProxies.Outage
{
    public class CallingServiceProxy : BaseProxy<ICallingContract>, ICallingContract
    {
        public CallingServiceProxy(string endpointName)
            : base(endpointName)
        {
        }

        public void ReportMalfunction(long consumerGid)
        {
            try
            {
                Channel.ReportMalfunction(consumerGid);
            }
            catch (Exception e)
            {
                string message = "Exception in ReportMalfunction() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }
        }
    }
}