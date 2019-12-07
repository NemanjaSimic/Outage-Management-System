using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using OMS.Web.UI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Web.API.Hubs
{
    [HubName("graphhub")]   
    public class GraphHub : Hub
    {
        private static IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<GraphHub>();

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
            return base.OnConnected();
        }
    }
}