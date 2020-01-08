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
                logger.LogError($"Exception catched in Prepare method on NMS Transaction actor. Exception: {ex.Message}", ex);
                success = false;
            }

            if(success)
            {
                logger.LogInfo("Preparation on NMS Transaction actor SUCCESSFULLY finished.");
            }
            else
            {
                logger.LogInfo("Preparation on NMS Transaction actor UNSUCCESSFULLY finished.");
            }

            return success;
        }

        public override void Commit()
        {
            try
            {
                networkModel.Commit(false);
                logger.LogInfo("Commit on NMS Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            { 
                logger.LogError($"Exception catched in Commit method on NMS Transaction actor. Exception: {ex.Message}", ex);
                logger.LogInfo("Commit on NMS Transaction actor UNSUCCESSFULLY finished.");
            }
        }

        public override void Rollback()
        {
            try
            {
                networkModel.Rollback();
                logger.LogInfo("Rollback on NMS Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception catched in Rollback method on NMS Transaction actor. Exception: {ex.Message}", ex);
                logger.LogInfo("Rollback on NMS Transaction actor UNSUCCESSFULLY finished.");
            }
        }
    }
}
