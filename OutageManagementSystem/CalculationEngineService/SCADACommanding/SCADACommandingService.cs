using CECommon.Providers;
using Outage.Common;
using Outage.Common.ServiceContracts.CalculationEngine;
using Outage.Common.ServiceContracts.SCADA;
using Outage.Common.ServiceProxies;
using System;

namespace SCADAFunctions
{
    public class SCADACommandingService : ISwitchStatusCommandingContract
    {
        private ILogger logger = LoggerWrapper.Instance;
        public bool SendAnalogCommand(long gid, float commandingValue)
        {
            bool success = false;
            //Imamo li analog komandu ????
            return success;
        }

        public void SendCommand(long guid, int value)
        {
            try
            {
                if (Provider.Instance.TopologyProvider.IsElementRemote(Provider.Instance.MeasurementProvider.GetElementGidForMeasurement(guid)))
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
                        proxy.SendDiscreteCommand(guid, (ushort)value);
                    }
                }
                else
                {
                    Provider.Instance.MeasurementProvider.UpdateDiscreteMeasurement(guid, (ushort)value);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Sending discrete command for measurement with GID 0x{guid.ToString("X16")} failed. Exception: {ex.Message}");
            }
        }
    }
}
