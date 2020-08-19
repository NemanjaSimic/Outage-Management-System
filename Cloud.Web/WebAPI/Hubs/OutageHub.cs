using Common.Web.Models.ViewModels;
using Microsoft.AspNetCore.SignalR;
using OMS.Common.Cloud.Logger;
using System;
using System.Threading.Tasks;

namespace WebAPI.Hubs
{
    public class OutageHub : Hub
    {
        private readonly string baseLogString;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public OutageHub()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
        }

        //public void NotifyActiveOutageUpdate(ActiveOutageViewModel activeOutage)
        public async Task NotifyActiveOutageUpdate(string activeOutageJson)
        {
            try
            {
                //Clients.All.SendAsync("activeOutageUpdate", activeOutage);

                Logger.LogDebug($"{baseLogString} NotifyActiveOutageUpdate => About to call Clients.All.SendAsync().");
                await Clients.All.SendAsync("activeOutageUpdate", activeOutageJson);
                Logger.LogDebug($"{baseLogString} NotifyActiveOutageUpdate => active outage data [json format] sent to front-end: {activeOutageJson}");
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} NotifyActiveOutageUpdate => Exception: {e.Message}", e);
            }
        }

        //public void NotifyArchivedOutageUpdate(ArchivedOutageViewModel archivedOutage)
        public async Task NotifyArchivedOutageUpdate(string archivedOutageJson)
        {
            try
            {
                //Clients.All.SendAsync("archivedOutage", archivedOutage);

                Logger.LogDebug($"{baseLogString} NotifyArchivedOutageUpdate => About to call Clients.All.SendAsync().");
                await Clients.All.SendAsync("archivedOutage", archivedOutageJson);
                Logger.LogDebug($"{baseLogString} NotifyArchivedOutageUpdate => archived outage data [json format] sent to front-end: {archivedOutageJson}");
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} NotifyArchivedOutageUpdate => Exception: {e.Message}", e);
            }
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
