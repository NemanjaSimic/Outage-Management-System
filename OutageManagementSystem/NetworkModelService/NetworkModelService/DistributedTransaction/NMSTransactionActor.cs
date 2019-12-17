using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts;
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
        private static readonly string transactionCoordinatorEndpoint = "TransactionCoordinatorEndpoint";
        protected static NetworkModel networkModel = null;

        public static NetworkModel NetworkModel
        {
            set
            {
                networkModel = value;
            }
        }

        public NMSTransactionActor() 
            : base(transactionCoordinatorEndpoint, ServiceHostNames.NetworkModelService)
        {
        }

        public override bool EnlistUpdateDelta(Delta delta)
        {
            return base.EnlistUpdateDelta(delta);
        }

        public override void Prepare()
        {
            try
            {
                using (CoordinatorProxy)
                {
                    CoordinatorProxy.FinishDistributedUpdate(ActorName, true);
                }
            }
            catch (Exception ex)
            {
                LoggerWrapper.Instance.LogError(ex.Message, ex);
            }
        }

        public override bool Commit()
        {
            bool success = false;

            try
            {
                success = networkModel.Commit();
            }
            catch(Exception ex)
            {
                LoggerWrapper.Instance.LogError(ex.Message, ex);
            }

            return success;
        }

        public override bool Rollback()
        {
            bool success = false;

            try
            {
                success = networkModel.Commit();
            }
            catch (Exception ex)
            {
                LoggerWrapper.Instance.LogError(ex.Message, ex);
            }

            return success;
        }
    }
}
