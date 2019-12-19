using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.DistributedTransactionActor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.NetworkModelService.DistributedTransaction
{
    public class NMSTransactionActor : TransactionActor, ITransactionActorContract
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
            }

            return success;
        }

        public override void Commit()
        {
            try
            {
                networkModel.Commit();
            }
            catch(Exception ex)
            {
                LoggerWrapper.Instance.LogError(ex.Message, ex);
            }
        }

        public override void Rollback()
        {
            try
            {
                networkModel.Rollback();
            }
            catch (Exception ex)
            {
                LoggerWrapper.Instance.LogError(ex.Message, ex);
            }
        }
    }
}
