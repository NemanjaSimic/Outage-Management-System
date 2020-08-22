using Common.Web.Models.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using OMS.Common.Cloud.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAdapterImplementation.HubDispatchers
{
    class GraphHubDispatcher
    {
        private const string graphHubUrl = "http://localhost:44351/graphhub";

        private readonly string baseLogString;
        private readonly HubConnection connection;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public GraphHubDispatcher()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            this.connection = new HubConnectionBuilder().WithUrl(graphHubUrl)
                                                        .Build();
        }

        public void Connect()
        {
            this.connection.StartAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Logger.LogDebug($"{baseLogString} Connect => successfuly connected to hub.");
                }
                else
                {
                    Logger.LogWarning($"{baseLogString} Connect => connection failed.");
                }
            }).Wait();
        }

        public async Task NotifyGraphUpdate(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        {
            try
            {
                Logger.LogDebug($"{baseLogString} NotifyGraphUpdate => nodes count: {nodes.Count}, relations count: {relations.Count}");

                var omsGraphData = new OmsGraphViewModel { Nodes = nodes, Relations = relations };
                var jsonOutput = JsonConvert.SerializeObject(omsGraphData);
                await this.connection.InvokeAsync("NotifyGraphUpdate", jsonOutput);

                Logger.LogDebug($"{baseLogString} NotifyGraphUpdate => json output sent to graph hub: {jsonOutput}");
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} NotifyGraphUpdate => Exception: {e.Message}", e);
            }
        }
    }
}