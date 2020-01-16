using CECommon.Interfaces;
using CECommon.Model;
using NetworkModelServiceFunctions;
using Outage.Common.GDA;
using System.Collections.Generic;
using System.Linq;
using TopologyBuilder;
using Logger = Outage.Common.LoggerWrapper;

namespace Topology
{
    public class Topology
    {
        #region Fields
        private ITopologyBuilder topologyBuilder;
        private List<long> roots;

        public TopologyModel TopologyModel { get; private set; }
        #endregion

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

        public void InitializeTopology()
        {
            Logger.Instance.LogDebug("Initializing topology started.");
            CreateTopology();
            Logger.Instance.LogDebug("Initializing topology finished.");
        }

        public void UpdateTopology()
        {
            Logger.Instance.LogDebug("Updating topology started.");
            CreateTopology();
            Logger.Instance.LogDebug("Updating topology finished.");
        }

        public void CreateTopology()
        {
            Logger.Instance.LogDebug("Get all energy sources started.");
            roots = NMSElements.Instance.GetAllEnergySources();
            Logger.Instance.LogDebug("Get all energy sources finished.");


            Logger.Instance.LogDebug("Creating topology started.");
            if(roots.Count > 0)
            {
                TopologyModel = topologyBuilder.CreateGraphTopology(roots.First());
            }
            else
            {
                Logger.Instance.LogDebug("No roots found.");
            }
            Logger.Instance.LogDebug("Creating topology finished.");

            Logger.Instance.LogDebug("Initializing topology finished.");

        }
    }
}
