using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
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
        private NetworkModel networkModel;

        public GdaService(StatelessServiceContext context)
            : base(context)
        {
            try
            {
                _ = Config.GetInstance(Context);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService] Configuration initialized.");
                
                this.networkModel = new NetworkModel();
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService] Error: {e.Message}");
            }
        }

        protected async override Task OnOpenAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ////FOR DEBUGING IN AZURE DEPLOYMENT (time to atach to process)
            //await Task.Delay(60000);

            await base.OnOpenAsync(cancellationToken);
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

        protected async override Task RunAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await this.networkModel.InitializeNetworkModel();
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService] NetworkModel initialized.");
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService] Error: {e.Message}");
            }
        }
    }
}
