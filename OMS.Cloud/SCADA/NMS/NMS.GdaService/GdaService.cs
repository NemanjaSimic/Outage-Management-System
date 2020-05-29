using System.Collections.Generic;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using NMS.GdaImplementation;
using NMS.GdaImplementation.GDA;
using OMS.Common.NmsContracts;
using Outage.Common;

namespace NMS.GdaService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class GdaService : StatelessService
    {
        private readonly NetworkModel networkModel;

        public GdaService(StatelessServiceContext context)
            : base(context)
        {
            _ = Config.GetInstance(context);
            this.networkModel = new NetworkModel();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            //eturn new ServiceInstanceListener[0];
            return new List<ServiceInstanceListener>()
            {
                //NetworkModelGDAEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<INetworkModelGDAContract>(context,
                                                                           new GenericDataAccess(networkModel),
                                                                           WcfUtility.CreateTcpListenerBinding(),
                                                                           EndpointNames.NetworkModelGDAEndpoint);
                }, EndpointNames.NetworkModelGDAEndpoint),

                ////NetworkModelTransactionActorEndpoint
                //new ServiceInstanceListener(context =>
                //{
                //    return new WcfCommunicationListener<ITransactionActorContract>(context,
                //                                                           new NMSTransactionActor(networkModel),
                //                                                           WcfUtility.CreateTcpListenerBinding(),
                //                                                           EndpointNames.NetworkModelTransactionActorEndpoint);
                //}, EndpointNames.NetworkModelTransactionActorEndpoint),
            };
        }
    }
}
