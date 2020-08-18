using Microsoft.AspNetCore.SignalR;
using OMS.Common.Cloud.Logger;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Hubs
{
    public class ScadaHub : Hub
    {
        private readonly string baseLogString;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public ScadaHub()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
        }

        //public void NotifyScadaDataUpdate(Dictionary<long, AnalogModbusData> scadaData)
        public void NotifyScadaDataUpdate(string scadaDataJson)
        {
            try
            {
                //Clients.All.SendAsync("updateScadaData", scadaData);

                Logger.LogDebug($"{baseLogString} NotifyScadaDataUpdate => About to call Clients.All.SendAsync().");
                Clients.All.SendAsync("updateScadaData", scadaDataJson);
                Logger.LogDebug($"{baseLogString} NotifyScadaDataUpdate => scada data in json format sent to front-end: {scadaDataJson}");
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} NotifyScadaDataUpdate => Exception: {e.Message}", e);
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
