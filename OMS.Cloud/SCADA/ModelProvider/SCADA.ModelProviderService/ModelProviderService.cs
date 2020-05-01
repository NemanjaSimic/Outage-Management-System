using System.Collections.Generic;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.ScadaContracts;
using Outage.Common;
using SCADA.ModelProviderImplementation.ContractProviders;

namespace SCADA.ModelProviderService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ModelProviderService : StatefulService
    {
        public ModelProviderService(StatefulServiceContext context)
            : base(context)
        { }

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
            return new[]
            {
                //ScadaModelReadAccessEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IScadaModelReadAccessContract>(context,
                                                                            new ModelReadAccessProvider(this.StateManager),
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.ScadaModelReadAccessEndpoint);
                }, EndpointNames.ScadaModelReadAccessEndpoint),

                //ScadaModelUpdateAccessEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IScadaModelUpdateAccessContract>(context,
                                                                            new ModelUpdateAccessProvider(this.StateManager),
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.ScadaModelUpdateAccessEndpoint);
                }, EndpointNames.ScadaModelUpdateAccessEndpoint),

                //SCADAIntegrityUpdateEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IScadaIntegrityUpdateContract>(context,
                                                                           new IntegrityUpdateProvider(this.StateManager),
                                                                           WcfUtility.CreateTcpListenerBinding(),
                                                                           EndpointNames.SCADAIntegrityUpdateEndpoint);
                }, EndpointNames.SCADAIntegrityUpdateEndpoint),
            };
        }
    }
}
