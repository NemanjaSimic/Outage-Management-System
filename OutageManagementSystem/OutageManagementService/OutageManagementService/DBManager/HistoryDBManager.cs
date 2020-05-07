using OMSCommon.OutageDatabaseModel;
using Outage.Common;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace OutageManagementService.DBManager
{

    public class HistoryDBManager
    {
        private HashSet<long> unenergizedConsumers;
        private HashSet<long> openedSwitches;
        private UnitOfWork dbContext;
        private object syncObject = new object();

        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public HistoryDBManager()
        {
            unenergizedConsumers = new HashSet<long>();
            openedSwitches = new HashSet<long>();
            dbContext = new UnitOfWork();
        }
        public void OnSwitchClosed(long elementGid)
        {
            try
            {
                lock (syncObject)
                {
                    if (openedSwitches.Contains(elementGid))
                    {
                        dbContext.EquipmentHistoricalRepository.Add(new EquipmentHistorical() { EquipmentId = elementGid, OperationTime = DateTime.Now, DatabaseOperation = DatabaseOperation.DELETE });
                        openedSwitches.Remove(elementGid);
                    }

                    dbContext.Complete();
                }
            }
            catch (Exception e)
            {
                string message = "HistoryDBManager::OnSwitchClosed method => exception on Complete()";
                Logger.LogError(message, e);
                Console.WriteLine($"{message}, Message: {e.Message}, Inner Message: {e.InnerException.Message})");
            }
        }

        public void OnConsumersBlackedOut(List<long> consumers, long? outageId)
        {
            List<ConsumerHistorical> consumerHistoricals = new List<ConsumerHistorical>();
            try
            {
                foreach (var consumer in consumers)
                {
                    if (!unenergizedConsumers.Contains(consumer))
                    {

                        consumerHistoricals.Add(new ConsumerHistorical() { OutageId = outageId, ConsumerId = consumer, OperationTime = DateTime.Now, DatabaseOperation = DatabaseOperation.INSERT });
                        unenergizedConsumers.Add(consumer);
                    }
                }
                lock (syncObject)
                {
                    dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricals);
                    dbContext.Complete();
                }
            }
            catch (Exception e)
            {
                string message = "HistoryDBManager::OnConsumersBlackedOut method => exception on Complete()";
                Logger.LogError(message, e);
                Console.WriteLine($"{message}, Message: {e.Message}, Inner Message: {e.InnerException.Message})");
            }
        }

        public void OnSwitchOpened(long elementGid, long? outageId)
        {
            try
            {
                if (!openedSwitches.Contains(elementGid))
                {

                    dbContext.EquipmentHistoricalRepository.Add(new EquipmentHistorical() { OutageId = outageId, EquipmentId = elementGid, OperationTime = DateTime.Now, DatabaseOperation = DatabaseOperation.INSERT });
                    openedSwitches.Add(elementGid);
                }

                lock (syncObject)
                {
                    dbContext.Complete();
                }
            }
            catch (Exception e)
            {
                string message = "HistoryDBManager::OnSwitchOpened method => exception on Complete()";
                Logger.LogError(message, e);
                Console.WriteLine($"{message}, Message: {e.Message}, Inner Message: {e.InnerException.Message})");
            }
        }

        public void OnConsumersEnergized(HashSet<long> consumers)
        {
            List<ConsumerHistorical> consumerHistoricals = new List<ConsumerHistorical>();
            var changedConsumers = unenergizedConsumers.Intersect(consumers).ToList();

            foreach (var consumer in changedConsumers)
            {
                consumerHistoricals.Add(new ConsumerHistorical() { ConsumerId = consumer, OperationTime = DateTime.Now, DatabaseOperation = DatabaseOperation.DELETE });
            }

            try
            {
                lock (syncObject)
                {
                    foreach (var changed in changedConsumers)
                    {
                        if (unenergizedConsumers.Contains(changed))
                            unenergizedConsumers.Remove(changed);
                    }

                    dbContext.ConsumerHistoricalRepository.AddRange(consumerHistoricals);
                    dbContext.Complete();
                }
            }
            catch (Exception e)
            {
                string message = "HistoryDBManager::OnConsumersEnergized method => exception on Complete()";
                Logger.LogError(message, e);
                Console.WriteLine($"{message}, Message: {e.Message}, Inner Message: {e.InnerException.Message})");
            }
        }
    }
}
