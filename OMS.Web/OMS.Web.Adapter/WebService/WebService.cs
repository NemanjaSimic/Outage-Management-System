using OMS.Web.Adapter.Contracts;
using OMS.Web.Adapter.HubDispatchers;
using OMS.Web.UI.Models.ViewModels;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;

namespace OMS.Web.Adapter.WebService
{
    using OMS.Web.Adapter.Contracts;
    using OMS.Web.Adapter.HubDispatchers;
    using OMS.Web.UI.Models.ViewModels;
    using System;
    using System.Collections.Generic;

    public class WebService : IWebService
    {
        private GraphHubDispatcher _graphDispatcher = null;
        private ScadaHubDipatcher _scadaDipatcher = null;

        public WebService()
        {
            _graphDispatcher = new GraphHubDispatcher();
            _scadaDipatcher = new ScadaHubDipatcher();
        }

        public void UpdateGraph(List<Node> nodes, List<Relation> relations)
        {
            Console.WriteLine("Hello from UpdateGraph()");

            _graphDispatcher.Connect();

            try
            {
                _graphDispatcher.NotifyGraphUpdate(nodes, relations);
                Console.WriteLine($"Sent notification to Graph Hub");
            }
            catch (Exception e)
            {
                // retry ?
                Console.WriteLine($"An exception occured during WebService.UpdateGraph(): {e.Message}");
            }
        }

        public void UpdateScadaData(Dictionary<long, AnalogModbusData> scadaData)
        {
            Console.WriteLine("Hello from UpdateScadaData()");

            _scadaDipatcher.Connect();

            try
            {
                _scadaDipatcher.NotifyScadaDataUpdate(scadaData);
                Console.WriteLine($"Sent notification to SCADA Hub");
            }
            catch (Exception e)
            {
                // retry ?
                Console.WriteLine($"An exception occured during WebService.UpdateScadaData(): {e.Message}");
            }
        }
    }
}
