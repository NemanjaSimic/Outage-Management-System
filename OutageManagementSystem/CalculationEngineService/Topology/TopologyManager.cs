using CECommon;
using CECommon.Interfaces;
using CECommon.Model;
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

        public List<ITopology> UpdateTopology(long elementGid)
        {
            logger.LogDebug("Updating topology started.");
            List<ITopology> topologies = Provider.Instance.TopologyProvider.GetTopologies();
            List<ITopology> tempTopologies = new List<ITopology>(topologies);
            ITopology topology = new TopologyModel();
            Stack<long> stack = new Stack<long>();

            foreach (var topologyModel in tempTopologies)
            {
                if (topologyModel.GetElementByGid(elementGid, out ITopologyElement firstTopologyElement))
                {
                    topologies.Remove(topologyModel);
                    topology = topologyModel;
                    stack.Push(elementGid);
                    while (stack.Count > 0)
                    {
                        ITopologyElement element = topology.TopologyElements[stack.Pop()];
                        foreach (var measurement in element.Measurements)
                        {
                            if (measurement is DiscreteMeasurement discreteMeasurement)
                            {

                                if (Provider.Instance.CacheProvider.GetDiscreteValue(measurement.Id) == true)
                                {
                                    topology.TopologyElements[element.Id].IsActive = false;
                                    TurnOffAllElements(element.Id, topology);
                                    break;
                                }
                                else
                                {
                                    topology.TopologyElements[element.Id].IsActive = true;
                                }
                            }
                        }

                        foreach (var child in element.SecondEnd)
                        {
                            stack.Push(child);
                        }

                    }
                    break;
                }
            }
            topologies.Add(topology);
            logger.LogDebug("Updating topology finished.");
            return topologies;

        }

        private void TurnOffAllElements(long topologyElement, ITopology topology)
        {
            Stack<long> stack = new Stack<long>();
            stack.Push(topologyElement);
            while (stack.Count > 0)
            {
                var element = stack.Pop();
                topology.TopologyElements[element].IsActive = false;
                foreach (var child in topology.TopologyElements[element].SecondEnd)
                {
                    stack.Push(child);
                }
            }
        }
    }
}
