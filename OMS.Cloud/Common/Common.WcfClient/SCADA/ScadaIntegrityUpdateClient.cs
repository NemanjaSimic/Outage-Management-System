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

        public static ScadaIntegrityUpdateClient CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<ScadaIntegrityUpdateClient, IScadaIntegrityUpdateContract>(microserviceName);
        }

        public static ScadaIntegrityUpdateClient CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<ScadaIntegrityUpdateClient, IScadaIntegrityUpdateContract>(serviceUri, servicePartitionKey);
        }

        #region IScadaIntegrityUpdateContract
        public Task<Dictionary<Topic, ScadaPublication>> GetIntegrityUpdate()
        {
            //return MethodWrapperAsync<Dictionary<Topic, ScadaPublication>>("GetIntegrityUpdate", new object[0]);
            return InvokeWithRetryAsync(client => client.Channel.GetIntegrityUpdate());
        }

        public Task<ScadaPublication> GetIntegrityUpdateForSpecificTopic(Topic topic)
        {
            //return MethodWrapperAsync<ScadaPublication>("GetIntegrityUpdateForSpecificTopic", new object[1] { topic });
            return InvokeWithRetryAsync(client => client.Channel.GetIntegrityUpdateForSpecificTopic(topic));
        }
        #endregion
    }
}
