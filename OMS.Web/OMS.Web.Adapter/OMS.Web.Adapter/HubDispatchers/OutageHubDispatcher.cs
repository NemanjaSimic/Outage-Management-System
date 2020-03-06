using Microsoft.AspNet.SignalR.Client;
using OMS.Web.Common;
using OMS.Web.Common.Mappers;
using Outage.Common.PubSub.OutageDataContract;
using System;

namespace OMS.Web.Adapter.HubDispatchers
{
    public class OutageHubDispatcher
    {
        // TODO: IDisposable
        private readonly string _url;
        private readonly string _hubName;

        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        private readonly IOutageMapper _mapper;   

        public OutageHubDispatcher(IOutageMapper mapper)
        {
            _url = AppSettings.Get<string>(HubAddress.OutageHubUrl);
            _hubName = AppSettings.Get<string>(HubAddress.OutageHubName);

            _connection = new HubConnection(_url);
            _proxy = _connection.CreateHubProxy(_hubName);

            _mapper = mapper;
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

        public void NotifyActiveOutageUpdate(ActiveOutageMessage activeOutage)
        {
            Console.WriteLine($"Sending active outage update to Outage Hub. ActiveOutage[ID: {activeOutage.OutageId}, State: {activeOutage.OutageState}, ElementGid: {activeOutage.OutageElementGid}, ReportedAt: {activeOutage.ReportTime} IsolatedAt: {activeOutage.IsolatedTime} RepairedAt {activeOutage.RepairedTime}]");
            _proxy.Invoke<string>("NotifyActiveOutageUpdate", _mapper.MapActiveOutage(activeOutage)).Wait();
        }

        public void NotifyArchiveOutageUpdate(ArchivedOutageMessage archivedOutage)
        {
            Console.WriteLine($"Sending archived outage update to Outage Hub. ArchivedOutage[ID: {archivedOutage.OutageId}, ElementGid: {archivedOutage.OutageElementGid}, ArchivedAt: {archivedOutage.ArchivedTime}]");
            _proxy.Invoke<string>("NotifyArchiveOutageUpdate", _mapper.MapArchivedOutage(archivedOutage)).Wait();
        }

        public void Stop() => _connection.Stop();
    }
}
