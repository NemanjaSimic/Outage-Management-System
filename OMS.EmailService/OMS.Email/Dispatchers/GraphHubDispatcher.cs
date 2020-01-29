namespace OMS.Email.Dispatchers
{
    using Microsoft.AspNet.SignalR.Client;
    using OMS.Email.Interfaces;
    using System;
    using System.Configuration;
    
    public class GraphHubDispatcher : IDispatcher
    {
        private readonly string _url;
        private readonly string _hubName;

        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        public bool IsConnected { get; private set; }

        public GraphHubDispatcher()
        {
            _url = ConfigurationManager.AppSettings["hubUrl"];
            _hubName = ConfigurationManager.AppSettings["hubName"];
            IsConnected = false;

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
                    IsConnected = true;
                }
            }).Wait();
        }

        public void Dispatch(long gid)
        {
            if (!IsConnected)
                Connect();

            try
            {
                Console.WriteLine($"Sending graph outage call update to Graph Hub");
                _proxy.Invoke<string>("NotifyGraphOutageCall", gid).Wait();
            }
            catch (Exception)
            {
                Console.WriteLine($"Sending graph outage call update failed.");
            }
        }

        public void Stop()
        {
            if (IsConnected)
                _connection.Stop();

            IsConnected = false;
        }

        ~GraphHubDispatcher()
        {
            Stop();
        }
    }

}
