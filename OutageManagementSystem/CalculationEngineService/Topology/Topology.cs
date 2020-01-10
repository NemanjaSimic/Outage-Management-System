using CECommon.Interfaces;
using CECommon.Model;
using NetworkModelServiceFunctions;
using Outage.Common;
using System.Collections.Generic;
using System.Linq;
using TopologyBuilder;

namespace Topology
{
    public class Topology
    {
        ILogger logger = LoggerWrapper.Instance;
        private ITopologyBuilder topologyBuilder;
        private List<long> roots;
        public TopologyModel TopologyModel { get; private set; }

        private Topology()
        {
            topologyBuilder = new GraphBuilder();
        }

        #region Singleton
        private static Topology instance;
        private static object syncObj = new object();

        public static Topology Instance
        {
            get 
            {
                lock (syncObj)
                {
                    if (instance == null)
                    {
                        instance = new Topology();
                    }
                    return instance;
                }
            }
        }
        #endregion

        public void UpdateTopology()
        {
            logger.LogDebug("Update topology started.");
            
            logger.LogDebug("Get all energy sources started.");
            roots = GDAModelHelper.Instance.GetAllEnergySources();
            logger.LogDebug("Get all energy sources finished.");

            logger.LogDebug("Retrieve all elements started.");
            GDAModelHelper.Instance.RetrieveAllElements();
            logger.LogDebug("Retrieve all elements finished.");

            TopologyModel = topologyBuilder.CreateGraphTopology(roots.First());
            logger.LogDebug("Update topology finished.");
        }
    }
}
