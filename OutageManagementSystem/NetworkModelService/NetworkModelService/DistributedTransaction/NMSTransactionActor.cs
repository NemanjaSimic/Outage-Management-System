using Outage.Common;
using Outage.DistributedTransactionActor;
using System;

namespace Outage.NetworkModelService.DistributedTransaction
{
    public class NMSTransactionActor : TransactionActor
    { 
        protected static NetworkModel networkModel = null;

        public static NetworkModel NetworkModel
        {
            set
            {
                networkModel = value;
            }
        }

        public override bool Prepare()
        {
            bool success = false;

            try
            {
                success = networkModel.Prepare();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception caught in Prepare method on NMS Transaction actor. Exception: {ex.Message}", ex);
                success = false;
            }

            if(success)
            {
                Logger.LogInfo("Preparation on NMS Transaction actor SUCCESSFULLY finished.");
            }
            else
            {
                Logger.LogInfo("Preparation on NMS Transaction actor UNSUCCESSFULLY finished.");
            }

            return success;
        }

        public override void Commit()
        {
            try
            {
                networkModel.Commit(false);
                Logger.LogInfo("Commit on NMS Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            { 
                Logger.LogError($"Exception caught in Commit method on NMS Transaction actor. Exception: {ex.Message}", ex);
                Logger.LogInfo("Commit on NMS Transaction actor UNSUCCESSFULLY finished.");
            }
        }

        public override void Rollback()
        {
            try
            {
                networkModel.Rollback();
                Logger.LogInfo("Rollback on NMS Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception caught in Rollback method on NMS Transaction actor. Exception: {ex.Message}", ex);
                Logger.LogInfo("Rollback on NMS Transaction actor UNSUCCESSFULLY finished.");
            }
        }
    }
}
