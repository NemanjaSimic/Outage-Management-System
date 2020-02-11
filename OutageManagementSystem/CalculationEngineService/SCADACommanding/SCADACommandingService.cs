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
                    ProxyFactory proxyFactory = new ProxyFactory();

                    using (SCADACommandProxy proxy = proxyFactory.CreateProxy<SCADACommandProxy, ISCADACommand>(EndpointNames.SCADACommandService))
                    {
                        if (proxy == null)
                        {
                            string message = "SendDiscreteCommand => SCADACommandProxy is null.";
                            logger.LogError(message);
                            throw new NullReferenceException(message);
                        }

                        success = proxy.SendDiscreteCommand(gid, commandingValue);
                    }
                }
                else
                {
                    //todo: sucess = what?
                    Provider.Instance.CacheProvider.UpdateDiscreteMeasurement(gid, commandingValue);
                }
            }
            catch (Exception ex)
            {
                success = false;
                logger.LogError($"Sending discrete command for measurement with GID {gid} failed. Exception: {ex.Message}");
            }

            return success;
        }
    }
}
