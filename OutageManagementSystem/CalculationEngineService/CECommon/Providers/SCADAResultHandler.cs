using CECommon.Interfaces;
using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
				Provider.Instance.CacheProvider.UpdateAnalogMeasurement(singleAnalog.Gid, singleAnalog.Value);
			}
			else if (message is MultipleAnalogValueSCADAMessage multipleAnalog)
			{
				Provider.Instance.CacheProvider.UpdateAnalogMeasurement(multipleAnalog.Data);
			}
			else if (message is SingleDiscreteValueSCADAMessage singleDiscrete)
			{
				Provider.Instance.CacheProvider.UpdateDiscreteMeasurement(singleDiscrete.Gid, singleDiscrete.Value);
			}
			else if (message is MultipleDiscreteValueSCADAMessage multipleDiscrete)
			{
				Provider.Instance.CacheProvider.UpdateDiscreteMeasurement(multipleDiscrete.Data);
			}
			else
			{
				logger.LogError($"Message has unsupported type [{message.GetType().ToString()}].");
			}
		}
	}
}