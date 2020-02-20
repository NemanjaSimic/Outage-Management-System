using Outage.Common.ServiceContracts.OMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceProxies.Outage
{
    public class OutageSimulatorServiceProxy : BaseProxy<IOutageSimulatorContract>, IOutageSimulatorContract
    {
        public OutageSimulatorServiceProxy(string endpointName)
            : base(endpointName)
        {
        }

        public bool IsOutageElement(long outageElementId)
        {
            bool result;
            try
            {
                result = Channel.IsOutageElement(outageElementId);
            }
            catch (Exception e)
            {
                string message = "Exception in IsOutageElement() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
        }

        public bool ResolvedOutage(long outageElementId)
        {
            bool resolved;
            try
            {
                resolved = Channel.ResolvedOutage(outageElementId);
            }
            catch (Exception e)
            {
                string message = "Exception in ResolvedOutage() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return resolved;
        }
    }
}
