using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.ServiceModel.Channels;

namespace Outage.Common.ServiceProxies
{
    public class WcfServiceFabricCommunicationClient<T> : ServicePartitionClient<WcfCommunicationClient<T>> where T : class, IService
    {
        public WcfServiceFabricCommunicationClient(ICommunicationClientFactory<WcfCommunicationClient<T>> communicationClientFactory,
                                                   Uri serviceUri,
                                                   ServicePartitionKey partitionKey = null,
                                                   TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default,
                                                   string listenerName = null,
                                                   OperationRetrySettings retrySettings = null)
            : base(communicationClientFactory, serviceUri, partitionKey, targetReplicaSelector, listenerName, retrySettings)
        {
        }

        public static WcfServiceFabricCommunicationClient<T> GetClient(Uri address)
        {
            var binding = WcfUtility.CreateTcpClientBinding();
            var partitionResolver = ServicePartitionResolver.GetDefault();
            var wcfClientFactory = new WcfCommunicationClientFactory<T>(binding, null, partitionResolver);
            var sfClient = new WcfServiceFabricCommunicationClient<T>(wcfClientFactory, address, ServicePartitionKey.Singleton); //ServicePartitionKey.Singleton
            return sfClient;
        }
    }
}
