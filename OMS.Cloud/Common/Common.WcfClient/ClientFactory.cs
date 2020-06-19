using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace OMS.Common.WcfClient
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
                throw new KeyNotFoundException($"Key '{serviceNameSetting}' not found in appSettings.");
            }

            return CreateClient<TClient, TContract>(serviceUri, servicePartition);
        }

        public TClient CreateClient<TClient, TContract>(Uri serviceUri, ServicePartitionKey servicePartition = null) where TContract : class, IService
                                                                                                                     where TClient : WcfSeviceFabricClientBase<TContract>
        {
            var binding = WcfUtility.CreateTcpClientBinding();
            var partitionResolver = ServicePartitionResolver.GetDefault();
            var wcfClientFactory = new WcfCommunicationClientFactory<TContract>(clientBinding: binding,
                                                                                servicePartitionResolver: partitionResolver);

            return (TClient)Activator.CreateInstance(typeof(TClient), new object[] { wcfClientFactory, serviceUri, servicePartition });
        }
    }
}
