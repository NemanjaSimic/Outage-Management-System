using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using OMS.Common.Cloud.Names;
using Common.Contracts.WebAdapterContracts;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using WebAdapterImplementation;
using OMS.Common.PubSubContracts;
using OMS.Common.WcfClient.PubSub;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;

namespace WebAdapterService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class WebAdapterService : StatelessService
    {
        private readonly string baseLogString;

        private readonly WebAdapterProvider webAdapterProvider;
        private readonly NotifySubscriberProvider notifySubscriberProvider;
        private readonly IRegisterSubscriberContract registerSubscriberClient;

        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public WebAdapterService(StatelessServiceContext context)
            : base(context)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            try
            {
                this.webAdapterProvider = new WebAdapterProvider();
                this.notifySubscriberProvider = new NotifySubscriberProvider(MicroserviceNames.WebAdapterService);

                this.registerSubscriberClient = RegisterSubscriberClient.CreateClient();
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} Ctor => exception {e.Message}";
                Logger.LogError(errorMessage, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[WebAdapterService | Error] {errorMessage}");
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>()
            {
                new ServiceInstanceListener(context =>
                {
                     return new WcfCommunicationListener<IWebAdapterContract>(context,
                                                                               this.webAdapterProvider,
                                                                               WcfUtility.CreateTcpListenerBinding(),
                                                                               EndpointNames.WebAdapterEndpoint);
                }, EndpointNames.WebAdapterEndpoint),

                new ServiceInstanceListener(context =>
                {
                     return new WcfCommunicationListener<INotifySubscriberContract>(context,
                                                                               this.notifySubscriberProvider,
                                                                               WcfUtility.CreateTcpListenerBinding(),
                                                                               EndpointNames.PubSubNotifySubscriberEndpoint);
                }, EndpointNames.PubSubNotifySubscriberEndpoint)
            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            //TEST Subscribe
            try
            {
                var topics = new List<Topic>()
                {
                    Topic.MEASUREMENT,
                    Topic.SWITCH_STATUS,
                    Topic.ACTIVE_OUTAGE,
                    Topic.ARCHIVED_OUTAGE,
                    Topic.TOPOLOGY,
                };

                var result = await registerSubscriberClient.SubscribeToTopics(topics, MicroserviceNames.WebAdapterService);
                var subscriptions = await registerSubscriberClient.GetAllSubscribedTopics(MicroserviceNames.WebAdapterService);
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} RynAsync => exception {e.Message}";
                Logger.LogError(errorMessage, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[WebAdapterService | Error] {errorMessage}");
            }
        }
    }
}
