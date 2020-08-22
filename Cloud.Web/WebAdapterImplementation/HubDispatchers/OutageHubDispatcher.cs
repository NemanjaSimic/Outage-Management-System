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
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            this.connection = new HubConnectionBuilder().WithUrl(outageHubUrl)
                                                        .Build();
            this.mapper = mapper;
        }

        public void Connect()
        {
            this.connection.StartAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Logger.LogDebug($"{baseLogString} Connect => successfuly connected to hub.");
                }
                else
                {
                    Logger.LogWarning($"{baseLogString} Connect => connection failed.");
                }
            }).Wait();
        }


        public async Task NotifyActiveOutageUpdate(ActiveOutageMessage activeOutage)
        {
            try
            {
                Logger.LogDebug($"{baseLogString} NotifyActiveOutageUpdate => active outage data => gid: 0x{activeOutage.OutageElementGid:X16}");

                var jsonOutput = JsonConvert.SerializeObject(activeOutage);
                await this.connection.InvokeAsync("NotifyActiveOutageUpdate", jsonOutput);

                Logger.LogDebug($"{baseLogString} NotifyActiveOutageUpdate => json output sent to scada hub: {jsonOutput}");
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

                var jsonOutput = JsonConvert.SerializeObject(archivedOutage);
                await this.connection.InvokeAsync("NotifyArchiveOutageUpdate", jsonOutput);

                Logger.LogDebug($"{baseLogString} NotifyArchiveOutageUpdate => json output sent to scada hub: {jsonOutput}");
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} NotifyArchiveOutageUpdate => Exception: {e.Message}", e);
            }
        }
    }
}
