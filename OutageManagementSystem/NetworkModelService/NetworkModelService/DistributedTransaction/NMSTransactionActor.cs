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
                LoggerWrapper.Instance.LogError(ex.Message, ex);
                //TODO: 
                //LoggerWrapper.Instance.LogInfo();
            }

            return success;
        }

        public override void Commit()
        {
            try
            {
                networkModel.Commit();
                //TODO: 
                //LoggerWrapper.Instance.LogInfo();
            }
            catch (Exception ex)
            {
                LoggerWrapper.Instance.LogError(ex.Message, ex);
            }
        }

        public override void Rollback()
        {
            try
            {
                networkModel.Rollback();
                //TODO: 
                //LoggerWrapper.Instance.LogInfo();
            }
            catch (Exception ex)
            {
                LoggerWrapper.Instance.LogError(ex.Message, ex);
            }
        }
    }
}
