using Common.CeContracts;
using Common.PubSub;
using Common.Web.Mappers;
using Common.Web.Models.ViewModels;
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
    class TopologyNotification : INotifySubscriberContract
    {
        private readonly string _subscriberName;
        private readonly IGraphMapper _mapper;
        private readonly GraphHubDispatcher _dispatcher;

        public TopologyNotification(string subscriberName, IGraphMapper mapper)
        {
            _subscriberName = subscriberName;
            _mapper = mapper;

            _dispatcher = new GraphHubDispatcher();
        }

        public async Task Notify(IPublishableMessage message, string publisherName)
        {
            if (message is TopologyForUIMessage topologyMessage)
            {
                OmsGraphViewModel graph = _mapper.Map(topologyMessage.UIModel);

                _dispatcher.Connect();
                try
                {
                    _dispatcher.NotifyGraphUpdate(graph.Nodes, graph.Relations);
                }
                catch (Exception)
                {
                    // TODO: log error/retry
                }

            }
        }

        public async Task<string> GetSubscriberName()
        {
            return _subscriberName;
        }
    }
}
