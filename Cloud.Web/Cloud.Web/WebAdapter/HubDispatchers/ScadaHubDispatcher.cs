using Microsoft.AspNetCore.SignalR.Client;
using Outage.Common.PubSub.SCADADataContract;
using System.Collections.Generic;

namespace WebAdapter.HubDispatchers
{
    class ScadaHubDispatcher
    {
        private HubConnection _connection;

        public ScadaHubDispatcher()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:44351/scadahub")
                .Build();
        }

        public void Connect()
        {
            _connection.StartAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    // TODO: log error
                }
                else
                {
                    // TODO: log error
                }
            }).Wait();
        }

        public void NotifyScadaDataUpdate(Dictionary<long, AnalogModbusData> scadaData)
        {
            _connection.InvokeAsync("NotifyScadaDataUpdate", scadaData).Wait();
        }
    }
}
