using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.PubSub;
using Outage.Common.PubSub.CalculationEngineDataContract;

namespace OMS.Web.Adapter.Topology
{
    using System;
    using System.ServiceModel;
    using OMS.Web.Common.Mappers;
    using System.Runtime.Serialization;
    using OMS.Web.UI.Models.ViewModels;
    using OMS.Web.Adapter.HubDispatchers;
    
    [DataContract]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class TopologyNotification : ISubscriberCallback
    {
        // TODO: Add logging
        private readonly string _subscriberName;
        private readonly IGraphMapper _mapper;
        private readonly GraphHubDispatcher _dispatcher;

        public TopologyNotification(string subscriberName, IGraphMapper mapper)
        {
            _subscriberName = subscriberName;
            _mapper = mapper;

            _dispatcher = new GraphHubDispatcher();
        }

        public string GetSubscriberName() => _subscriberName;

        /// <summary>
        /// Maps the input topology object to a graph object and dispatches it  
        /// to a Graph Hub endpoint
        /// </summary>
        /// <param name="message"></param>
        public void Notify(IPublishableMessage message)
        {
            if(message is TopologyForUIMessage topologyMessage)
            {
                OmsGraphViewModel graph = _mapper.Map(topologyMessage.UIModel);

                _dispatcher.Connect();
                try
                {
                    _dispatcher.NotifyGraphUpdate(graph.Nodes, graph.Relations);
                }
                catch (Exception)
                {
                    // retry ?
                }

            }
        }
    }
}
