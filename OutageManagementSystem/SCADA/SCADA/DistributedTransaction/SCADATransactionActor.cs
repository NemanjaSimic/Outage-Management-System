using Outage.DistributedTransactionActor;
using System;

namespace SCADA_Service.DistributedTransaction
{
    [Obsolete]
    public class SCADATransactionActor : TransactionActor
    {
        //protected static SCADAService scadaService = null;

        //public static SCADAService SCADAService
        //{
        //    set
        //    {
        //        scadaService = value;
        //    }
        //}

        public override bool Prepare()
        {
            bool success = false;

            try
            {
                success = true;
                //success = scadaService.Prepare();
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception catched in Prepare method on SCADA Transaction actor. Exception: {ex.Message}", ex);
                success = false;
            }

            if (success)
            {
                logger.LogInfo("Preparation on SCADA Transaction actor SUCCESSFULLY finished.");
            }
            else
            {
                logger.LogInfo("Preparation on SCADA Transaction actor UNSUCCESSFULLY finished.");
            }

            return success;
        }

        public override void Commit()
        {
            try
            {
                //scadaService.Commit();
                logger.LogInfo("Commit on SCADA Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception catched in Commit method on SCADA Transaction actor. Exception: {ex.Message}", ex);
                logger.LogInfo("Commit on SCADA Transaction actor UNSUCCESSFULLY finished.");
            }
        }

        public override void Rollback()
        {
            try
            {
                //scadaService.Commit();
                logger.LogInfo("Rollback on SCADA Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception catched in Rollback method on SCADA Transaction actor. Exception: {ex.Message}", ex);
                logger.LogInfo("Rollback on SCADA Transaction actor UNSUCCESSFULLY finished.");
            }
        }
    }
}