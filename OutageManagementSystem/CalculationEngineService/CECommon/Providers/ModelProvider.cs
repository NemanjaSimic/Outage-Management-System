using CECommon.Interfaces;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Providers
{
    public class ModelProvider : IModelProvider
    {
        private TransactionFlag transactionFlag;
        private ILogger logger = LoggerWrapper.Instance;
        private IModelManager modelManager;
        private List<long> energySources;
        private List<long> transactionEnergySources;
        private Dictionary<long, IMeasurement> measurementModels;
        private Dictionary<long, ITopologyElement> elementModels;
        private Dictionary<long, List<long>> allElementConnections;
        private Dictionary<long, IMeasurement> transactionMeasurementModels;
        private Dictionary<long, ITopologyElement> transactionElementModels;
        private Dictionary<long, List<long>> transactionAllElementConnections;
        
        public ModelProvider(IModelManager modelManager)
        {
            this.modelManager = modelManager;
            transactionFlag = TransactionFlag.NoTransaction;
            energySources = this.modelManager.GetAllEnergySources();
            this.modelManager.GetAllModels(out elementModels, out measurementModels, out allElementConnections);
            Provider.Instance.ModelProvider = this;
        }
        public Dictionary<long, IMeasurement> GetMeasurementModels()
        {
            if (transactionFlag == TransactionFlag.NoTransaction)
            {
                return measurementModels;
            }
            else
            {
                return transactionMeasurementModels;
            }
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
        public bool PrepareForTransaction()
        {
            bool success = true;
            try
            {
                logger.LogInfo($"Topology manager prepare for transaction started.");
                transactionFlag = TransactionFlag.InTransaction;
                this.modelManager.PrepareTransaction();
                this.modelManager.GetAllModels(out transactionElementModels, out transactionMeasurementModels, out transactionAllElementConnections);
                transactionEnergySources = this.modelManager.GetAllEnergySources();
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
            measurementModels = transactionMeasurementModels;
            allElementConnections = transactionAllElementConnections;
            energySources = transactionEnergySources;
            transactionFlag = TransactionFlag.NoTransaction;
            logger.LogDebug("Model provider commited transaction successfully.");
        }
        public void RollbackTransaction()
        {
            transactionElementModels = null;
            transactionMeasurementModels = null;
            transactionAllElementConnections = null;
            transactionEnergySources = null;
            transactionFlag = TransactionFlag.NoTransaction;
            logger.LogDebug("Model provider rolled back topology.");
        }
    }
}
