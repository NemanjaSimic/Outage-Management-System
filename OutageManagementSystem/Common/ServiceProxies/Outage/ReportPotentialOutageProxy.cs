using Outage.Common.ServiceContracts.OMS;
using System;

namespace Outage.Common.ServiceProxies.Outage
{
    public class ReportPotentialOutageProxy : BaseProxy<IReportPotentialOutageContract>, IReportPotentialOutageContract
    {
        public ReportPotentialOutageProxy(string endpointName)
           : base(endpointName)
        {
        }

        public bool ReportPotentialOutage(long elementGid)
        {
            bool success;

            try
            {
                success = Channel.ReportPotentialOutage(elementGid);
            }
            catch (Exception e)
            {
                string message = "Exception in ReportPotentialOutage() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
        }
    }
}
