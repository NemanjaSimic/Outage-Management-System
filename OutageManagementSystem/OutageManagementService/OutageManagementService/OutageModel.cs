using Outage.Common;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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

        private TopologyServiceProxy GetTopologyProxy()
        {
            int numberOfTries = 0;
            int sleepInterval = 500;

            while (numberOfTries <= int.MaxValue)
            {
                try
                {
                    if (topologyProxy != null)
                    {
                        topologyProxy.Abort();
                        topologyProxy = null;
                    }

                    topologyProxy = new TopologyServiceProxy(EndpointNames.TopologyServiceEndpoint);
                    topologyProxy.Open();

                    if (topologyProxy.State == CommunicationState.Opened)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    string message = $"Exception on TopologyServiceProxy initialization. Message: {ex.Message}";
                    Logger.LogError(message, ex);
                    topologyProxy = null;
                }
                finally
                {
                    numberOfTries++;
                    Logger.LogDebug($"OutageModel: TopologyServiceProxy getter, try number: {numberOfTries}.");

                    if (numberOfTries >= 100)
                    {
                        sleepInterval = 1000;
                    }

                    Thread.Sleep(sleepInterval);
                }
            }

            return topologyProxy;
        }
        #endregion

        public OutageModel()
        {
            ImportTopologyModel();
        }

        private void ImportTopologyModel()
        {
            using (TopologyServiceProxy topologyServiceProxy = GetTopologyProxy())
            {
                if (topologyServiceProxy != null)
                {
                    topology = topologyServiceProxy.GetTopology();
                    //PrintUI(topology);
                }
                else
                {
                    string message = "From method ImportTopologyModel(): TopologyServiceProxy is null.";
                    logger.LogError(message);
                    throw new NullReferenceException(message);
                }
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
