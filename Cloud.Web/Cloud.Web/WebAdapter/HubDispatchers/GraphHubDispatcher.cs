using Microsoft.AspNetCore.SignalR.Client;
using OMS.Web.UI.Models.ViewModels;
using System.Collections.Generic;

namespace WebAdapter.HubDispatchers
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
