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
using OMS.Common.NmsContracts;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.ScadaContracts.Commanding;
using OMS.Common.ScadaContracts.FunctionExecutior;
using OMS.Common.ScadaContracts.ModelProvider;
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
        private readonly string baseLogString;
        private readonly IReadCommandEnqueuerContract readCommandEnqueuerClient;
        private readonly IWriteCommandEnqueuerContract writeCommandEnqueuerClient;
        private readonly IModelUpdateCommandEnqueuerContract modelUpdateCommandEnqueuerClient;
        private readonly IScadaModelReadAccessContract scadaModelReadAccessClient;
        private readonly IScadaModelUpdateAccessContract scadaModelUpdateAccessClient;
        private readonly IScadaIntegrityUpdateContract scadaIntegrityUpdateClient;
        private readonly IScadaCommandingContract scadaCommandingClient;
        private readonly INetworkModelGDAContract networkModelGdaClient;
        private readonly IRegisterSubscriberContract registerSubscriberClient;
        private readonly IPublisherContract publisherClient;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public TestService(StatelessServiceContext context)
            : base(context)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

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
        public async Task Notify(IPublishableMessage message, string publisherName)
        {
            if(message  is SingleAnalogValueSCADAMessage singleAnalog)
            {
                var analogData = singleAnalog.AnalogModbusData;
                string dataMessage = $"Gid: 0x{analogData.MeasurementGid:X16} | Value: {analogData.Value} | Alarm: {analogData.Alarm} | Origin: {analogData.CommandOrigin}";
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[TestService] Notify message single analog: {dataMessage}");
                Logger.LogDebug(dataMessage);
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
                Logger.LogDebug(sb.ToString());
            }
            else if (message is SingleDiscreteValueSCADAMessage singleDiscrete)
            {
                var discreteData = singleDiscrete.DiscreteModbusData;
                string dataMessage = $"Gid: 0x{discreteData.MeasurementGid:X16} | Value: {discreteData.Value} | Alarm: {discreteData.Alarm} | Origin: {discreteData.CommandOrigin}";
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[TestService] Notify message single discrete: {dataMessage}");
                Logger.LogDebug(dataMessage);
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
                Logger.LogDebug(sb.ToString());
            }
        }

        public async Task<string> GetSubscriberName()
        {
            return "TestService";
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
                                                                                   EndpointNames.PubSubNotifySubscriberEndpoint);
                }, EndpointNames.PubSubNotifySubscriberEndpoint),
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
                var subscriptions = await registerSubscriberClient.GetAllSubscribedTopics("TestService");
                var result = await registerSubscriberClient.SubscribeToTopics(new List<Topic>() { Topic.MEASUREMENT, Topic.SWITCH_STATUS }, MicroserviceNames.TestService);
                subscriptions = await registerSubscriberClient.GetAllSubscribedTopics(MicroserviceNames.TestService);
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
            //    await publisherClient.Publish(publication, MicroserviceNames.TestService);
            //}
            //catch (Exception e)
            //{
            //}
        }
    }
}
