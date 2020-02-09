using Outage.Common.ServiceContracts.DistributedTransaction;
using System;
using System.ServiceModel;

namespace Outage.Common.ServiceProxies.DistributedTransaction
{
    public class TransactionActorProxy : BaseProxy<ITransactionActorContract>, ITransactionActorContract
    {
        public TransactionActorProxy(string endpointName)
            : base(endpointName)
        {
        }

        public bool Prepare()
        {
            bool success;

            try
            {
                success = Channel.Prepare();
            }
            catch (Exception e)
            {
                string message = "Exception in Prepare() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
            
        }

        public void Commit()
        {
            try
            {
                Channel.Commit();
            }
            catch (Exception e)
            {
                string message = "Exception in Commit() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }         
        }

        public void Rollback()
        {
            try
            {
                Channel.Rollback();
            }
            catch (Exception e)
            {

                string message = "Exception in Rollback() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            } 
        }
    }
}
