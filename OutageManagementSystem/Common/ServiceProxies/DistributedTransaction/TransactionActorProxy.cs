using Outage.Common.ServiceContracts.DistributedTransaction;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Outage.Common.ServiceProxies.DistributedTransaction
{
    public class TransactionActorProxy : BaseProxy<ITransactionActorContract>, ITransactionActorContract
    {
        public TransactionActorProxy(string endpointName)
            : base(endpointName)
        {
        }

        public async Task<bool> Prepare()
        {
            bool success;

            try
            {
                success = await Channel.Prepare();
            }
            catch (Exception e)
            {
                string message = "Exception in Prepare() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
            
        }

        public async Task Commit()
        {
            try
            {
                await Channel.Commit();
            }
            catch (Exception e)
            {
                string message = "Exception in Commit() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }         
        }

        public async Task Rollback()
        {
            try
            {
                await Channel.Rollback();
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
