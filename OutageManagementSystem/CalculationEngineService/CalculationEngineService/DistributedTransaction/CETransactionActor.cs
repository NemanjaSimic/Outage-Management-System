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
                logger.LogError($"Exception catched in Prepare method on CE Transaction actor. Exception: {ex.Message}", ex);
                success = false;
            }

            if (success)
            {
                logger.LogInfo("Preparation on CE Transaction actor SUCCESSFULLY finished.");
            }
            else
            {
                logger.LogInfo("Preparation on CE Transaction actor UNSUCCESSFULLY finished.");
            }

            return success;
        }

        public override void Commit()
        {
            try
            {
                //topologyModel.Commit();
                logger.LogInfo("Commit on CE Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                logger.LogFatal($"Exception catched in Commit method on CE Transaction actor. Exception: {ex.Message}", ex);
                logger.LogInfo("Commit on CE Transaction actor UNSUCCESSFULLY finished.");
            }
        }

        public override void Rollback()
        {
            try
            {
                //topologyModel.Commit();
                logger.LogInfo("Rollback on CE Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception catched in Rollback method on CE Transaction actor. Exception: {ex.Message}", ex);
                logger.LogInfo("Rollback on CE Transaction actor UNSUCCESSFULLY finished.");
            }
        }
    }
}
