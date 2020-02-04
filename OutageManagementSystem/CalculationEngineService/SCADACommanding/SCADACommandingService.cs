using CECommon.Providers;
using Outage.Common;
using Outage.Common.ServiceContracts.SCADA;
using Outage.Common.ServiceProxies;
using System;

namespace SCADACommanding
{
    public class SCADACommandingService : ISCADACommand
    {
        private ILogger logger = LoggerWrapper.Instance;
        public bool SendAnalogCommand(long gid, float commandingValue)
        {
            bool success = false;
            //Imamo li analog komandu ????
            return success;
        }

        public bool SendDiscreteCommand(long gid, ushort commandingValue)
        {
            bool success = false;
            try
            {
                if (Provider.Instance.TopologyProvider.IsElementRemote(Provider.Instance.CacheProvider.GetElementGidForMeasurement(gid)))
                {
                    using (var proxy = new SCADACommandProxy(EndpointNames.SCADACommandService))
                    {
                        proxy.SendDiscreteCommand(gid, commandingValue);
                        success = true;
                    }
                }
                else
                {
                    Provider.Instance.CacheProvider.UpdateDiscreteMeasurement(gid, commandingValue);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Sending discrete command for measurement with GID {gid} failed. Exception: {ex.Message}");
            }
            return success;
        }
    }
}
