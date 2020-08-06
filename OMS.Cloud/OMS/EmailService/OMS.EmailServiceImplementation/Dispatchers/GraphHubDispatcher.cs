using Microsoft.AspNet.SignalR.Client;
using OMS.CallTrackingServiceImplementation.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.CallTrackingServiceImplementation.Dispatchers
{
	public class GraphHubDispatcher : IDispatcher
    {
        private readonly string url;
        private readonly string hubName;

        private readonly HubConnection connection;
        private readonly IHubProxy proxy;

        public bool IsConnected { get; private set; }

        public GraphHubDispatcher()
        {
            url = ConfigurationManager.AppSettings["hubUrl"];
            hubName = ConfigurationManager.AppSettings["hubName"];
            IsConnected = false;

            connection = new HubConnection(url);
            proxy = connection.CreateHubProxy(hubName);
        }

        public void Connect()
        {
            connection.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine($"[GraphHubDispatcher::Connect] Could not connect to Graph SignalR Hub.");
                }
                else
                {
                    Console.WriteLine($"[GraphHubDispatcher::Connect] Connected to {hubName}. ");
                    IsConnected = true;
                }
            }).Wait();
        }

        public void Dispatch(long gid)
        {
            if (!IsConnected)
			{
                Connect();
			}

            try
            {
                Console.WriteLine($"[GraphHubDispatcher::Dispatch] Sending graph outage call update to Graph Hub");
                proxy.Invoke<string>("NotifyGraphOutageCall", gid)?.Wait();
            }
            catch (Exception)
            {
                Console.WriteLine($"[GraphHubDispatcher::Dispatch] Sending graph outage call update failed.");
            }
        }

        public void Stop()
        {
            if (IsConnected)
			{
                connection.Stop();
			}

            IsConnected = false;
        }

        ~GraphHubDispatcher()
        {
            Stop();
        }
    }
}
