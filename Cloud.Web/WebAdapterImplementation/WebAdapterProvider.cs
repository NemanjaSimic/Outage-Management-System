using Common.Contracts.WebAdapterContracts;
using Common.Web.Models.ViewModels;
using OMS.Common.Cloud.Logger;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAdapterImplementation.HubDispatchers;

namespace WebAdapterImplementation
{
    public class WebAdapterProvider : IWebAdapterContract
    {
        private readonly string baseLogString;

        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public WebAdapterProvider()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");
        }

        public async Task UpdateGraph(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        {
            var graphDispatcher = new GraphHubDispatcher();
            graphDispatcher.Connect();

            try
            {
                await graphDispatcher.NotifyGraphUpdate(nodes, relations);
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} UpdateGraph => exception {e.Message}";
                Logger.LogError(errorMessage, e);
            }
        }

        public async Task UpdateScadaData(Dictionary<long, AnalogModbusData> scadaData)
        {
            var scadaDipatcher = new ScadaHubDispatcher();
            scadaDipatcher.Connect();

            try
            {
                await scadaDipatcher.NotifyScadaDataUpdate(scadaData);
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} UpdateScadaData => exception {e.Message}";
                Logger.LogError(errorMessage, e);
            }
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => true);
        }
    }
}
