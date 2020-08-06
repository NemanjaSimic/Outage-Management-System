using Common.PubSub;
using Common.PubSubContracts.DataContracts.OMS;
using Common.Web.Mappers;
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
        private readonly string _subscriberName;
        private OutageHubDispatcher _dispatcher;
        private readonly IOutageMapper _mapper;

        public OutageNotification(string subscriberName, IOutageMapper mapper)
        {
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
                catch (Exception)
                {
                    // TODO: log error
                }
            }
            else if (message is ArchivedOutageMessage archivedOutage)
            {
                _dispatcher.Connect();

                try
                {
                    _dispatcher.NotifyArchiveOutageUpdate(archivedOutage);
                }
                catch (Exception)
                {
                    // TODO: log error
                }
            }
        }

        public async Task<string> GetSubscriberName()
        {
            return _subscriberName;
        }
    }
}
