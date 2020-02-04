using CECommon.Interfaces;
using Outage.Common;
using System;
using System.Collections.Generic;

namespace CECommon.Providers
{
    public class TopologyProvider : ITopologyProvider
    {
        //private ILogger logger = new LoggerWrapper.Instance;
        private TransactionFlag transactionFlag;
        private List<ITopology> topology;
        private IModelTopologyServis modelTopologyServis;
        private List<ITopology> Topology
        {
            get { return topology; }
            set
            {
                topology = value;
                ProviderTopologyDelegate?.Invoke(topology);
            }
        }
        private List<ITopology> TransactionTopology { get; set; }
        public ProviderTopologyDelegate ProviderTopologyDelegate { get; set; }
        public ProviderTopologyConnectionDelegate ProviderTopologyConnectionDelegate{get; set;}
        public TopologyProvider(IModelTopologyServis modelTopologyServis)
        {
            this.modelTopologyServis = modelTopologyServis;
            transactionFlag = TransactionFlag.NoTransaction;
            Topology = this.modelTopologyServis.CreateTopology();
            Provider.Instance.CacheProvider.DiscreteMeasurementDelegate += DiscreteMeasurementDelegate;
            Provider.Instance.TopologyProvider = this;
        }  
        
        public void DiscreteMeasurementDelegate(long meausrementGid)
        {
            Topology = this.modelTopologyServis.UpdateLoadFlow(meausrementGid, Topology);
        }

        public List<ITopology> GetTopologies()
        {
            if (transactionFlag == TransactionFlag.NoTransaction)
            {
                return Topology;
            }
            else
            {
                return TransactionTopology;
            }
        }
        public void CommitTransaction()
        {
            Topology = TransactionTopology;
            transactionFlag = TransactionFlag.NoTransaction;
            ProviderTopologyConnectionDelegate?.Invoke(Topology);
        }
        public bool PrepareForTransaction()
        {
            bool success = true;
            try
            {
                //logger.LogInfo($"Topology manager prepare for transaction started.");
                TransactionTopology = this.modelTopologyServis.CreateTopology();
                transactionFlag = TransactionFlag.InTransaction;
            }
            catch (Exception ex)
            {
                //logger.LogInfo($"Model provider failed to prepare for transaction. Exception message: {ex.Message}");
                success = false;
            }
            return success;
        }
        public void RollbackTransaction()
        {
            TransactionTopology = null;
            transactionFlag = TransactionFlag.NoTransaction;
        }

        public bool IsElementRemote(long elementGid)
        {
            bool isRemote = false;
            foreach (var topology in Topology)
            {
                if (topology.TopologyElements.TryGetValue(elementGid, out ITopologyElement element))
                {
                    isRemote = element.IsRemote;
                    break;
                }
            }
            return isRemote;
        }
            
    }
}
