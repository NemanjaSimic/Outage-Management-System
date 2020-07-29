using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using OMS.Web.Common;
using OMS.Web.UI.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Hubs;

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

        public void NotifyGraphUpdate(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        {
            _connection.StartAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    // TODO: log error
                }
                else
                {
                    _connection.InvokeAsync("NotifyGraphUpdate", nodes, relations);
                }
            }).Wait();
        }
    }
}
