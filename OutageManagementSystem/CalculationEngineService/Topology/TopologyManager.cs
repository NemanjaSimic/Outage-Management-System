﻿using CECommon;
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
        private IWebTopologyBuilder webTopologyBuilder;
        private List<long> roots;

        public List<ITopology> TopologyModel { get; private set; }
        public List<ITopology> TransactionTopologyModel { get; private set; }
        #endregion

        private TopologyManager()
        {
            topologyBuilder = new GraphBuilder();
            webTopologyBuilder = new WebTopologyBuilder();
            TopologyModel = new List<ITopology>();
            TransactionTopologyModel = new List<ITopology>();
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
        private List<ITopology> CreateTopology()
        {
            logger.LogDebug("Get all energy sources started.");
            roots = NMSManager.Instance.GetAllEnergySources();
            logger.LogDebug("Get all energy sources finished.");

            List<ITopology> topologyModel = new List<ITopology>();

            foreach (var rootElement in roots)
            {
                topologyModel.Add(topologyBuilder.CreateGraphTopology(rootElement));
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
            TopologyModel = new List<ITopology>(TransactionTopologyModel);
            logger.LogDebug("TopologyManager commited transaction successfully.");
            using (var publisherProxy = new PublisherProxy(EndpointNames.PublisherEndpoint))
            {
                //Dok se ne sredi logika za vise root-ova na WEB-u
                ITopology topology = new TopologyModel();
                if (TopologyModel.Count > 0)
                {
                    topology = TopologyModel.First();
                }
                TopologyForUIMessage message = new TopologyForUIMessage(webTopologyBuilder.CreateTopologyForWeb(topology)); //privremeno resenje, dok se ne razradi logika
                CalcualtionEnginePublication publication = new CalcualtionEnginePublication(Topic.TOPOLOGY, message);
                publisherProxy.Publish(publication);
                logger.LogDebug("TopologyManager published new topology successfully.");
            }
        }

        public void RollbackTransaction()
        {
            TransactionTopologyModel = new List<ITopology>();
            logger.LogDebug("TopologyManager rolled back topology.");
        }

    }
}
