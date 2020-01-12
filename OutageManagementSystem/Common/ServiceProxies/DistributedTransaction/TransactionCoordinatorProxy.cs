using Outage.Common.ServiceContracts.DistributedTransaction;
using System;
using System.ServiceModel;

namespace Outage.Common.ServiceProxies.DistributedTransaction
{
    public class TransactionCoordinatorProxy : ClientBase<ITransactionCoordinatorContract>, ITransactionCoordinatorContract
    {
        public TransactionCoordinatorProxy(string endpointName)
            : base(endpointName)
        {
        }

        public void StartDistributedUpdate()
        {
            try
            {
                Channel.StartDistributedUpdate();
            }
            catch (Exception e)
            {

                string message = "Exception in StartDistributedUpdate() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }           
        }

        public void FinishDistributedUpdate(bool success)
        {
            try
            {
                Channel.FinishDistributedUpdate(success);
            }
            catch (Exception e)
            {
                string message = "Exception in FinishDistributedUpdate() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }   
        }
    }

    public class TransactionEnlistmentProxy : ClientBase<ITransactionEnlistmentContract>, ITransactionEnlistmentContract
    { 
        public TransactionEnlistmentProxy(string endpointName)
            : base(endpointName)
        { 
        }

        public bool Enlist(string actorName)
        {
            bool success;

            try
            {
                success = Channel.Enlist(actorName);
            }
            catch (Exception e)
            {
                string message = "Exception in Enlist() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
        }
    }
}
