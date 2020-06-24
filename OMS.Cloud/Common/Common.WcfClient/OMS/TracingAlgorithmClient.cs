using Common.OmsContracts.TracingAlgorithm;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using OMS.Common.ScadaContracts.ModelProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS
{
    public class TracingAlgorithmClient : WcfSeviceFabricClientBase<ITracingAlgorithmContract>, ITracingAlgorithmContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsTracingAlgorithmService;
        private static readonly string listenerName = EndpointNames.TracingAlgorithmEndpoint;
        public TracingAlgorithmClient(WcfCommunicationClientFactory<ITracingAlgorithmContract> clientFactory,Uri serviceUri,ServicePartitionKey servicePartition)
            :base(clientFactory,serviceUri,servicePartition,listenerName)
        {

        }

        public static TracingAlgorithmClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

            if (serviceUri == null)
            {
                return factory.CreateClient<TracingAlgorithmClient, ITracingAlgorithmContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<TracingAlgorithmClient, ITracingAlgorithmContract>(serviceUri, servicePartition);
            }
        }
        #region ITracingAlgorithmContract
        public Task StartTracingAlgorithm(List<long> calls)
        {
            return InvokeWithRetryAsync(client => client.Channel.StartTracingAlgorithm(calls));
        }

        #endregion
    }
}
