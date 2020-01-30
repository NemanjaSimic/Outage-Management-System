using CECommon;
using CECommon.Interfaces;
using CECommon.Providers;
using Outage.Common;
using System.Collections.Generic;

namespace Topology
{
    public class TopologyManager : IModelTopologyServis
    {
        #region Fields
        ILogger logger = LoggerWrapper.Instance;
        private ITopologyBuilder topologyBuilder;
        private List<long> roots;
        #endregion

        public TopologyManager(ITopologyBuilder topologyBuilder)
        {
            this.topologyBuilder = topologyBuilder;
        }

        public List<ITopology> CreateTopology()
        {
            logger.LogDebug("Get all energy sources started.");
            roots = Provider.Instance.ModelProvider.GetEnergySources();
            logger.LogDebug("Get all energy sources finished.");

            List<ITopology> topologyModel = new List<ITopology>();

            foreach (var rootElement in roots)
            {
                topologyModel.Add(topologyBuilder.CreateGraphTopology(rootElement));
            }

            return topologyModel;
        }

        public void UpdateTopology(long elementGid)
        {
            logger.LogDebug("Updating topology started.");
            List<ITopology> topologies = Provider.Instance.TopologyProvider.GetTopologies();
            foreach (var topology in topologies)
            {
                if (topology.GetElementByGid(elementGid, out ITopologyElement topologyElement))
                {
                    while (topologyElement.SecondEnd.Count > 0)
                    {

                    }
                    break;
                }
            }
           // TopologyModel = CreateTopology();
            logger.LogDebug("Updating topology finished.");
            //Publish();
        }


    }
}
