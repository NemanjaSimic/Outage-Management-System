using CECommon.Interfaces;
using CECommon.Model;
using CECommon.Providers;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.ServiceContracts.SCADA;
using Outage.Common.ServiceProxies;
using System;
using System.Collections.Generic;

namespace CalculationEngine.SCADAFunctions
{
    public class SCADACommanding : ISCADACommanding
    {
        private ILogger logger = LoggerWrapper.Instance;

        public void SendAnalogCommand(long measurementGid, float commandingValue, CommandOriginType commandOrigin)
        {
            //Imamo li analog komandu ????
            throw new NotImplementedException("CalculationEngine.SCADAFunctions.SendAnalogCommand");
        }

        public void SendDiscreteCommand(long measurementGid, int value, CommandOriginType commandOrigin)
        {
            try
            {
                if (Provider.Instance.MeasurementProvider.TryGetDiscreteMeasurement(measurementGid, out DiscreteMeasurement measurement) && !(measurement is ArtificalDiscreteMeasurement))
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

                        proxy.SendDiscreteCommand(measurementGid, (ushort)value, commandOrigin);
                    }
                }
                else
                {
                    //TOOD: DiscreteModbusData prilikom prijema sa skade prepakovati u model podataka koji ce se cuvati na CE, u prilogy AlarmType.NO_ALARM, nije validna stvar, navodi se da se 
                    //DiscreteModbusData data = new DiscreteModbusData((ushort)value, AlarmType.NO_ALARM, measurementGid, commandOrigin);
                    Dictionary<long, DiscreteModbusData> data = new Dictionary<long, DiscreteModbusData>(1)
                    {
                        { measurementGid, new DiscreteModbusData((ushort)value, AlarmType.NO_ALARM, measurementGid, commandOrigin) } 
				    };
                    Provider.Instance.MeasurementProvider.UpdateDiscreteMeasurement(data);
                    //Provider.Instance.MeasurementProvider.UpdateDiscreteMeasurement(data.MeasurementGid, data.Value, data.CommandOrigin);

                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Sending discrete command for measurement with GID 0x{measurementGid.ToString("X16")} failed. Exception: {ex.Message}");
            }
        }
    }
}
