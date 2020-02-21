using Outage.Common.ServiceContracts.OMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceProxies.Outage
{
    public class OutageLifecycleProxy : BaseProxy<IOutageLifecycleContract>, IOutageLifecycleContract
    {
        public OutageLifecycleProxy(string endpointName)
           : base(endpointName)
        {
        }

        public bool IsolateOutage(long outageId)
        {
            throw new NotImplementedException();
        }

        public bool ReportOutage(long elementGid)
        {
            bool success;

            try
            {
                success = Channel.ReportOutage(elementGid);
            }
            catch (Exception e)
            {
                string message = "Exception in ReportOutage() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
        }
    }
}
