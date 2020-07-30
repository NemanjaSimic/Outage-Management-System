using OMS.Web.Common.Mappers;
using OMS.Web.UI.Models.ViewModels;
using Outage.Common.PubSub;
using Outage.Common.PubSub.CalculationEngineDataContract;
using Outage.Common.ServiceContracts.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WebAdapter.HubDispatchers;

namespace WebAdapter.Topology
{
    [DataContract]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    class TopologyNotification : ISubscriberCallback
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

        public string GetSubscriberName() => _subscriberName;

        /// <summary>
        /// Maps the input topology object to a graph object and dispatches it  
        /// to a Graph Hub endpoint
        /// </summary>
        /// <param name="message"></param>
        public void Notify(IPublishableMessage message)
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
    }
}
