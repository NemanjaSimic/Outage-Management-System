using Common.PubSubContracts.DataContracts.OMS;
using Common.Web.Mappers;
using Microsoft.AspNetCore.SignalR.Client;

namespace WebAdapterImplementation.HubDispatchers
{
    class OutageHubDispatcher
    {
        private HubConnection _connection;

        private readonly IOutageMapper _mapper;

        public OutageHubDispatcher(IOutageMapper mapper)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:44351/outagehub")
                .Build();

            _mapper = mapper;
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

        public void NotifyActiveOutageUpdate(ActiveOutageMessage activeOutage)
        {
            _connection.InvokeAsync("NotifyActiveOutageUpdate", _mapper.MapActiveOutage(activeOutage)).Wait();
        }

        public void NotifyArchiveOutageUpdate(ArchivedOutageMessage archivedOutage)
        {
            _connection.InvokeAsync("NotifyArchiveOutageUpdate", _mapper.MapArchivedOutage(archivedOutage)).Wait();
        }
    }
}
