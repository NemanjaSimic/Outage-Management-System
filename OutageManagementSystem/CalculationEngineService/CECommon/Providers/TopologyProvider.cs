using CECommon.Interfaces;
using Outage.Common;
using System;
using System.Collections.Generic;

namespace CECommon.Providers
{
    public class TopologyProvider : ITopologyProvider
    {
        #region Fields
        private ILogger logger =  LoggerWrapper.Instance;
        private TransactionFlag transactionFlag;
        private ILoadFlow voltageFlow;
        private List<ITopology> topology;
        private IModelTopologyService modelTopologyServis;
        #endregion
        private List<ITopology> Topology
        {
            get { return topology; }
            set
            {
                topology = value;
                ProviderTopologyDelegate?.Invoke(Topology);
            }
        }
        private List<ITopology> TransactionTopology { get; set; }
        public ProviderTopologyDelegate ProviderTopologyDelegate { get; set; }
        public ProviderTopologyConnectionDelegate ProviderTopologyConnectionDelegate{get; set;}
        public TopologyProvider(IModelTopologyService modelTopologyServis, ILoadFlow voltageFlow)
        {
            this.voltageFlow = voltageFlow;
            this.modelTopologyServis = modelTopologyServis;
            transactionFlag = TransactionFlag.NoTransaction;
            Topology = this.modelTopologyServis.CreateTopology();
            Provider.Instance.MeasurementProvider.DiscreteMeasurementDelegate += DiscreteMeasurementDelegate;
            Provider.Instance.TopologyProvider = this;
        }
        public void DiscreteMeasurementDelegate(List<long> elementGids)
        {
            voltageFlow.UpdateLoadFlow(Topology);
            ProviderTopologyDelegate?.Invoke(Topology);
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

        #region Distributed Transaction
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
                logger.LogDebug($"Topology provider preparing for transaction.");
                TransactionTopology = modelTopologyServis.CreateTopology();
                transactionFlag = TransactionFlag.InTransaction;
            }
            catch (Exception ex)
            {
                logger.LogError($"Topology provider failed to prepare for transaction. Exception message: {ex.Message}");
                success = false;
            }
            return success;
        }
        public void RollbackTransaction()
        {
            TransactionTopology = null;
            transactionFlag = TransactionFlag.NoTransaction;
        }
        #endregion
    }
}
