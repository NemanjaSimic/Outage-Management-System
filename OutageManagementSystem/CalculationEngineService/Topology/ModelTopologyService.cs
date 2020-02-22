using CECommon.Interfaces;
using CECommon.Providers;
using Outage.Common;
using System.Collections.Generic;

namespace Topology
{
    public class ModelTopologyService : IModelTopologyService
    {
        #region Fields
        private readonly ILogger logger = LoggerWrapper.Instance;
        private ITopologyBuilder topologyBuilder;
        private IVoltageFlow voltageFlow;
        private List<long> roots;
        #endregion

        public ModelTopologyService(ITopologyBuilder topologyBuilder, IVoltageFlow voltageFlow)
        {
            this.topologyBuilder = topologyBuilder;
            this.voltageFlow = voltageFlow;
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
            voltageFlow.UpdateLoadFlow(topologyModel);

            return topologyModel;
        }

       
    }
}
