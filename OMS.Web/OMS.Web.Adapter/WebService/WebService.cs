using OMS.Web.Adapter.Contracts;
using OMS.Web.Adapter.HubDispatchers;
using OMS.Web.UI.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace OMS.Web.Adapter.WebService
{
    public class WebService : IWebService
    {
        private GraphHubDispatcher _dispatcher = null;

        public WebService()
        {
            _dispatcher = new GraphHubDispatcher();
        }

        public void UpdateGraph(List<Node> nodes, List<Relation> relations)
        {
            Console.WriteLine("Hello from UpdateGraph()");

            _dispatcher.Connect();
            try
            {
                _dispatcher.NotifyGraphUpdate(nodes, relations);
                Console.WriteLine($"Sent notification to Graph Hub");
            }
            catch (Exception e)
            {
                // retry ?
                Console.WriteLine($"An exception occured during WebService.UpdateGraph(): {e.Message}");
            }
        }
    }
}
