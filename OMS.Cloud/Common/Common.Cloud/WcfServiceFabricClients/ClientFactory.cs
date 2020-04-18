using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Fabric;
using System.Threading;

namespace OMS.Common.Cloud.WcfServiceFabricClients
{
    internal class ClientFactory
    {
        public TClient CreateClient<TClient, TContract>(string serviceNameSetting, ServicePartitionKey servicePartition = null) where TContract : class, IService 
                                                                                                                                where TClient : WcfSeviceFabricClientBase<TContract>, TContract
        {
            Uri serviceUri;
            
            if (ConfigurationManager.AppSettings[serviceNameSetting] is string serviceName)
            {
                serviceUri = new Uri(serviceName);
            }
            else
            {
                throw new KeyNotFoundException($"Key '{serviceNameSetting}' not found.");
            }

            return CreateClient<TClient, TContract>(serviceUri, servicePartition);
        }

        public TClient CreateClient<TClient, TContract>(Uri serviceUri, ServicePartitionKey servicePartition = null) where TContract : class, IService
                                                                                                                     where TClient : WcfSeviceFabricClientBase<TContract>
        {
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());

            //if (serviceUri.ToString() == "fabric:/OMS.Cloud/SCADA.ModelProviderService")
            //{
            //    long partitionKey = 0;
            //    var partition = partitionResolver.ResolveAsync(serviceUri, new ServicePartitionKey(partitionKey), new CancellationToken()).Result;
            //    var endpoints = partition.Endpoints;
            //}

            var factory = new WcfCommunicationClientFactory<TContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);
            return (TClient)Activator.CreateInstance(typeof(TClient), new object[] { factory, serviceUri, servicePartition });
        }
    }
}
