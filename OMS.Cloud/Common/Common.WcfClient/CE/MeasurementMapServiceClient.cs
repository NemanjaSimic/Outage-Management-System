using Common.CeContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.CE
{
	public class MeasurementMapServiceClient : WcfSeviceFabricClientBase<IMeasurementMapContract>, IMeasurementMapContract
	{
		private static readonly string microserviceName = MicroserviceNames.MeasurementMapService;
		private static readonly string listenerName = EndpointNames.MeasurementMapEndpoint;

		public MeasurementMapServiceClient(WcfCommunicationClientFactory<IMeasurementMapContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static MeasurementMapServiceClient CreateClient(Uri serviceUri = null)
		{
			ClientFactory factory = new ClientFactory();
			ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

			if (serviceUri == null)
			{
				return factory.CreateClient<MeasurementMapServiceClient, IMeasurementMapContract>(microserviceName, servicePartition);
			}
			else
			{
				return factory.CreateClient<MeasurementMapServiceClient, IMeasurementMapContract>(serviceUri, servicePartition);
			}
		}

		public Task<Dictionary<long, List<long>>> GetElementToMeasurementMap()
		{
			return MethodWrapperAsync<Dictionary<long, List<long>>>("GetElementToMeasurementMap", new object[0]);

		}

		public Task<List<long>> GetMeasurementsOfElement(long elementId)
		{
			return MethodWrapperAsync<List<long>>("GetMeasurementsOfElement", new object[1] { elementId});

		}

		public Task<Dictionary<long, long>> GetMeasurementToElementMap()
		{
			return MethodWrapperAsync<Dictionary<long, long>>("GetMeasurementToElementMap", new object[0]);
		}
	}
}
