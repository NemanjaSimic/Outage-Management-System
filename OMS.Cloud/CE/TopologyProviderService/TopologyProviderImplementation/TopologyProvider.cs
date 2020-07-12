using CECommon;
using CECommon.CeContrats;
using CECommon.Interface;
using CECommon.Interfaces;
using CECommon.Models;
using Common.CeContracts.TopologyProvider;
using OMS.Common.WcfClient.CE;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TopologyProviderImplementation
{
    public class TopologyProvider : ITopologyProviderService
    {
        #region Fields
        //private ILogger logger =  LoggerWrapper.Instance;
        private TransactionFlag transactionFlag;
        private ITopologyBuilder topologyBuilder;
        private ILoadFlow loadFlow;
        private List<ITopology> topology;
        private ModelProviderClient modelProviderClient = ModelProviderClient.CreateClient();
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
        public TopologyProvider(ITopologyBuilder topologyBuilder, ILoadFlow loadFlow)
        {
            //Provider.Instance.TopologyProvider = this;
            //Provider.Instance.MeasurementProvider.DiscreteMeasurementDelegate += DiscreteMeasurementDelegate;
            
            this.loadFlow = loadFlow;
            this.topologyBuilder = topologyBuilder;
            transactionFlag = TransactionFlag.NoTransaction;
            Topology = CreateTopology().Result; // bez await , da li ce praviti problem ?
            loadFlow.UpdateLoadFlow(Topology);
        }
        private async Task<List<ITopology>> CreateTopology()
        {
            List<long> roots = await modelProviderClient.GetEnergySources();
            List<ITopology> topologyModel = new List<ITopology>();

            foreach (var rootElement in roots)
            {
                topologyModel.Add(topologyBuilder.CreateGraphTopology(rootElement));
            }

            return topologyModel;
        }
        private List<ITopology> TransactionTopology { get; set; }
        public ProviderTopologyDelegate ProviderTopologyDelegate { get; set; }
        public ProviderTopologyConnectionDelegate ProviderTopologyConnectionDelegate{get; set;}
        public void DiscreteMeasurementDelegate()
        {
            loadFlow.UpdateLoadFlow(Topology);
            ProviderTopologyDelegate?.Invoke(Topology);
            ProviderTopologyConnectionDelegate?.Invoke(Topology);
        }
        public async Task<List<ITopology>> GetTopologies()
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
        //Veruj developeru na rec da je recloser, umoran sam
        public void ResetRecloser(long recloserGid)
        {
            try
            {
                foreach (var topology in Topology)
                {
                    if (topology.GetElementByGid(recloserGid, out ITopologyElement recloser))
                    {
                        ((Recloser)recloser).NumberOfTry = 0;
                        break;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        #region Distributed Transaction
        public void CommitTransaction()
        {
            transactionFlag = TransactionFlag.NoTransaction;
            this.loadFlow.UpdateLoadFlow(TransactionTopology);
            Topology = TransactionTopology;
            ProviderTopologyConnectionDelegate?.Invoke(Topology);
        }
        public async Task<bool> PrepareForTransaction()
        {
            bool success = true;
            try
            {
                //logger.LogDebug($"Topology provider preparing for transaction.");
                TransactionTopology = await CreateTopology();
                transactionFlag = TransactionFlag.InTransaction;
            }
            catch (Exception ex)
            {
                //logger.LogError($"Topology provider failed to prepare for transaction. Exception message: {ex.Message}");
                success = false;
            }
            return success;
        }
        public Task RollbackTransaction()
        {
            return Task.Run(() =>
            {
                TransactionTopology = null;
                transactionFlag = TransactionFlag.NoTransaction;
            });
        }
        #endregion
    }
}
