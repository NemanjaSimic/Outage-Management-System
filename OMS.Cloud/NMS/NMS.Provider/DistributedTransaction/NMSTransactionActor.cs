using OMS.Cloud.TMS.DistributedTransactionActor;
using System;
using System.Threading.Tasks;

namespace OMS.Cloud.NMS.GdaProvider.DistributedTransaction
{
    public class NMSTransactionActor : TransactionActor
    { 
        private readonly NetworkModel networkModel = null;

        public NMSTransactionActor(NetworkModel networkModel)
        {
            this.networkModel = networkModel;
        }

        public override async Task<bool> Prepare()
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

        public override async Task Commit()
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

        public override async Task Rollback()
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
