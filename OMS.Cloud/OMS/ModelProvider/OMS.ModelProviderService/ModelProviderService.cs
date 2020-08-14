using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Common.OmsContracts.ModelProvider;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts;
using OMS.Common.WcfClient.PubSub;
using OMS.ModelProviderImplementation;
using OMS.ModelProviderImplementation.ContractProviders;

namespace OMS.ModelProviderService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ModelProviderService : StatefulService
    {
        private readonly OutageModel outageModel;
        private readonly OutageModelReadAccessProvider outageModelReadAccessProvider;
        private readonly OutageModelUpdateAccessProvider outageModelUpdateAccessProvider;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private readonly IRegisterSubscriberContract registerSubscriberClient;

        public ModelProviderService(StatefulServiceContext context)
            : base(context)
        {
            this.outageModel = new OutageModel(this.StateManager, this.outageModelReadAccessProvider, this.outageModelUpdateAccessProvider);
            this.outageModelReadAccessProvider = new OutageModelReadAccessProvider(this.StateManager);
            this.outageModelUpdateAccessProvider = new OutageModelUpdateAccessProvider(this.StateManager);

            this.registerSubscriberClient = RegisterSubscriberClient.CreateClient();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
             {
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IOutageModelReadAccessContract>(context,
                                                                            this.outageModelReadAccessProvider,
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.OmsOutageManagementServiceModelReadAccessEndpoint);
                }, EndpointNames.OmsOutageManagementServiceModelReadAccessEndpoint),
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IOutageModelUpdateAccessContract>(context,
                                                                             this.outageModelUpdateAccessProvider,
                                                                             WcfUtility.CreateTcpListenerBinding(),
                                                                             EndpointNames.OmsOutageManagmenetServiceModelUpdateAccessEndpoint);
                }, EndpointNames.OmsOutageManagmenetServiceModelUpdateAccessEndpoint),
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<INotifySubscriberContract>(context,
                                                                             this.outageModel,
                                                                             WcfUtility.CreateTcpListenerBinding(),
                                                                             EndpointNames.PubSubNotifySubscriberEndpoint);
                }, EndpointNames.PubSubNotifySubscriberEndpoint)

            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {   
            try
			{
                await this.registerSubscriberClient.SubscribeToTopic(Topic.TOPOLOGY, MicroserviceNames.OmsModelProviderService);
                await this.registerSubscriberClient.SubscribeToTopic(Topic.OMS_MODEL, MicroserviceNames.OmsModelProviderService);
            }
            catch (Exception e)
			{
                Logger.LogError($"Subscribe to topic failed with error: {e.Message}");
			}
        }

    }
}
