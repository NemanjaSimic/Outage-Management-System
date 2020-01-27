using Outage.Common;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TopologyServiceClientMock;

namespace OutageManagementService
{
    public class OutageModel
    {

        public static UIModel topology = new UIModel();
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        #region Proxies
        private TopologyServiceProxy topologyProxy = null;

        protected TopologyServiceProxy TopologyProxy
        {
            get
            {
                int numberOfTries = 0;
                while (numberOfTries < 10)
                {
                    try
                    {
                        if(topologyProxy != null)
                        {
                            topologyProxy.Abort();
                            topologyProxy = null;
                        }

                        topologyProxy = new TopologyServiceProxy(EndpointNames.TopologyServiceEndpoint);
                        topologyProxy.Open();
                        break;
                    }
                    catch(Exception ex)
                    {
                        string message = $"Exception on TopologyServiceProxy initialization. Message: {ex.Message}";
                        Logger.LogError(message, ex);
                        topologyProxy = null;
                    }
                    finally
                    {
                        numberOfTries++;
                        Logger.LogDebug($"OutageModel: TopologyServiceProxy getter, try number: {numberOfTries}.");
                        Thread.Sleep(500);
                    }
                }

                return topologyProxy;
            }
        }
        #endregion
        public OutageModel()
        {
            ImportTopologyModel();
        }

        private void ImportTopologyModel()
        {
            using (TopologyServiceProxy topologyServiceProxy = TopologyProxy)
            {
                topology = TopologyProxy.GetTopology();
                //PrintUI(topology);
            }
        }

        //private void PrintUI(UIModel topology)
        //{
        //    if (topology.Nodes.Count > 0)
        //    {
        //        Print(topology.Nodes[topology.FirstNode], topology);
        //    }
        //}

        //private void Print(UINode parent, UIModel topology)
        //{
        //    var connectedElements = topology.GetRelatedElements(parent.Gid);
        //    if (connectedElements != null)
        //    {
        //        foreach (var connectedElement in connectedElements)
        //        {
        //            Console.WriteLine($"{parent.Type} with gid {parent.Gid.ToString("X")} connected to {topology.Nodes[connectedElement].Type} with gid {topology.Nodes[connectedElement].Gid.ToString("X")}");
        //            Print(topology.Nodes[connectedElement], topology);
        //        }
        //    }
        //}
    }
}
