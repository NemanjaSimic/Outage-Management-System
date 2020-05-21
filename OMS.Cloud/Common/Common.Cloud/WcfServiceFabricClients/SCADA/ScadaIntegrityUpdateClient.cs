using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.ScadaContracts.ModelProvider;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{
    public class ScadaIntegrityUpdateClient : WcfSeviceFabricClientBase<IScadaIntegrityUpdateContract>, IScadaIntegrityUpdateContract
    {
        public ScadaIntegrityUpdateClient(WcfCommunicationClientFactory<IScadaIntegrityUpdateContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition)
        {
        }

        public static ScadaIntegrityUpdateClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = new ServicePartitionKey(0);

            if (serviceUri == null)
            {
                return factory.CreateClient<ScadaIntegrityUpdateClient, IScadaIntegrityUpdateContract>(MicroserviceNames.ScadaModelProviderService, servicePartition);
            }
            else
            {
                return factory.CreateClient<ScadaIntegrityUpdateClient, IScadaIntegrityUpdateContract>(serviceUri, servicePartition);
            }
        }

        #region IScadaIntegrityUpdateContract
        public Task<Dictionary<Topic, SCADAPublication>> GetIntegrityUpdate()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetIntegrityUpdate());
        }

        public Task<SCADAPublication> GetIntegrityUpdateForSpecificTopic(Topic topic)
        {
            return InvokeWithRetryAsync(client => client.Channel.GetIntegrityUpdateForSpecificTopic(topic));
        }
        #endregion
    }
}
