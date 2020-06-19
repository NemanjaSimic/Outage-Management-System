using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.ScadaContracts.ModelProvider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.SCADA
{
    public class ScadaIntegrityUpdateClient : WcfSeviceFabricClientBase<IScadaIntegrityUpdateContract>, IScadaIntegrityUpdateContract
    {
        private static readonly string microserviceName = MicroserviceNames.ScadaModelProviderService;
        private static readonly string listenerName = EndpointNames.ScadaIntegrityUpdateEndpoint;

        public ScadaIntegrityUpdateClient(WcfCommunicationClientFactory<IScadaIntegrityUpdateContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static ScadaIntegrityUpdateClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = new ServicePartitionKey(0);

            if (serviceUri == null)
            {
                return factory.CreateClient<ScadaIntegrityUpdateClient, IScadaIntegrityUpdateContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<ScadaIntegrityUpdateClient, IScadaIntegrityUpdateContract>(serviceUri, servicePartition);
            }
        }

        #region IScadaIntegrityUpdateContract
        public Task<Dictionary<Topic, ScadaPublication>> GetIntegrityUpdate()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetIntegrityUpdate());
        }

        public Task<ScadaPublication> GetIntegrityUpdateForSpecificTopic(Topic topic)
        {
            return InvokeWithRetryAsync(client => client.Channel.GetIntegrityUpdateForSpecificTopic(topic));
        }
        #endregion
    }
}
