using OMS.Web.Adapter.Contracts;
using OMS.Web.UI.Models;
using System;
using System.Collections.Generic;

namespace OMS.Web.Adapter.WebService
{
    public class WebService : IWebService
    {
        public void UpdateGraph(List<Node> nodes, List<Relation> relations)
        {
            Console.WriteLine("Hello from UpdateGraph()");
        }
    }
}
