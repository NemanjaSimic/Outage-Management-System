using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Fabric;

namespace OMS.Common.Cloud.WcfServiceFabricClients
{
    internal class ClientFactory
    {
        public TClient CreateClient<TClient, TContract>(string serviceNameSetting) where TContract : class, IService 
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

            var partitionResolver = new ServicePartitionResolver(() =>
            {
                FabricClient foo = new FabricClient();
                return foo;
            });

            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<TContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);

            return (TClient)Activator.CreateInstance(typeof(TClient), new object[] { factory, serviceUri });
        }

        public TClient CreateClient<TClient, TContract>(Uri serviceUri) where TContract : class, IService
                                                                        where TClient : WcfSeviceFabricClientBase<TContract>
        {
            //var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<TContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);

            return (TClient)Activator.CreateInstance(typeof(TClient), new object[] { factory, serviceUri });
        }
    }
}
