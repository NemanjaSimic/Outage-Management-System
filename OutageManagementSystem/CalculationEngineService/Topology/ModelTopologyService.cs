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
        private List<long> roots;
        #endregion

        public ModelTopologyService(ITopologyBuilder topologyBuilder)
        {
            this.topologyBuilder = topologyBuilder;
        }
        public List<ITopology> CreateTopology()
        {
            roots = Provider.Instance.ModelProvider.GetEnergySources();
            List<ITopology> topologyModel = new List<ITopology>();

            foreach (var rootElement in roots)
            {
                topologyModel.Add(topologyBuilder.CreateGraphTopology(rootElement));
            }

            return topologyModel;
        }     
    }
}
