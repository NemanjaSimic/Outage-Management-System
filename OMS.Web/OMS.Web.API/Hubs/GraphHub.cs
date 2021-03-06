﻿using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using OMS.Web.UI.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Web.API.Hubs
{
    [HubName("graphhub")]   
    public class GraphHub : Hub
    {
        private static IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<GraphHub>();

        public void NotifyGraphUpdate(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        {
            Clients.All.updateGraph(new OmsGraphViewModel { Nodes = nodes, Relations = relations });
        }

        public void NotifyGraphOutageCall(long gid)
        {
            Clients.All.reportOutageCall(gid);
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