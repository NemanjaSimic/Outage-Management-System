using OMS.Web.Adapter.Contracts;
using OMS.Web.UI.Models.ViewModels;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using WebAdapter.HubDispatchers;

namespace WebAdapter.WebService
{
    class WebService : IWebService
    {
        private GraphHubDispatcher _graphDispatcher = null;
        private ScadaHubDispatcher _scadaDipatcher = null;

        public WebService()
        {
            _graphDispatcher = new GraphHubDispatcher();
            _scadaDipatcher = new ScadaHubDispatcher();
        }

        public void UpdateGraph(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        {
            _graphDispatcher.Connect();

            try
            {
                _graphDispatcher.NotifyGraphUpdate(nodes, relations);
            }
            catch
            {
                // TODO: log error
            }
        }

        public void UpdateScadaData(Dictionary<long, AnalogModbusData> scadaData)
        {
            _scadaDipatcher.Connect();

            try
            {
                _scadaDipatcher.NotifyScadaDataUpdate(scadaData);
            }
            catch
            {
                // TODO: log error
            }
        }
    }
}
