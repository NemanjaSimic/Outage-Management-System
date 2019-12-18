using Microsoft.AspNet.SignalR.Client;
using OMS.Web.Common;
using OMS.Web.UI.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace OMS.Web.Adapter.HubDispatchers
{
    public class GraphHubDispatcher
    {
        // API hub config - TODO: change to read from config file
        private readonly string _url;
        private readonly string _hubName;

        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        public GraphHubDispatcher()
        {
            _url = AppSettings.Get<string>("hubUrl");
            _hubName = AppSettings.Get<string>("hubName");

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

        public void NotifyGraphUpdate(List<Node> nodes, List<Relation> relations)
        {
            Console.WriteLine($"Sending graph update to Graph Hub");
            _proxy.Invoke<string>("NotifyGraphUpdate", nodes, relations).Wait();
        }

        public void Stop() => _connection.Stop();
    }
}