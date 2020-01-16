using CECommon;
using CECommon.Interfaces;
using CECommon.Model;
using NetworkModelServiceFunctions;
using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using TopologyBuilder;
using Logger = Outage.Common.LoggerWrapper;

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
            TopologyModel = CreateTopology(TransactionFlag.NoTransaction);
            logger.LogDebug("Initializing topology finished.");
        }
        private TopologyModel CreateTopology(TransactionFlag flag)
        {
            logger.LogDebug("Get all energy sources started.");
            roots = NMSManager.Instance.GetAllEnergySources(flag);
            logger.LogDebug("Get all energy sources finished.");

            TopologyModel topologyModel = new TopologyModel();

            if (roots.Count > 0)
            {
                topologyModel = topologyBuilder.CreateGraphTopology(roots.First(), flag);
            }
            return topologyModel;
        }

        public bool PrepareForTransaction()
        {
            bool success = true;
            try
            {
                Logger.Instance.LogInfo($"Topology manager prepare for transaction started.");
                TransactionTopologyModel = CreateTopology(TransactionFlag.InTransaction);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogInfo($"Topology manager failed to prepare for transaction. Exception message: {ex.Message}");
                success = false;
            }
            return success;
        }

        public void CommitTransaction()
        {
            TopologyModel = TransactionTopologyModel;
        }

        public void RollbackTransaction()
        {
            TransactionTopologyModel = null;
        }

    }
}
