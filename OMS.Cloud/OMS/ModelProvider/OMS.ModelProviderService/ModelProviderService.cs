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
        private readonly IRegisterSubscriberContract registerSubscriberClient;

        public ModelProviderService(StatefulServiceContext context)
            : base(context)
        {
            this.outageModel = new OutageModel(this.StateManager);
            this.outageModelReadAccessProvider = new OutageModelReadAccessProvider(this.StateManager);
            this.outageModelUpdateAccessProvider = new OutageModelUpdateAccessProvider(this.StateManager);
            this.registerSubscriberClient = RegisterSubscriberClient.CreateClient();
            this.registerSubscriberClient.SubscribeToTopic(Topic.OMS_MODEL, MicroserviceNames.OmsModelProviderService);
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
                                                                            EndpointNames.OutageManagementServiceModelReadAccessEndpoint);
                }, EndpointNames.OutageManagementServiceModelReadAccessEndpoint),
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IOutageModelUpdateAccessContract>(context,
                                                                             this.outageModelUpdateAccessProvider,
                                                                             WcfUtility.CreateTcpListenerBinding(),
                                                                             EndpointNames.OutageManagmenetServiceModelUpdateAccessEndpoint);
                },EndpointNames.OutageManagmenetServiceModelUpdateAccessEndpoint),
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<INotifySubscriberContract>(context,
                                                                             this.outageModel,
                                                                             WcfUtility.CreateTcpListenerBinding(),
                                                                             EndpointNames.NotifySubscriberEndpoint);
                },EndpointNames.NotifySubscriberEndpoint)

            };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
