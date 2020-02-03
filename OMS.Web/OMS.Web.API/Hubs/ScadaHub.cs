using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OMS.Web.API.Hubs
{
    [HubName("scadahub")]
    public class ScadaHub : Hub
    {
        private static IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<GraphHub>();

        public void NotifyScadaDataUpdate(Dictionary<long, AnalogModbusData> scadaData)
        {
            Clients.All.updateScadaData(scadaData);
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