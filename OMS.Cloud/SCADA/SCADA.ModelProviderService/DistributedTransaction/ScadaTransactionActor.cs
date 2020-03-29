using Outage.DistributedTransactionActor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.Cloud.SCADA.ModelProviderService.DistributedTransaction
{
    internal class ScadaTransactionActor : TransactionActor
    {
        private readonly ScadaModel scadaModel;

        public ScadaTransactionActor(ScadaModel scadaModel)
        {
            this.scadaModel = scadaModel;
        }

        public override async Task<bool> Prepare()
        {
            bool success;

            try
            {
                success = await this.scadaModel.Prepare();
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
                this.scadaModel.Commit();
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
                this.scadaModel.Rollback();
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
