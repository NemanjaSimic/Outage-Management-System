using Outage.DistributedTransactionActor;
using System;

namespace CalculationEngineService.DistributedTransaction
{
    public class CETransactionActor : TransactionActor
    {
        //protected static TopologyModel topologyModel = null;

        //public static TopologyModel TopologyModel
        //{
        //    set
        //    {
        //        topologyModel = value;
        //    }
        //}

        public override bool Prepare()
        {
            bool success = false;

            try
            {
                success = true;
                //success = topologyModel.Prepare();
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

        public override void Commit()
        {
            try
            {
                //topologyModel.Commit();
                Logger.LogInfo("Commit on CE Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception caught in Commit method on CE Transaction actor. Exception: {ex.Message}", ex);
                Logger.LogInfo("Commit on CE Transaction actor UNSUCCESSFULLY finished.");
            }
        }

        public override void Rollback()
        {
            try
            {
                //topologyModel.Commit();
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
