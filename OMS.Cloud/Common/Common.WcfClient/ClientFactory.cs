using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;

namespace OMS.Common.WcfClient
{
    internal class ClientFactory
    {
        private readonly string baseLogString;

        public ClientFactory()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
        }

        public TContract CreateClient<TClient, TContract>(string serviceName) where TContract : class, IService
                                                                            where TClient : WcfSeviceFabricClientBase<TContract>, TContract
        {
            var serviceNameToServiceUri = ServiceDefines.Instance.ServiceNameToServiceUri;
            var serviceNameToServiceType = ServiceDefines.Instance.ServiceNameToServiceType;

            if (serviceNameToServiceUri.ContainsKey(serviceName) == false || serviceNameToServiceType.ContainsKey(serviceName) == false)
            {
                throw new Exception($"{baseLogString} CreateClient({typeof(TClient)}) => serviceName '{serviceName}' not found in {typeof(ServiceDefines)} collections. Found in ServiceNameToServiceUri: {serviceNameToServiceUri.ContainsKey(serviceName)}. Found in ServiceNameToServiceType: {serviceNameToServiceType.ContainsKey(serviceName)}.");
            }

            ServicePartitionKey servicePartition;
            var serviceType = serviceNameToServiceType[serviceName];

            //SPECIAL CASE
            if (serviceType == ServiceType.STANDALONE_SERVICE)
            {
                var binding = new NetTcpBinding();
                var externalEndpoint = new EndpointAddress(serviceNameToServiceUri[serviceName]);

                var factory = new ChannelFactory<TContract>(binding, externalEndpoint);
                return factory.CreateChannel();
            }

            if (serviceType == ServiceType.STATEFUL_SERVICE)
            {
                servicePartition = new ServicePartitionKey(0);
            }
            else if (serviceType == ServiceType.STATELESS_SERVICE)
            {
                servicePartition = ServicePartitionKey.Singleton;
            }
            else
            {
                throw new Exception($"{baseLogString} CreateClient({typeof(TClient)}) => UNKNOWN value of ServiceType. Value: {serviceType}");
            }

            return CreateClient<TClient, TContract>(serviceNameToServiceUri[serviceName], servicePartition);
        }

        public TClient CreateClient<TClient, TContract>(string serviceNameSetting, ServicePartitionKey servicePartition) where TContract : class, IService 
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

        public TClient CreateClient<TClient, TContract>(Uri serviceUri, ServicePartitionKey servicePartition) where TContract : class, IService
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
