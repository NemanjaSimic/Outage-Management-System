using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Fabric;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient
{
    internal class ClientFactory
    {
        private readonly string baseLogString;
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        public ClientFactory()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
        }

        public TContract CreateClient<TClient, TContract>(string serviceName) where TContract : class, IService, IHealthChecker
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

        public TContract CreateClient<TClient, TContract>(string serviceNameSetting, ServicePartitionKey servicePartition) where TContract : class, IService , IHealthChecker
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

        public TContract CreateClient<TClient, TContract>(Uri serviceUri, ServicePartitionKey servicePartition) where TContract : class, IService, IHealthChecker
                                                                                                              where TClient : WcfSeviceFabricClientBase<TContract>
        {
            var binding = WcfUtility.CreateTcpClientBinding();
            var partitionResolver = ServicePartitionResolver.GetDefault();
            var wcfClientFactory = new WcfCommunicationClientFactory<TContract>(clientBinding: binding,
                                                                                servicePartitionResolver: partitionResolver);

            TContract result = (TContract)Activator.CreateInstance(typeof(TClient), new object[] { wcfClientFactory, serviceUri, servicePartition });

            int counter = 1;
            while (true)
            {
                try
                {
                    result.IsAlive();
                    Logger.LogDebug($"{baseLogString} Returning client for service uri: {serviceUri}.");
                    return result;
                }
                catch (FabricServiceNotFoundException)
                {
                    Logger.LogDebug($"{baseLogString} FabricServiceNotFoundException, service uri: {serviceUri}, number of tries: {counter}.");
                    Task.Delay(200);
                    if (++counter > 10)
                    {
                        throw;
                    }
                    continue;
                }
                catch(Exception)
                {
                    throw;
                }
            }
        }
    }
}
