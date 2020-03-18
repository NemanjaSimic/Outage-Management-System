using Outage.DistributedTransactionActor;
using Outage.SCADA.SCADAData.Repository;
using System;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADAService.DistributedTransaction
{
    public class SCADATransactionActor : TransactionActor
    {
        #region Static Members

        protected static SCADAModel scadaModel = null;

        public static SCADAModel SCADAModel
        {
            set
            {
                if (scadaModel == null)
                {
                    scadaModel = value;
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
                success = SCADATransactionActor.scadaModel.Prepare();
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

        public override async Task Commit()
        {
            try
            {
                SCADATransactionActor.scadaModel.Commit();
                Logger.LogInfo("Commit on SCADA Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception caught in Commit method on SCADA Transaction actor. Exception: {ex.Message}", ex);
                Logger.LogInfo("Commit on SCADA Transaction actor UNSUCCESSFULLY finished.");
            }
        }

        public override async Task Rollback()
        {
            try
            {
                SCADATransactionActor.scadaModel.Rollback();
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