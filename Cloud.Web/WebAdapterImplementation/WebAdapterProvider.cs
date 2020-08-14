using Common.Contracts.WebAdapterContracts;
using Common.Web.Models.ViewModels;
using OMS.Common.Cloud.Logger;
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

        private GraphHubDispatcher _graphDispatcher = null;
        private ScadaHubDispatcher _scadaDipatcher = null;

        public WebAdapterProvider()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");
        }

        public Task UpdateGraph(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        {
            _graphDispatcher = new GraphHubDispatcher();
            _graphDispatcher.Connect();

            try
            {
                _graphDispatcher.NotifyGraphUpdate(nodes, relations);
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} UpdateGraph => exception {e.Message}";
                Logger.LogError(errorMessage, e);
            }

            return null;
        }

        public Task UpdateScadaData(Dictionary<long, OMS.Common.PubSubContracts.DataContracts.SCADA.AnalogModbusData> scadaData)
        {
            _scadaDipatcher = new ScadaHubDispatcher();
            _scadaDipatcher.Connect();

            try
            {
                _scadaDipatcher.NotifyScadaDataUpdate(scadaData);
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} UpdateScadaData => exception {e.Message}";
                Logger.LogError(errorMessage, e);
            }

            return null;
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => true);
        }
    }
}
