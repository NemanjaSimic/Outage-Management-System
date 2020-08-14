using Common.PubSub;
using Common.PubSubContracts.DataContracts.OMS;
using Common.Web.Mappers;
using OMS.Common.Cloud.Logger;
using OMS.Common.PubSubContracts;
using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
using WebAdapterImplementation.HubDispatchers;

namespace WebAdapterImplementation.NotificationSubscribers
{
    [DataContract]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class OutageNotification : INotifySubscriberContract
    {
        private readonly string baseLogString;

        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private readonly string _subscriberName;
        private OutageHubDispatcher _dispatcher;
        private readonly IOutageMapper _mapper;

        public OutageNotification(string subscriberName, IOutageMapper mapper)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            _subscriberName = subscriberName;
            _mapper = mapper;
        }

        public async Task Notify(IPublishableMessage message, string publisherName)
        {
            if (message is ActiveOutageMessage activeOutage)
            {
                _dispatcher = new OutageHubDispatcher(_mapper);
                _dispatcher.Connect();

                try
                {
                    _dispatcher.NotifyActiveOutageUpdate(activeOutage);
                }
                catch (Exception e)
                {
                    string errorMessage = $"{baseLogString} Notify => exception {e.Message}";
                    Logger.LogError(errorMessage, e);
                }
            }
            else if (message is ArchivedOutageMessage archivedOutage)
            {
                _dispatcher.Connect();

                try
                {
                    _dispatcher.NotifyArchiveOutageUpdate(archivedOutage);
                }
                catch (Exception e)
                {
                    string errorMessage = $"{baseLogString} Notify => exception {e.Message}";
                    Logger.LogError(errorMessage, e);
                }
            }
        }

        public async Task<string> GetSubscriberName()
        {
            return _subscriberName;
        }
    }
}
