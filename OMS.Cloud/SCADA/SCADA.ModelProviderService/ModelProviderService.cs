using System.Collections.Generic;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Cloud.SCADA.ModelProviderService.ContractProviders;
using OMS.Cloud.SCADA.ModelProviderService.DistributedTransaction;
using OMS.Common.Cloud.WcfServiceFabricClients;
using OMS.Common.DistributedTransactionContracts;
using OMS.Common.ScadaContracts;
using Outage.Common;

namespace OMS.Cloud.SCADA.ModelProviderService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ModelProviderService : StatefulService
    {
        private readonly ScadaModel scadaModel;

        public ModelProviderService(StatefulServiceContext context)
            : base(context)
        {
            scadaModel = new ScadaModel(this.StateManager, new ModelResourcesDesc(), new EnumDescs());
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
            //return new ServiceReplicaListener[0];
            return new List<ServiceReplicaListener>()
            {
                //SCADAIntegrityUpdateEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IScadaIntegrityUpdateContract>(context,
                                                                           new IntegrityUpdateProvider(this.StateManager),
                                                                           TcpBindingHelper.CreateListenerBinding(),
                                                                           EndpointNames.SCADAIntegrityUpdateEndpoint);
                }, EndpointNames.SCADAIntegrityUpdateEndpoint),
            
                //ScadaModelReadAccessEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IScadaModelReadAccessContract>(context,
                                                                            new ModelReadAccessProvider(this.StateManager),
                                                                            TcpBindingHelper.CreateListenerBinding(),
                                                                            EndpointNames.ScadaModelReadAccessEndpoint);
                }, EndpointNames.ScadaModelReadAccessEndpoint),
            
                //ScadaModelUpdateAccessEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IScadaModelUpdateAccessContract>(context,
                                                                            new ModelUpdateAccessProvider(this.StateManager),
                                                                            TcpBindingHelper.CreateListenerBinding(),
                                                                            EndpointNames.ScadaModelUpdateAccessEndpoint);
                }, EndpointNames.ScadaModelUpdateAccessEndpoint),
            
                //SCADAModelUpdateNotifierEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IModelUpdateNotificationContract>(context,
                                                                            new ScadaModelUpdateNotification(scadaModel),
                                                                            TcpBindingHelper.CreateListenerBinding(),
                                                                            EndpointNames.SCADAModelUpdateNotifierEndpoint);
                }, EndpointNames.SCADAModelUpdateNotifierEndpoint),
            
                //SCADATransactionActorEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<ITransactionActorContract>(context,
                                                                            new ScadaTransactionActor(scadaModel),
                                                                            TcpBindingHelper.CreateListenerBinding(),
                                                                            EndpointNames.SCADATransactionActorEndpoint);
                }, EndpointNames.SCADATransactionActorEndpoint),
            };
        }
    }
}
