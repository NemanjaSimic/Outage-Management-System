using Common.Web.Models.ViewModels;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WebAPI.Hubs
{
    public class OutageHub : Hub
    {
        public void NotifyActiveOutageUpdate(ActiveOutageViewModel activeOutage)
        {
            Clients.All.SendAsync("activeOutageUpdate", activeOutage);
        }

        public void NotifyArchivedOutageUpdate(ArchivedOutageViewModel archivedOutage)
        {
            Clients.All.SendAsync("archivedOutage", archivedOutage);
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
