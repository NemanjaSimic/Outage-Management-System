namespace OMS.Web.Adapter.HubDispatchers
{
    using System;
    using OMS.Web.Common;
    using System.Collections.Generic;
    using OMS.Web.UI.Models.ViewModels;
    using Microsoft.AspNet.SignalR.Client;

    public class GraphHubDispatcher
    {
        // TODO: IDisposable
        private readonly string _url;
        private readonly string _hubName;

        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        public GraphHubDispatcher()
        {
            _url = AppSettings.Get<string>(HubAddress.GraphHubUrl);
            _hubName = AppSettings.Get<string>(HubAddress.GraphHubName);

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

        public void NotifyGraphUpdate(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        {
            Console.WriteLine($"Sending graph update to Graph Hub");
            _proxy.Invoke<string>("NotifyGraphUpdate", nodes, relations).Wait();
        }

        public void Stop() => _connection.Stop();
    }
}