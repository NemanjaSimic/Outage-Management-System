using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.ScadaContracts;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Fabric;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{
    class ScadaIntegrityUpdateClient : WcfSeviceFabricClientBase<IScadaIntegrityUpdateContract>, IScadaIntegrityUpdateContract
    {
        public ScadaIntegrityUpdateClient(WcfCommunicationClientFactory<IScadaIntegrityUpdateContract> clientFactory, Uri serviceUri)
            : base(clientFactory, serviceUri)
        {
        }

        public static ScadaIntegrityUpdateClient CreateClient(Uri serviceUri = null)
        {
            if (serviceUri == null && ConfigurationManager.AppSettings[MicroserviceNames.ScadaModelProviderService] is string scadaModelProviderServiceName)
            {
                serviceUri = new Uri(scadaModelProviderServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<IScadaIntegrityUpdateContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);

            return new ScadaIntegrityUpdateClient(factory, serviceUri);
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
