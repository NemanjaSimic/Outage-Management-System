using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using OMS.Common.Cloud.Logger;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAdapterImplementation.HubDispatchers
{
    class ScadaHubDispatcher
    {
        private const string scadaHubUrl = "http://localhost:44351/scadahub";

        private readonly string baseLogString;
        private readonly HubConnection connection;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public ScadaHubDispatcher()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.connection = new HubConnectionBuilder().WithUrl(scadaHubUrl)
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
                    string message = $"{baseLogString} Connect => Hub Successfully connected. url: {scadaHubUrl}";
                    Logger.LogDebug(message);
                }
            }).Wait();
        }

        public async Task NotifyScadaDataUpdate(Dictionary<long, AnalogModbusData> scadaData)
        {
            try
            {
                Logger.LogDebug($"{baseLogString} NotifyScadaDataUpdate => scadaData count: {scadaData.Count}");

                var jsonOutput = JsonConvert.SerializeObject(scadaData);
                await this.connection.InvokeAsync("NotifyScadaDataUpdate", jsonOutput);

                Logger.LogDebug($"{baseLogString} NotifyScadaDataUpdate => json output sent to outage hub: {jsonOutput}");
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} NotifyScadaDataUpdate => Exception: {e.Message}", e);
            }
        }

        //public void NotifyScadaDataUpdate(Dictionary<long, AnalogModbusData> scadaData)
        //{
        //    this.connection.InvokeAsync("NotifyScadaDataUpdate", scadaData).Wait();
        //}
    }
}
