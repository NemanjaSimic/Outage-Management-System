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
            this.mapper = mapper;

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
                Logger.LogDebug($"{baseLogString} NotifyActiveOutageUpdate => active outage id: {activeOutage.OutageId}");

                var jsonOutput = JsonConvert.SerializeObject(this.mapper.MapActiveOutage(activeOutage));
                await this.connection.InvokeAsync("NotifyActiveOutageUpdate", jsonOutput);

                Logger.LogDebug($"{baseLogString} NotifyActiveOutageUpdate => json output sent to outage hub: {jsonOutput}");
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} NotifyActiveOutageUpdate => Exception: {e.Message}", e);
            }
        }

        //public void NotifyActiveOutageUpdate(ActiveOutageMessage activeOutage)
        //{
        //    connection.InvokeAsync("NotifyActiveOutageUpdate", mapper.MapActiveOutage(activeOutage)).Wait();
        //}


        public async Task NotifyArchiveOutageUpdate(ArchivedOutageMessage archivedOutage)
        {
            try
            {
                Logger.LogDebug($"{baseLogString} NotifyArchiveOutageUpdate => archived outage id: {archivedOutage.OutageId}");

                var jsonOutput = JsonConvert.SerializeObject(this.mapper.MapArchivedOutage(archivedOutage));
                await this.connection.InvokeAsync("NotifyArchiveOutageUpdate", jsonOutput);

                Logger.LogDebug($"{baseLogString} NotifyArchiveOutageUpdate => json output sent to outage hub: {jsonOutput}");
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} NotifyArchiveOutageUpdate => Exception: {e.Message}", e);
            }
        }

        //public void NotifyArchiveOutageUpdate(ArchivedOutageMessage archivedOutage)
        //{
        //    connection.InvokeAsync("NotifyArchiveOutageUpdate", mapper.MapArchivedOutage(archivedOutage)).Wait();
        //}
    }
}
