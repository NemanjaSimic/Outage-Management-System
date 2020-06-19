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
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.NmsContracts;


namespace NMS.GdaService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class GdaService : StatelessService
    {
        private readonly ICloudLogger logger;

        private readonly NetworkModel networkModel;

        public GdaService(StatelessServiceContext context)
            : base(context)
        {
            logger = CloudLoggerFactory.GetLogger();

            try
            {
                _ = Config.GetInstance(this.Context);
                string message = "Configuration initialized.";
                logger.LogInformation(message);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService | Information] {message}");

                this.networkModel = new NetworkModel();
                message = "NetworkModel created.";
                logger.LogInformation(message);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService | Information] {message}");
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService | Error] {e.Message}");
            }
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
                string message = $"NetworkModel initialized.";
                logger.LogInformation(message);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService | Information] {message}");
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService | Error] {e.Message}");
            }
        }
    }
}
