using CECommon.Interfaces;
using CECommon.Models;
using Outage.Common;
using System;
using System.Collections.Generic;

namespace CECommon.Providers
{
    public class ModelProvider : IModelProvider
    {
        #region Fields
        private TransactionFlag transactionFlag;
        private ILogger logger = LoggerWrapper.Instance;
        private IModelManager modelManager;

        private List<long> energySources;
        private Dictionary<long, ITopologyElement> elementModels;
        private Dictionary<long, List<long>> allElementConnections;
        private HashSet<long> reclosers;

        private List<long> transactionEnergySources;
        private Dictionary<long, ITopologyElement> transactionElementModels;
        private Dictionary<long, List<long>> transactionAllElementConnections;
        private HashSet<long> transactionReclosers;
        #endregion
        public ModelProvider(IModelManager modelManager)
        {
            this.modelManager = modelManager;
            transactionFlag = TransactionFlag.NoTransaction;

            if(!modelManager.TryGetAllModelEntities(
                out elementModels,
                out allElementConnections,
                out reclosers,
                out energySources))
            {
                logger.LogFatal($"[Model provider] Failed to get all model entities.");
            }

            Provider.Instance.ModelProvider = this;
        }
        public Dictionary<long, ITopologyElement> GetElementModels()
        {
            if (transactionFlag == TransactionFlag.NoTransaction)
            {
                return elementModels;
            }
            else
            {
                return transactionElementModels;
            }
        }
        public Dictionary<long, List<long>> GetConnections()
        {
            if (transactionFlag == TransactionFlag.NoTransaction)
            {
                return allElementConnections;
            }
            else
            {
                return transactionAllElementConnections;
            }
        }
        public HashSet<long> GetReclosers()
        {
            if (transactionFlag == TransactionFlag.NoTransaction)
            {
                return reclosers;
            }
            else
            {
                return transactionReclosers;
            }
        }
        public List<long> GetEnergySources()
        {
            if (transactionFlag == TransactionFlag.NoTransaction)
            {
                return energySources;
            }
            else
            {
                return transactionEnergySources;
            }
        }
        public bool IsRecloser(long recloserGid)
        {
            return reclosers.Contains(recloserGid);
        }

        #region Distributed Transaction
        public bool PrepareForTransaction()
        {
            bool success = true;
            try
            {
                logger.LogInfo($"Topology manager prepare for transaction started.");
                transactionFlag = TransactionFlag.InTransaction;

                if (!modelManager.TryGetAllModelEntities(
                    out transactionElementModels,
                    out transactionAllElementConnections,
                    out transactionReclosers,
                    out transactionEnergySources))
                {
                    logger.LogError($"[Model provider] Failed to get all model entities in transaction.");
                    success = false;
                }
            }
            catch (Exception ex)
            {
                logger.LogInfo($"Model provider failed to prepare for transaction. Exception message: {ex.Message}");
                success = false;
            }
            return success;
        }
        public void CommitTransaction()
        {
            elementModels = transactionElementModels;
            allElementConnections = transactionAllElementConnections;
            energySources = transactionEnergySources;
            transactionFlag = TransactionFlag.NoTransaction;
            logger.LogDebug("Model provider commited transaction successfully.");
        }
        public void RollbackTransaction()
        {
            transactionElementModels = null;
            transactionAllElementConnections = null;
            transactionEnergySources = null;
            transactionFlag = TransactionFlag.NoTransaction;
            logger.LogDebug("Model provider rolled back successfully.");
        }
        #endregion
    }
}
