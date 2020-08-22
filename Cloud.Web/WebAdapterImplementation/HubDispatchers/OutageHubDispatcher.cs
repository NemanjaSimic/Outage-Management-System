using Common.PubSubContracts.DataContracts.OMS;
using Common.Web.Mappers;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using OMS.Common.Cloud.Logger;
using System;
using System.Threading.Tasks;

namespace WebAdapterImplementation.HubDispatchers
{
    class OutageHubDispatcher
    {
        private const string outageHubUrl = "http://localhost:44351/outagehub";

        private readonly string baseLogString;
        private readonly IOutageMapper mapper;
        private readonly HubConnection connection;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public OutageHubDispatcher(IOutageMapper mapper)
        {
            this.mapper = mapper;
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            this.connection = new HubConnectionBuilder().WithUrl(outageHubUrl)
                                                        .Build();
        }

        public void Connect()
        {
            this.connection.StartAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    string message = $"{baseLogString} Connect => Fault on connection.";
                    Logger.LogError(message);
                }
                else
                {
                    string message = $"{baseLogString} Connect => Hub Successfully connected. url: {outageHubUrl}";
                    Logger.LogDebug(message);
                }
            }).Wait();
        }

        public async Task NotifyActiveOutageUpdate(ActiveOutageMessage activeOutage)
        {
            try
            {
                Logger.LogDebug($"{baseLogString} NotifyActiveOutageUpdate => active outage data => gid: 0x{activeOutage.OutageElementGid:X16}");

                var jsonOutput = JsonConvert.SerializeObject(this.mapper.MapActiveOutage(activeOutage));
                await this.connection.InvokeAsync("NotifyActiveOutageUpdate", jsonOutput);

                Logger.LogDebug($"{baseLogString} NotifyActiveOutageUpdate => json output sent to outage hub: {jsonOutput}");
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} NotifyActiveOutageUpdate => Exception: {e.Message}", e);
            }
        }

        public async Task NotifyArchiveOutageUpdate(ArchivedOutageMessage archivedOutage)
        {
            try
            {
                Logger.LogDebug($"{baseLogString} NotifyArchiveOutageUpdate => archived outage data => gid: 0x{archivedOutage.OutageElementGid:X16}");

                var jsonOutput = JsonConvert.SerializeObject(this.mapper.MapArchivedOutage(archivedOutage));
                await this.connection.InvokeAsync("NotifyArchiveOutageUpdate", jsonOutput);

                Logger.LogDebug($"{baseLogString} NotifyArchiveOutageUpdate => json output sent to outage hub: {jsonOutput}");
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} NotifyArchiveOutageUpdate => Exception: {e.Message}", e);
            }
        }
    }
}
