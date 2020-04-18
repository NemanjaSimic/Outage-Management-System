using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using System;

namespace OMS.Common.Cloud.WcfServiceFabricClients
{
    public class WcfSeviceFabricClientBase<T> : ServicePartitionClient<WcfCommunicationClient<T>> where T : class, IService
    {
        public WcfSeviceFabricClientBase(WcfCommunicationClientFactory<T> clientFactory, Uri serviceName, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceName, servicePartition)
        {
        }
    }
}
