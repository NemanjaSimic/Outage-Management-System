using Common.Web.Models.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;

namespace WebAdapterImplementation.HubDispatchers
{
    class GraphHubDispatcher
    {
        private HubConnection _connection;

        public GraphHubDispatcher()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:44351/graphhub")
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

        public void NotifyGraphUpdate(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        {
            _connection.InvokeAsync("NotifyGraphUpdate", nodes, relations).Wait();
        }
    }
}