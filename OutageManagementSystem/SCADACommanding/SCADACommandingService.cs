using Outage.Common;
using Outage.Common.ServiceContracts.SCADA;
using Outage.Common.ServiceProxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADACommanding
{
    public class SCADACommandingService : ISCADACommand
    {
        private ILogger logger = LoggerWrapper.Instance;
        public bool SendAnalogCommand(long gid, float commandingValue)
        {
            bool success = false;
            if (SCADACommandingCache.Instance.TryGetMeasurementOfElement(gid, out long measurementId))
            {
                try
                {
                    using (var proxy = new SCADACommandProxy(EndpointNames.SCADACommandService))
                    {
                        proxy.SendAnalogCommand(measurementId, commandingValue);
                        success = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"Sending analog command for measurement with GID {measurementId} failed. Exception: {ex.Message}");
                }
            }
            else
            {
                logger.LogError($"Failed to get measurement for element with GID {gid}.");
            }
            return success;
        }

        public bool SendDiscreteCommand(long gid, ushort commandingValue)
        {
            bool success = false;
            if (SCADACommandingCache.Instance.TryGetMeasurementOfElement(gid, out long measurementId))
            {
                try
                {
                    using (var proxy = new SCADACommandProxy(EndpointNames.SCADACommandService))
                    {
                        proxy.SendDiscreteCommand(measurementId, commandingValue);
                        success = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"Sending discrete command for measurement with GID {measurementId} failed. Exception: {ex.Message}");
                }
            }
            else
            {
                logger.LogError($"Failed to get measurement for element with GID {gid}.");
            }
            return success;
        }
    }
}
