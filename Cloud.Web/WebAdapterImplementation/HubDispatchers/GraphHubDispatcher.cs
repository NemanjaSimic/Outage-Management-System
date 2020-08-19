using Common.Web.Models.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OMS.Common.Cloud.Logger;
using System;
using System.Collections.Generic;
using System.Fabric.Query;
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

            //this.connection = new HubConnectionBuilder().WithUrl(graphHubUrl)
                                                        //.Build();

            this.connection = new HubConnectionBuilder().WithUrl(graphHubUrl).AddJsonProtocol(options => {
                options.PayloadSerializerOptions.PropertyNamingPolicy = null;
            })
            .Build();
        }

        public void Connect()
        {
            this.connection.StartAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    string message = $"{baseLogString} Connect => Fault on connection.";
                    Logger.LogError(message);
                }
                else
                {
                    string message = $"{baseLogString} Connect => Hub Successfully connected. url: {graphHubUrl}";
                    Logger.LogDebug(message);
                }
            }).Wait();
        }

        //TODO: OPTION 1 - promeniti potpis medtode u GraphHub kada se koristi (isto je obelezeno sa option 1 i option 2)
        public async Task NotifyGraphUpdate(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        {
            try
            {
                Logger.LogDebug($"{baseLogString} NotifyGraphUpdate => nodes count: {nodes.Count}, relations count: {relations.Count}");

                //var omsGraphViewModel = new OmsGraphViewModel()
                //{
                //    Nodes = nodes,
                //    Relations = relations,
                //};

                //var jsonOutput = JsonConvert.SerializeObject(omsGraphViewModel);
                await this.connection.InvokeAsync("NotifyGraphUpdate", nodes, relations);

                //Logger.LogDebug($"{baseLogString} NotifyGraphUpdate => json output sent to graph hub: {jsonOutput}");
                Logger.LogDebug($"{baseLogString} NotifyGraphUpdate => Graph sent to UI.");
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} NotifyGraphUpdate => Exception: {e.Message}", e);
            }
        }

        //TODO: OPTION 2 - promeniti potpis medtode u GraphHub kada se koristi
        //public async Task NotifyGraphUpdate(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        //{
        //    try
        //    {
        //        Logger.LogDebug($"{baseLogString} NotifyGraphUpdate => nodes count: {nodes.Count}, relations count: {relations.Count}");

        //        await this.connection.InvokeAsync("NotifyGraphUpdate", nodes, relations);

        //        Logger.LogDebug($"{baseLogString} NotifyGraphUpdate => json output sent to graph hub => nodes count: {nodes.Count}, relations count: {relations.Count}");
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.LogError($"{baseLogString} NotifyGraphUpdate => Exception: {e.Message}", e);
        //    }
        //}

        //public void NotifyGraphUpdate(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        //{
        //    this.connection.InvokeAsync("NotifyGraphUpdate", nodes, relations).Wait();
        //}
    }
}