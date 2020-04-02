using OMS.Cloud.TMS.DistributedTransactionActor;
using OMS.Common.DistributedTransactionContracts;
using System;
using System.Threading.Tasks;

namespace OMS.Cloud.SCADA.ModelProviderService.DistributedTransaction
{
    internal class ScadaTransactionActor : TransactionActor
    {
        private readonly ITransactionActorContract scadaTransactionActor;

        public ScadaTransactionActor(ITransactionActorContract scadaTransactionActor)
        {
            this.scadaTransactionActor = scadaTransactionActor;
        }

        public override async Task<bool> Prepare()
        {
            bool success;

            try
            {
                success = await this.scadaTransactionActor.Prepare();
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
                await this.scadaTransactionActor.Commit();
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
                await this.scadaTransactionActor.Rollback();
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
