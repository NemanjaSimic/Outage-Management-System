using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Cloud.SCADA.ModelProviderService.ContractProviders;
using OMS.Common.Cloud.WcfServiceFabricClients;
using OMS.Common.ScadaContracts;
using Outage.Common;
using Outage.Common.ServiceContracts.DistributedTransaction;

namespace OMS.Cloud.SCADA.ModelProviderService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ModelProviderService : StatefulService
    {
        public ModelProviderService(StatefulServiceContext context)
            : base(context)
        {
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
            return new List<ServiceReplicaListener>()
            {
                //SCADAIntegrityUpdateEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IScadaIntegrityUpdateContract>(context,
                                                                           new IntegrityUpdateProvider(),
                                                                           TcpBindingHelper.CreateListenerBinding(),
                                                                           EndpointNames.SCADAIntegrityUpdateEndpoint);
                }, EndpointNames.SCADAIntegrityUpdateEndpoint),

                //SCADAModelAccessEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IScadaModelAccessContract>(context,
                                                                           new ModelAccessProvider(),
                                                                           TcpBindingHelper.CreateListenerBinding(),
                                                                           EndpointNames.SCADAModelAccessEndpoint);
                }, EndpointNames.SCADAModelAccessEndpoint),

                //ScadaModelTransactionActorEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<ITransactionActorContract>(context,
                                                                           new NMSTransactionActor(),
                                                                           TcpBindingHelper.CreateListenerBinding(),
                                                                           EndpointNames.SCADATransactionActorEndpoint);
                }, EndpointNames.SCADATransactionActorEndpoint),
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

        private async Task InitializeReliableCollections()
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("ScadaModel");
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("ScadaConfig");
        }
    }
}
