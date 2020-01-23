using CECommon;
using CECommon.Interfaces;
using CECommon.Model;
using NetworkModelServiceFunctions;
using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.PubSub.CalculationEngineDataContract;
using Outage.Common.ServiceProxies.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using TopologyBuilder;

namespace Topology
{
    public class TopologyManager
    {
        #region Fields
        ILogger logger = LoggerWrapper.Instance;
        private ITopologyBuilder topologyBuilder;
        private List<long> roots;

        public TopologyModel TopologyModel { get; private set; }
        public TopologyModel TransactionTopologyModel { get; private set; }
        #endregion

        private TopologyManager()
        {
            topologyBuilder = new GraphBuilder();
        }

        #region Singleton
        private static TopologyManager instance;
        private static object syncObj = new object();

        public static TopologyManager Instance
        {
            get 
            {
                lock (syncObj)
                {
                    if (instance == null)
                    {
                        instance = new TopologyManager();
                    }
                    return instance;
                }
            }
        }
        #endregion

        public void InitializeTopology()
        {
            logger.LogDebug("Initializing topology started.");
            TopologyModel = CreateTopology();
            logger.LogDebug("Initializing topology finished.");
        }
        private TopologyModel CreateTopology()
        {
            logger.LogDebug("Get all energy sources started.");
            roots = NMSManager.Instance.GetAllEnergySources();
            logger.LogDebug("Get all energy sources finished.");

            TopologyModel topologyModel = new TopologyModel();

            if (roots.Count > 0)
            {
                topologyModel = topologyBuilder.CreateGraphTopology(roots.First());
            }
            return topologyModel;
        }

        public bool PrepareForTransaction()
        {
            bool success = true;
            try
            {
                logger.LogInfo($"Topology manager prepare for transaction started.");
                TransactionTopologyModel = CreateTopology();
            }
            catch (Exception ex)
            {
                logger.LogInfo($"Topology manager failed to prepare for transaction. Exception message: {ex.Message}");
                success = false;
            }
            return success;
        }

        public void CommitTransaction()
        {
            TopologyModel = TransactionTopologyModel;
            logger.LogDebug("TopologyManager commited transaction successfully.");
            using (var publisherProxy = new PublisherProxy(EndpointNames.PublisherEndpoint))
            {
                TopologyForUIMessage message = new TopologyForUIMessage(TopologyModel.UIModel);
                CalcualtionEnginePublication publication = new CalcualtionEnginePublication(Topic.TOPOLOGY, message);
                publisherProxy.Publish(publication);
                logger.LogDebug("TopologyManager published new topology successfully.");
            }
        }

        public void RollbackTransaction()
        {
            TransactionTopologyModel = null;
            logger.LogDebug("TopologyManager rolled back topology.");
        }

    }
}
