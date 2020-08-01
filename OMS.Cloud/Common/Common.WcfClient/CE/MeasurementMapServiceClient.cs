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
		private static readonly string microserviceName = MicroserviceNames.MeasurementProviderService;
		private static readonly string listenerName = EndpointNames.MeasurementMapEndpoint;

		public MeasurementMapServiceClient(WcfCommunicationClientFactory<IMeasurementMapContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static MeasurementMapServiceClient CreateClient()
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<MeasurementMapServiceClient, IMeasurementMapContract>(microserviceName);
		}

		public static MeasurementMapServiceClient CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<MeasurementMapServiceClient, IMeasurementMapContract>(serviceUri, servicePartitionKey);
		}


		public Task<Dictionary<long, List<long>>> GetElementToMeasurementMap()
		{
			return InvokeWithRetryAsync(client => client.Channel.GetElementToMeasurementMap());

		}

		public Task<List<long>> GetMeasurementsOfElement(long elementId)
		{
			return InvokeWithRetryAsync(client => client.Channel.GetMeasurementsOfElement(elementId));

		}

		public Task<Dictionary<long, long>> GetMeasurementToElementMap()
		{
			return InvokeWithRetryAsync(client => client.Channel.GetMeasurementToElementMap());
		}
	}
}
