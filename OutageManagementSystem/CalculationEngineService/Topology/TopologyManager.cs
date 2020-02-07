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
                ITopology newTopology = topologyBuilder.CreateGraphTopology(rootElement);
                topologyModel.Add(CalulateLoadFlow(rootElement, newTopology));
            }
            
             return topologyModel;
        }

        public ITopology CalulateLoadFlow(long startingElementGid, ITopology topology)
        {
            logger.LogDebug("CalulateLoadFlow started.");
            Stack<long> stack = new Stack<long>();

            if (topology.GetElementByGid(startingElementGid, out ITopologyElement element))
            {
                stack.Push(startingElementGid);
                while (stack.Count > 0)
                {
                    element = topology.TopologyElements[stack.Pop()];
                    //element.IsActive = IsElementActive(element, topology);
                    IsElementActive(element, topology);

                    if (!element.IsActive)
                    {
                        TurnOffAllElements(element.Id, topology);                 
                    }
                    else
                    {
                        foreach (var child in element.SecondEnd)
                        {
                            stack.Push(child);
                        }
                    }
                }
            }
 
            logger.LogDebug("Updating topology finished.");
            return topology;
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
        public List<ITopology> UpdateLoadFlow(List<long> signalGids, List<ITopology> topologies)
        {
            List<ITopology> retVal = new List<ITopology>(topologies);
            foreach (long signalGid in signalGids)
            {
                foreach (var topology in topologies)
                {
                    if (topology.GetElementByGid(signalGid, out ITopologyElement element))
                    {
                        retVal.Remove(topology);
                        retVal.Add(CalulateLoadFlow(signalGid, topology));
                        break;
                    }
                }
            }
            return retVal;
        }


        private bool IsElementActive(ITopologyElement element, ITopology topology)
        {
            bool isActive = true;
            element.IsActive = true;
            if (element is Field field)
            {
                foreach (var member in field.Members)
                {                    
                    if (topology.GetElementByGid(member, out ITopologyElement memberElement) && !IsElementActive(memberElement, topology))
                    {
                        isActive = false;
                        element.IsActive = false;
                    }
                }
            }
            else
            {
                foreach (var measurement in element.Measurements)
                {
                    if (measurement is DiscreteMeasurement discreteMeasurement)
                    {
                        if (Provider.Instance.CacheProvider.GetDiscreteValue(measurement.Id) == true)
                        {
                            isActive = false;
                            element.IsActive = false;
                        }
                        break;
                    }
                }
            }
            return isActive;
        }
    }
}
