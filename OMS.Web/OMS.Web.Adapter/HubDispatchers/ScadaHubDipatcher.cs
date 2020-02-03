using Microsoft.AspNet.SignalR.Client;
using OMS.Web.Common;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Web.Adapter.HubDispatchers
{
    public class ScadaHubDipatcher
    {
        // TODO: IDisposable
        private readonly string _url;
        private readonly string _hubName;

        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        public ScadaHubDipatcher()
        {
            _url = AppSettings.Get<string>("scadaHubUrl");
            _hubName = AppSettings.Get<string>("scadaHubName");

            _connection = new HubConnection(_url);
            _proxy = _connection.CreateHubProxy(_hubName);
        }

        public void Connect()
        {
            _connection.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine($"There was an error opening the connection: {task.Exception.GetBaseException()}");
                }
                else
                {
                    Console.WriteLine($"Connected to {_hubName}. ");
                }
            }).Wait();
        }

        public void NotifyScadaDataUpdate(Dictionary<long, AnalogModbusData> scadaData)
        {
            Console.WriteLine($"Sending scada data update to Scada Hub");
            _proxy.Invoke<string>("NotifyScadaDataUpdate", scadaData).Wait();
        }

        public void Stop() => _connection.Stop();
    }
}
