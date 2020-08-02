using Common.Web.UI.Models.ViewModels;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Hubs
{
    public class GraphHub : Hub
    {
        public void NotifyGraphUpdate(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        {
            Clients.All.SendAsync("updateGraph", new OmsGraphViewModel { Nodes = nodes, Relations = relations });
        }

        public void NotifyGraphOutageCall(long gid)
        {
            Clients.All.SendAsync("reportOutageCall", gid);
        }

        public void Join()
        {
            Groups.AddToGroupAsync(Context.ConnectionId, "Users");
        }

        public override Task OnConnectedAsync()
        {
            Groups.AddToGroupAsync(Context.ConnectionId, "Users");
            return base.OnConnectedAsync();
        }
    }
}