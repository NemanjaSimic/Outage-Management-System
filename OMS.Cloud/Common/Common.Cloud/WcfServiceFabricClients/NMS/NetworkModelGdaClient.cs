using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.GDA;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Fabric;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.NMS
{
    public class NetworkModelGdaClient : WcfSeviceFabricClientBase<INetworkModelGDAContract>, INetworkModelGDAContract
    {
        public NetworkModelGdaClient(WcfCommunicationClientFactory<INetworkModelGDAContract> clientFactory, Uri serviceUri) 
            : base(clientFactory, serviceUri)
        {
        }

        public static NetworkModelGdaClient CreateClient(Uri serviceUri = null)
        {
            if (serviceUri == null && ConfigurationManager.AppSettings[MicroserviceNames.NmsGdaService] is string nmsGdaServiceName)
            {
                serviceUri = new Uri(nmsGdaServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<INetworkModelGDAContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);
            
            return new NetworkModelGdaClient(factory, serviceUri);
        }

        #region INetworkModelGDAContract
        public Task<UpdateResult> ApplyUpdate(Delta delta)
        {
            return InvokeWithRetryAsync(client => client.Channel.ApplyUpdate(delta));
        }

        public Task<int> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
        {
            return InvokeWithRetryAsync(client => client.Channel.GetExtentValues(entityType, propIds));
        }

        public Task<int> GetRelatedValues(long source, List<ModelCode> propIds, Association association)
        {
            return InvokeWithRetryAsync(client => client.Channel.GetRelatedValues(source, propIds, association));
        }

        public Task<ResourceDescription> GetValues(long resourceId, List<ModelCode> propIds)
        {
            return InvokeWithRetryAsync(client => client.Channel.GetValues(resourceId, propIds));
        }

        public Task<bool> IteratorClose(int id)
        {
            return InvokeWithRetryAsync(client => client.Channel.IteratorClose(id));
        }

        public Task<List<ResourceDescription>> IteratorNext(int n, int id)
        {
            return InvokeWithRetryAsync(client => client.Channel.IteratorNext(n, id));
        }

        public Task<int> IteratorResourcesLeft(int id)
        {
            return InvokeWithRetryAsync(client => client.Channel.IteratorResourcesLeft(id));
        }

        public Task<int> IteratorResourcesTotal(int id)
        {
            return InvokeWithRetryAsync(client => client.Channel.IteratorResourcesTotal(id));
        }

        public Task<bool> IteratorRewind(int id)
        {
            return InvokeWithRetryAsync(client => client.Channel.IteratorRewind(id));
        }
        #endregion
    }
}
