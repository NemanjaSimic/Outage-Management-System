using Common.Contracts.WebAdapterContracts;
using Common.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAdapterImplementation.HubDispatchers;

namespace WebAdapterImplementation
{
    public class WebAdapterProvider : IWebAdapterContract
    {
        private GraphHubDispatcher _graphDispatcher = null;
        private ScadaHubDispatcher _scadaDipatcher = null;

        public WebAdapterProvider()
        {
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
                // retry ?
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
                // retry ?
            }

            return null;
        }
    }
}
