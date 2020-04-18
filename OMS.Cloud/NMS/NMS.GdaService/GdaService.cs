using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Cloud.NMS.GdaProvider;
using OMS.Cloud.NMS.GdaProvider.DistributedTransaction;
using OMS.Cloud.NMS.GdaProvider.GDA;
using OMS.Common.Cloud.WcfServiceFabricClients;
using OMS.Common.DistributedTransactionContracts;
using OMS.Common.NmsContracts;
using Outage.Common;

namespace OMS.Cloud.NMS.GdaService
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
            Config.GetInstance(context);
            networkModel = new NetworkModel();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>()
            {
                //NetworkModelGDAEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<INetworkModelGDAContract>(context,
                                                                           new GenericDataAccess(networkModel),
                                                                           TcpBindingHelper.CreateListenerBinding(),
                                                                           EndpointNames.NetworkModelGDAEndpoint);
                }, EndpointNames.NetworkModelGDAEndpoint),

                //NetworkModelTransactionActorEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<ITransactionActorContract>(context,
                                                                           new NMSTransactionActor(networkModel),
                                                                           TcpBindingHelper.CreateListenerBinding(),
                                                                           EndpointNames.NetworkModelTransactionActorEndpoint);
                }, EndpointNames.NetworkModelTransactionActorEndpoint),
            };
        }
    }
}
