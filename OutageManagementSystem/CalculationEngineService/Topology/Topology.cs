using CECommon.Interfaces;
using CECommon.Model;
using NetworkModelServiceFunctions;
using Outage.Common;
using Outage.Common.GDA;
using System.Collections.Generic;
using System.Linq;
using TopologyBuilder;

namespace Topology
{
    public class Topology
    {
        #region Fields
        ILogger logger = LoggerWrapper.Instance;
        private ITopologyBuilder topologyBuilder;
        private List<long> roots;
        private Dictionary<long, ResourceDescription> modelEntities;
        public TopologyModel TopologyModel { get; private set; }
        #endregion

        private Topology()
        {
            topologyBuilder = new GraphBuilder();
            modelEntities = new Dictionary<long, ResourceDescription>();
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

        public void InitializeTopology()
        {
            logger.LogDebug("Initializing topology started.");

            logger.LogDebug("Retrieve all elements started.");
            modelEntities = GDAModelHelper.Instance.RetrieveAllElements();
            logger.LogDebug("Retrieve all elements finished.");

            logger.LogDebug("Get all energy sources started.");
            roots = GDAModelHelper.Instance.GetAllEnergySources();
            logger.LogDebug("Get all energy sources finished.");

            logger.LogDebug("Creating topology started.");
            if(roots.Count > 0)
            {
                TopologyModel = topologyBuilder.CreateGraphTopology(roots.First());
            }
            else
            {
                logger.LogDebug("No roots found.");
            }
            logger.LogDebug("Creating topology finished.");

            logger.LogDebug("Initializing topology finished.");
        }
    }
}
