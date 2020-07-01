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

        public void OnSwitchClose(long elementGid)
        {
            try
            {
               Channel.OnSwitchClose(elementGid);
            }
            catch (Exception e)
            {
                string message = "Exception in ReportPotentialOutage() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }
        }

        public bool ReportPotentialOutage(long elementGid, CommandOriginType commandOriginType)
        {
            bool success;

            try
            {
                success = Channel.ReportPotentialOutage(elementGid, commandOriginType);
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
