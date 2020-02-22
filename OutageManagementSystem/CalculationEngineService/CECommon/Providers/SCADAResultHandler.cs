using CECommon.Interfaces;
using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.PubSub.SCADADataContract;
using System.Collections.Generic;

namespace CECommon.Providers
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
			logger.LogDebug($"Message recived from PubSub with type {message.GetType().ToString()}.");

			if (message is SingleAnalogValueSCADAMessage singleAnalog)
			{
				Provider.Instance.MeasurementProvider.UpdateAnalogMeasurement(singleAnalog.Gid, singleAnalog.Value);
			}
			else if (message is MultipleAnalogValueSCADAMessage multipleAnalog)
			{
				Provider.Instance.MeasurementProvider.UpdateAnalogMeasurement(multipleAnalog.Data);
			}
			else if (message is SingleDiscreteValueSCADAMessage singleDiscrete)
			{
				Dictionary<long, DiscreteModbusData> data = new Dictionary<long, DiscreteModbusData>(1) { { singleDiscrete.Gid, new DiscreteModbusData(singleDiscrete.Value, singleDiscrete.Alarm) } };
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