using Outage.DistributedTransactionActor;
using Outage.SCADA.SCADAData.Repository;
using System;

namespace Outage.SCADA.SCADAService.DistributedTransaction
{
    public class SCADATransactionActor : TransactionActor
    {
        protected static SCADAModel scadaModel = null;

        public static SCADAModel SCADAModel
        {
            set
            {
                scadaModel = value;
            }
        }

        public override bool Prepare()
        {
            bool success = false;

            try
            {
                success = true;
                success = scadaModel.Prepare();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception caught in Prepare method on SCADA Transaction actor. Exception: {ex.Message}", ex);
                success = false;
            }

            if (success)
            {
                Logger.LogInfo("Preparation on SCADA Transaction actor SUCCESSFULLY finished.");
            }
            else
            {
                Logger.LogInfo("Preparation on SCADA Transaction actor UNSUCCESSFULLY finished.");
            }

            return success;
        }

        public override void Commit()
        {
            try
            {
                scadaModel.Commit();
                Logger.LogInfo("Commit on SCADA Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception caught in Commit method on SCADA Transaction actor. Exception: {ex.Message}", ex);
                Logger.LogInfo("Commit on SCADA Transaction actor UNSUCCESSFULLY finished.");
            }
        }

        public override void Rollback()
        {
            try
            {
                scadaModel.Rollback();
                Logger.LogInfo("Rollback on SCADA Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception caught in Rollback method on SCADA Transaction actor. Exception: {ex.Message}", ex);
                Logger.LogInfo("Rollback on SCADA Transaction actor UNSUCCESSFULLY finished.");
            }
        }
    }
}