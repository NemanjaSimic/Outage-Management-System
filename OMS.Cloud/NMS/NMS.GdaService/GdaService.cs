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
        private readonly string baseLogString;
        private readonly NetworkModel networkModel;
        private readonly INetworkModelGDAContract genericDataAccess;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public GdaService(StatelessServiceContext context)
            : base(context)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            try
            {
                //LOGIC
                _ = Config.GetInstance(this.Context);
                
                string debugMessage = $"{baseLogString} Ctor => Configuration initialized.";
                Logger.LogDebug(debugMessage);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService | Debug] {debugMessage}");

                //LOGIC
                this.networkModel = new NetworkModel();

                string infoMessage = $"{baseLogString} Ctor => NetworkModel created.";
                Logger.LogInformation(infoMessage);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService | Information] {infoMessage}");

                //LOGIC
                this.genericDataAccess = new GenericDataAccess(networkModel);

                infoMessage = $"{baseLogString} Ctor => Contract providers initialized.";
                Logger.LogInformation(infoMessage);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService | Information] {infoMessage}");
            }
            catch (Exception e)
            {
                string errMessage = $"{baseLogString} Ctor => Exception caught: {e.Message}.";
                Logger.LogError(errMessage, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService | Error] {errMessage}");
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
                                                                                  this.genericDataAccess,
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
                //LOGIC
                await this.networkModel.InitializeNetworkModel();

                string infoMessage = $"{baseLogString} RunAsync => NetworkModel initialized.";
                Logger.LogInformation(infoMessage);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService | Information] {infoMessage}");
            }
            catch (Exception e)
            {
                string errMessage = $"{baseLogString} RunAsync => Exception caught: {e.Message}.";
                Logger.LogError(errMessage, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[GdaService | Error] {errMessage}");
            }
        }
    }
}
