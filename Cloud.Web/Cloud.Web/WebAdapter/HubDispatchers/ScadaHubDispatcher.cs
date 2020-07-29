using Microsoft.AspNetCore.SignalR.Client;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void NotifyScadaDataUpdate(Dictionary<long, AnalogModbusData> scadaData)
        {
            _connection.StartAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    // TODO: log error
                }
                else
                {
                    _connection.InvokeAsync("NotifyScadaDataUpdate", scadaData);
                }
            }).Wait();
        }
    }
}
