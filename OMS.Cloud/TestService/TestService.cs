using System;
using System.Collections.Generic;
using System.Fabric;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.PubSub;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.WcfClient.NMS;
using OMS.Common.WcfClient.PubSub;
using OMS.Common.WcfClient.SCADA;


namespace TestService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class TestService : StatelessService, INotifySubscriberContract
    {
        private readonly ICloudLogger logger;
        private readonly Uri subscriberUri;

        private readonly ReadCommandEnqueuerClient readCommandEnqueuerClient;
        private readonly WriteCommandEnqueuerClient writeCommandEnqueuerClient;
        private readonly ModelUpdateCommandEnqueuerClient modelUpdateCommandEnqueuerClient;
        private readonly ScadaModelReadAccessClient scadaModelReadAccessClient;
        private readonly ScadaModelUpdateAccessClient scadaModelUpdateAccessClient;
        private readonly ScadaIntegrityUpdateClient scadaIntegrityUpdateClient;
        private readonly ScadaCommandingClient scadaCommandingClient;
        private readonly NetworkModelGdaClient networkModelGdaClient;
        private readonly RegisterSubscriberClient registerSubscriberClient;
        private readonly PublisherClient publisherClient;

        public TestService(StatelessServiceContext context)
            : base(context)
        {
            logger = CloudLoggerFactory.GetLogger();
            subscriberUri = new Uri("fabric:/OMS.Cloud/TestService");

            this.readCommandEnqueuerClient = ReadCommandEnqueuerClient.CreateClient();
            this.writeCommandEnqueuerClient = WriteCommandEnqueuerClient.CreateClient();
            this.modelUpdateCommandEnqueuerClient = ModelUpdateCommandEnqueuerClient.CreateClient();

            this.scadaModelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
            this.scadaModelUpdateAccessClient = ScadaModelUpdateAccessClient.CreateClient();
            this.scadaIntegrityUpdateClient = ScadaIntegrityUpdateClient.CreateClient();

            this.scadaCommandingClient = ScadaCommandingClient.CreateClient();

            this.networkModelGdaClient = NetworkModelGdaClient.CreateClient();

            this.registerSubscriberClient = RegisterSubscriberClient.CreateClient();
            this.publisherClient = PublisherClient.CreateClient();
        }

        #region INotifySubscriberContract
        public async Task Notify(IPublishableMessage message)
        {
            if(message  is SingleAnalogValueSCADAMessage singleAnalog)
            {
                var analogData = singleAnalog.AnalogModbusData;
                string dataMessage = $"Gid: 0x{analogData.MeasurementGid:X16} | Value: {analogData.Value} | Alarm: {analogData.Alarm} | Origin: {analogData.CommandOrigin}";
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[TestService] Notify message single analog: {dataMessage}");
                logger.LogDebug(dataMessage);
            }
            else if (message is MultipleAnalogValueSCADAMessage multipleAnalog)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[TestService] Notify message multiple analog: ");

                foreach(var analogData in multipleAnalog.Data.Values)
                { 
                    sb.AppendLine($"Gid: 0x{analogData.MeasurementGid:X16} | Value: {analogData.Value} | Alarm: {analogData.Alarm} | Origin: {analogData.CommandOrigin}");
                }
                
                ServiceEventSource.Current.ServiceMessage(this.Context, sb.ToString());
                logger.LogDebug(sb.ToString());
            }
            else if (message is SingleDiscreteValueSCADAMessage singleDiscrete)
            {
                var discreteData = singleDiscrete.DiscreteModbusData;
                string dataMessage = $"Gid: 0x{discreteData.MeasurementGid:X16} | Value: {discreteData.Value} | Alarm: {discreteData.Alarm} | Origin: {discreteData.CommandOrigin}";
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[TestService] Notify message single discrete: {dataMessage}");
                logger.LogDebug(dataMessage);
            }
            else if (message is MultipleDiscreteValueSCADAMessage multipleDiscrete)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[TestService] Notify message multiple discrete: ");

                foreach (var discreteData in multipleDiscrete.Data.Values)
                {
                    sb.AppendLine($"Gid: 0x{discreteData.MeasurementGid:X16} | Value: {discreteData.Value} | Alarm: {discreteData.Alarm} | Origin: {discreteData.CommandOrigin}");
                }

                ServiceEventSource.Current.ServiceMessage(this.Context, sb.ToString());
                logger.LogDebug(sb.ToString());
            }
        }

        public async Task<Uri> GetSubscriberUri()
        {
            return subscriberUri;
        }
        #endregion

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            //return new ServiceInstanceListener[0];
            return new List<ServiceInstanceListener>()
            {
                //ScadaReadCommandEnqueuerEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<INotifySubscriberContract>(context,
                                                                                   this,
                                                                                   WcfUtility.CreateTcpListenerBinding(),
                                                                                   EndpointNames.NotifySubscriberEndpoint);
                }, EndpointNames.NotifySubscriberEndpoint),
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
                var subscriptions = await registerSubscriberClient.GetAllSubscribedTopics(subscriberUri);
                var result = await registerSubscriberClient.SubscribeToTopics(new List<Topic>() { Topic.MEASUREMENT, Topic.SWITCH_STATUS }, subscriberUri, ServiceType.STATELESS_SERVICE);
                subscriptions = await registerSubscriberClient.GetAllSubscribedTopics(subscriberUri);
            }
            catch (Exception e)
            {
            }

            //TEST PUBLISH
            //try
            //{
            //    var data = new AnalogModbusData(1, AlarmType.NO_ALARM, 0, CommandOriginType.OTHER_COMMAND);
            //    var publishableMessage = new SingleAnalogValueSCADAMessage(data);
            //    var publication = new ScadaPublication(Topic.MEASUREMENT, publishableMessage);
            //    await publisherClient.Publish(publication, subscriberUri);
            //}
            //catch (Exception e)
            //{
            //}
        }
    }
}
