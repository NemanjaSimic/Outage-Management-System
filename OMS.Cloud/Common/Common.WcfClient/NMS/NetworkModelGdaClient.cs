using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.NMS
{
    public class NetworkModelGdaClient : WcfSeviceFabricClientBase<INetworkModelGDAContract>, INetworkModelGDAContract
    {
        private static readonly string microserviceName = MicroserviceNames.NmsGdaService;
        private static readonly string listenerName = EndpointNames.NmsGdaEndpoint;

        public NetworkModelGdaClient(WcfCommunicationClientFactory<INetworkModelGDAContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition) 
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static INetworkModelGDAContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<NetworkModelGdaClient, INetworkModelGDAContract>(microserviceName);
        }

        public static INetworkModelGDAContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<NetworkModelGdaClient, INetworkModelGDAContract>(serviceUri, servicePartitionKey);
        }

        #region INetworkModelGDAContract
        public Task<UpdateResult> ApplyUpdate(Delta delta)
        {
            //return MethodWrapperAsync<UpdateResult>("ApplyUpdate", new object[1] { delta });
            return InvokeWithRetryAsync(client => client.Channel.ApplyUpdate(delta));
        }

        public Task<int> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
        {
            //return MethodWrapperAsync<int>("GetExtentValues", new object[2] { entityType, propIds });
            return InvokeWithRetryAsync(client => client.Channel.GetExtentValues(entityType, propIds));
        }

        public Task<int> GetRelatedValues(long source, List<ModelCode> propIds, Association association)
        {
            //return MethodWrapperAsync<int>("GetRelatedValues", new object[3] { source, propIds, association });
            return InvokeWithRetryAsync(client => client.Channel.GetRelatedValues(source, propIds, association));
        }

        public Task<ResourceDescription> GetValues(long resourceId, List<ModelCode> propIds)
        {
            //return MethodWrapperAsync<ResourceDescription>("GetValues", new object[2] { resourceId, propIds });
            return InvokeWithRetryAsync(client => client.Channel.GetValues(resourceId, propIds));
        }

        public Task<bool> IteratorClose(int id)
        {
            //return MethodWrapperAsync<bool>("IteratorClose", new object[1] { id });
            return InvokeWithRetryAsync(client => client.Channel.IteratorClose(id));
        }

        public Task<List<ResourceDescription>> IteratorNext(int n, int id)
        {
            //return MethodWrapperAsync<List<ResourceDescription>>("IteratorNext", new object[2] { n, id });
            return InvokeWithRetryAsync(client => client.Channel.IteratorNext(n, id));
        }

        public Task<int> IteratorResourcesLeft(int id)
        {
            //return MethodWrapperAsync<int>("IteratorResourcesLeft", new object[1] { id });
            return InvokeWithRetryAsync(client => client.Channel.IteratorResourcesLeft(id));
        }

        public Task<int> IteratorResourcesTotal(int id)
        {
            //return MethodWrapperAsync<int>("IteratorResourcesTotal", new object[1] { id });
            return InvokeWithRetryAsync(client => client.Channel.IteratorResourcesTotal(id));
        }

        public Task<bool> IteratorRewind(int id)
        {
            //return MethodWrapperAsync<bool>("IteratorRewind", new object[1] { id });
            return InvokeWithRetryAsync(client => client.Channel.IteratorRewind(id));
        }

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }
        #endregion
    }
}
