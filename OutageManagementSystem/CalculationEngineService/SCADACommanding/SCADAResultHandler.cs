using CECommon.Interfaces;
using CECommon.Providers;
using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.PubSub.SCADADataContract;
using System.Collections.Generic;

namespace CalculationEngine.SCADAFunctions
{
	public class SCADAResultHandler : ISCADAResultHandler
	{
		private ILogger logger = LoggerWrapper.Instance;
		public SCADAResultHandler()
		{
			Provider.Instance.SCADAResultHandler = this;
		}

		public void HandleResult(IPublishableMessage message)
		{
			if (message is SingleAnalogValueSCADAMessage singleAnalog)
			{
				Provider.Instance.MeasurementProvider.UpdateAnalogMeasurement(singleAnalog.AnalogModbusData.MeasurementGid, singleAnalog.AnalogModbusData.Value, singleAnalog.AnalogModbusData.CommandOrigin);
			}
			else if (message is MultipleAnalogValueSCADAMessage multipleAnalog)
			{
				Provider.Instance.MeasurementProvider.UpdateAnalogMeasurement(multipleAnalog.Data);
			}
			else if (message is SingleDiscreteValueSCADAMessage singleDiscrete)
			{
				Dictionary<long, DiscreteModbusData> data = new Dictionary<long, DiscreteModbusData>(1) 
				{ 
					{ singleDiscrete.DiscreteModbusData.MeasurementGid, singleDiscrete.DiscreteModbusData }, //new DiscreteModbusData(singleDiscrete.DiscreteModbusData.Value, singleDiscrete.DiscreteModbusData.Alarm, singleDiscrete.DiscreteModbusData.MeasurementGid, singleDiscrete.DiscreteModbusData.CommandOrigin) } 
				};
				Provider.Instance.MeasurementProvider.UpdateDiscreteMeasurement(data);
			}
			else if (message is MultipleDiscreteValueSCADAMessage multipleDiscrete)
			{
				Provider.Instance.MeasurementProvider.UpdateDiscreteMeasurement(multipleDiscrete.Data);
			}
			else
			{
				logger.LogError($"Message has unsupported type [{message.GetType().ToString()}].");
			}
		}
	}
}