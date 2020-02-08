using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using OMS.Web.Common.Mappers;
using OMS.Web.UI.Models.ViewModels;
using System.Threading.Tasks;

namespace OMS.Web.API.Hubs
{
    [HubName("outagehub")]
    public class OutageHub : Hub
    {
        private static IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<OutageHub>();

        public void NotifyActiveOutageUpdate(ActiveOutageViewModel activeOutage)
        {
            Clients.All.activeOutageUpdate(activeOutage);
        }

        public void NotifyArchivedOutageUpdate(ArchivedOutageViewModel archivedOutage)
        {
            Clients.All.archivedOutageUpdate(archivedOutage);
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