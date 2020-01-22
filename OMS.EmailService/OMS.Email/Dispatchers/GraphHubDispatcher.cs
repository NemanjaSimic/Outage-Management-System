using Microsoft.AspNet.SignalR.Client;
using OMS.Email.Interfaces;
using System;
using System.Configuration;

namespace OMS.Email.Dispatchers
{
    public class GraphHubDispatcher : IDispatcher
    {
        private readonly string _url;
        private readonly string _hubName;
        private bool _isConnected;

        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        public GraphHubDispatcher()
        {
            _url = ConfigurationManager.AppSettings["hubUrl"];
            _hubName = ConfigurationManager.AppSettings["hubName"];
            _isConnected = false;

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

        public void Dispatch(long gid)
        {
            if(!_isConnected)
                Connect();

            Console.WriteLine($"Sending graph outage call update to Graph Hub");
            _proxy.Invoke<string>("NotifyGraphOutageCall", gid).Wait();
        }

        public void Stop()
        {
            if(_isConnected) 
                _connection.Stop();

            _isConnected = false;
        }

        ~GraphHubDispatcher()
        {
            Stop();
        }
    }

}
