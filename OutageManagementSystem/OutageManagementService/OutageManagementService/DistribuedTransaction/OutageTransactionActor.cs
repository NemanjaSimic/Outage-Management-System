using Outage.DistributedTransactionActor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutageManagementService.DistribuedTransaction
{
   

    public class OutageTransactionActor : TransactionActor
    {
        #region Static Members
        protected static OutageModel outageModel = null;
        
        public static OutageModel OutageModel
        {
            set
            {
                if(outageModel == null)
                {
                    outageModel = value;
                }
            }
        }
        #endregion

        public override async Task<bool> Prepare()
        {
            bool success = false;

            try
            {
                success = true;
                success = OutageTransactionActor.outageModel.Prepare();
            }
            catch(Exception ex)
            {
                Logger.LogError($"Exception caught in Prepare method on Outage Transaction actor. Exception: {ex.Message}", ex);
                success = false;
            }

            if (success)
            {
                Logger.LogInfo("Preparation on Outage Transaction actor SUCCESSFULLY finished.");
            }
            else
            {
                Logger.LogInfo("Preparation on Outage Transaction actor UNSUCCESSFULLY finished.");
            }

            return success;
        }

        public override async Task Commit()
        {
            try
            {
                OutageTransactionActor.outageModel.Commit();
                Logger.LogInfo("Commit on Outage Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception caught in Commit method on Outage Transaction actor. Exception: {ex.Message}", ex);
                Logger.LogInfo("Commit on Outage Transaction actor UNSUCCESSFULLY finished.");
            }
        }

        public override async Task Rollback()
        {
            try
            {
                OutageTransactionActor.outageModel.Rollback();
                Logger.LogInfo("Rollback on Outage Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception caught in Rollback method on Outage Transaction actor. Exception: {ex.Message}", ex);
                Logger.LogInfo("Rollback on Outage actor UNSUCCESSFULLY finished.");
            }
        }
    }
}
