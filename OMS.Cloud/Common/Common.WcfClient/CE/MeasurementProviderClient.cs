using CECommon.Model;
using Common.CeContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.CE
{
	public class MeasurementProviderClient : WcfSeviceFabricClientBase<IMeasurementProviderService>, IMeasurementProviderService
	{
		private static readonly string microserviceName = MicroserviceNames.MeasurementProviderService;
		private static readonly string listenerName = EndpointNames.MeasurementProviderEndpoint;
		public MeasurementProviderClient(WcfCommunicationClientFactory<IMeasurementProviderService> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static MeasurementProviderClient CreateClient(Uri serviceUri = null)
		{
			ClientFactory factory = new ClientFactory();
			ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

			if (serviceUri == null)
			{
				return factory.CreateClient<MeasurementProviderClient, IMeasurementProviderService>(microserviceName, servicePartition);
			}
			else
			{
				return factory.CreateClient<MeasurementProviderClient, IMeasurementProviderService>(serviceUri, servicePartition);
			}
		}
		public Task AddAnalogMeasurement(AnalogMeasurement analogMeasurement)
		{
			return MethodWrapperAsync("AddAnalogMeasurement", new object[1] { analogMeasurement} );
		}

		public Task AddDiscreteMeasurement(DiscreteMeasurement discreteMeasurement)
		{
			return MethodWrapperAsync("AddDiscreteMeasurement", new object[1] { discreteMeasurement });
		}

		public Task AddMeasurementElementPair(long measurementId, long elementId)
		{
			return MethodWrapperAsync("AddMeasurementElementPair", new object[2] { measurementId, elementId });
		}

		public Task CommitTransaction()
		{
			return MethodWrapperAsync("CommitTransaction", new object[0]);
		}

		public Task<float> GetAnalogValue(long measurementGid)
		{
			return MethodWrapperAsync<float>("GetAnalogValue", new object[1] { measurementGid });
		}

		public Task<bool> GetDiscreteValue(long measurementGid)
		{
			return MethodWrapperAsync<bool>("GetDiscreteValue", new object[1] { measurementGid });
		}

		public Task<long> GetElementGidForMeasurement(long measurementGid)
		{
			return MethodWrapperAsync<long>("GetElementGidForMeasurement", new object[1] { measurementGid });
		}

		public Task<Dictionary<long, List<long>>> GetElementToMeasurementMap()
		{
			return MethodWrapperAsync<Dictionary<long, List<long>>>("GetElementToMeasurementMap", new object[0]);
		}

		public Task<List<long>> GetMeasurementsOfElement(long elementGid)
		{
			return MethodWrapperAsync<List<long>>("GetMeasurementsOfElement", new object[1] { elementGid });
		}

		public Task<Dictionary<long, long>> GetMeasurementToElementMap()
		{
			return MethodWrapperAsync<Dictionary<long, long>>("GetMeasurementToElementMap", new object[0]);
		}

		public Task<bool> PrepareForTransaction()
		{
			return MethodWrapperAsync<bool>("PrepareForTransaction", new object[0]);
		}

		public Task RollbackTransaction()
		{
			return MethodWrapperAsync("RollbackTransaction", new object[0]);
		}

		public Task<AnalogMeasurement> GetAnalogMeasurement(long measurementGid)
		{
			return MethodWrapperAsync<AnalogMeasurement>("GetAnalogMeasurement", new object[1] { measurementGid });
		}

		public Task<DiscreteMeasurement> GetDiscreteMeasurement(long measurementGid)
		{
			return MethodWrapperAsync<DiscreteMeasurement>("GetDiscreteMeasurement", new object[1] { measurementGid });
		}

		public Task UpdateAnalogMeasurement(Dictionary<long, AnalogModbusData> data)
		{
			return MethodWrapperAsync("UpdateAnalogMeasurement", new object[1] { data });
		}

		public Task UpdateDiscreteMeasurement(Dictionary<long, DiscreteModbusData> data)
		{
			return MethodWrapperAsync("UpdateDiscreteMeasurement", new object[1] { data });
		}

		public Task SendAnalogCommand(long measurementGid, float commandingValue, CommandOriginType commandOrigin)
		{
			return MethodWrapperAsync("SendAnalogCommand", new object[3] { measurementGid, commandingValue, commandOrigin });
		}

		public Task SendDiscreteCommand(long measurementGid, int value, CommandOriginType commandOrigin)
		{
			return MethodWrapperAsync("SendAnalogCommand", new object[3] { measurementGid, value, commandOrigin });
		}
	}
}
