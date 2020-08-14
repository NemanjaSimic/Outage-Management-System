using Microsoft.AspNetCore.SignalR;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Hubs
{
    public class ScadaHub : Hub
    {
        public void NotifyScadaDataUpdate(Dictionary<long, AnalogModbusData> scadaData)
        {
            Clients.All.SendAsync("updateScadaData", scadaData);
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
