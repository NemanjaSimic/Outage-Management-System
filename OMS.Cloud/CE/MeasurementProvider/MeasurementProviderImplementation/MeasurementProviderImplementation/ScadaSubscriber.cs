using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.PubSubContracts.Interfaces;
using OMS.Common.WcfClient.CE;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CE.MeasurementProviderImplementation
{
	public class ScadaSubscriber : INotifySubscriberContract
	{
		private readonly string baseLogString;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		public ScadaSubscriber()
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering Ctor.";
			Logger.LogVerbose(verboseMessage);

			string debugMessage = $"{baseLogString} Ctor => Clients initialized.";
			Logger.LogDebug(debugMessage);
		}

		public Task<string> GetSubscriberName()
		{
			return Task.Run(() => { return MicroserviceNames.CeMeasurementProviderService; });
		}

		public Task<bool> IsAlive()
		{
			return Task.Run(() => { return true; });
		}

		public async Task Notify(IPublishableMessage message, string publisherName)
		{
			Logger.LogDebug($"{baseLogString} Notify method invoked.");

			if (message is SingleAnalogValueSCADAMessage singleAnalog)
			{
				Dictionary<long, AnalogModbusData> data = new Dictionary<long, AnalogModbusData>(1)
				{
					{ singleAnalog.AnalogModbusData.MeasurementGid, singleAnalog.AnalogModbusData },
				};

				Logger.LogDebug($"{baseLogString} Calling Update analog measurement from measurement provider.");
				var measurementProviderClient = MeasurementProviderClient.CreateClient();
				await measurementProviderClient.UpdateAnalogMeasurement(data);
			}
			else if (message is MultipleAnalogValueSCADAMessage multipleAnalog)
			{
				Logger.LogDebug($"{baseLogString} Calling Update analog measurement from measurement provider.");
				var measurementProviderClient = MeasurementProviderClient.CreateClient();
				await measurementProviderClient.UpdateAnalogMeasurement(multipleAnalog.Data);
			}
			else if (message is SingleDiscreteValueSCADAMessage singleDiscrete)
			{
				Dictionary<long, DiscreteModbusData> data = new Dictionary<long, DiscreteModbusData>(1)
				{
					{ singleDiscrete.DiscreteModbusData.MeasurementGid, singleDiscrete.DiscreteModbusData },
				};

				Logger.LogDebug($"{baseLogString} Calling Update discrete measurement from measurement provider.");
				var measurementProviderClient = MeasurementProviderClient.CreateClient();
				await measurementProviderClient.UpdateDiscreteMeasurement(data);
			}
			else if (message is MultipleDiscreteValueSCADAMessage multipleDiscrete)
			{
				Logger.LogDebug($"{baseLogString} Calling Update discrete measurement from measurement provider.");
				var measurementProviderClient = MeasurementProviderClient.CreateClient();
				await measurementProviderClient.UpdateDiscreteMeasurement(multipleDiscrete.Data);
			}
			else
			{
				Logger.LogError($"{baseLogString} ERROR Message has unsupported type [{message.GetType().ToString()}].");
			}
		}
	}
}
