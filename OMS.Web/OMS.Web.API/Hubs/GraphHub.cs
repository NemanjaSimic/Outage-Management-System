using Outage.Common;
using OMS.Web.Common;
using System.Threading.Tasks;
using OMS.Web.Common.Mappers;
using Microsoft.AspNet.SignalR;
using OMS.Web.Adapter.Topology;
using System.Collections.Generic;
using OMS.Web.UI.Models.ViewModels;
using Microsoft.AspNet.SignalR.Hubs;
using Outage.Common.ServiceProxies.PubSub;

namespace OMS.Web.API.Hubs
{
    [HubName("graphhub")]   
    public class GraphHub : Hub
    {
        private static IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<GraphHub>();
        // todo: di
        private SubscriberProxy _subscriberClient = new SubscriberProxy(
            new TopologyNotification("WEB_SUBSCRIBER", new GraphMapper()),
            AppSettings.Get<string>("pubSubServiceAddress")
            );

        public void NotifyGraphUpdate(List<Node> nodes, List<Relation> relations)
        {
            Clients.All.updateGraph(new OmsGraph { Nodes = nodes, Relations = relations });
        }

        public void Join()
        {
            Groups.Add(Context.ConnectionId, "Users");
        }

        public override Task OnConnected()
        {
            Groups.Add(Context.ConnectionId, "Users");
            _subscriberClient.Subscribe(Topic.TOPOLOGY); // find an entry point in the web app
            return base.OnConnected();
        }
    }
}