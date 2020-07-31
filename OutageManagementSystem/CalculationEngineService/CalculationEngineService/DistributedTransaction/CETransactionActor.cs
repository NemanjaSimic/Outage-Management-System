using Outage.DistributedTransactionActor;
using System;
using System.Threading.Tasks;

namespace CalculationEngineService.DistributedTransaction
{
    public class CETransactionActor : TransactionActor
    {
        public override async Task<bool> Prepare()
        {
            bool success = false;

            try
            {
                success = TransactionManager.Intance.Prepare();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception caught in Prepare method on CE Transaction actor. Exception: {ex.Message}", ex);
                success = false;
            }

            if (success)
            {
                Logger.LogInfo("Preparation on CE Transaction actor SUCCESSFULLY finished.");
            }
            else
            {
                Logger.LogInfo("Preparation on CE Transaction actor UNSUCCESSFULLY finished.");
            }

            return success;
        }

        public override async Task Commit()
        {
            try
            {
                TransactionManager.Intance.CommitTransaction();
                Logger.LogInfo("Commit on CE Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception caught in Commit method on CE Transaction actor. Exception: {ex.Message}", ex);
                Logger.LogInfo("Commit on CE Transaction actor UNSUCCESSFULLY finished.");
            }
        }

        public override async Task Rollback()
        {
            try
            {
                TransactionManager.Intance.RollbackTransaction();
                Logger.LogInfo("Rollback on CE Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception caught in Rollback method on CE Transaction actor. Exception: {ex.Message}", ex);
                Logger.LogInfo("Rollback on CE Transaction actor UNSUCCESSFULLY finished.");
            }
        }
    }
}
