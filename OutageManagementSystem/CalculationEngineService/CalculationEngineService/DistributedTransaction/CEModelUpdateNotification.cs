using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceProxies.DistributedTransaction;
using Outage.DistributedTransactionActor;
using System;
using System.Collections.Generic;

namespace CalculationEngineService.DistributedTransaction
{
    public class CEModelUpdateNotification : ModelUpdateNotification
    {
        //public static CalculationEngineService calculationEngineService = null;

        public CEModelUpdateNotification()
            : base(EndpointNames.TransactionEnlistmentEndpoint, ServiceNames.CalculationEngineService)
        {
        }

        public override bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            TransactionManager.Intance.UpdateNotify(modelChanges);

            using (TransactionEnlistmentProxy transactionEnlistmentProxy = GetTransactionEnlistmentProxy())
            {
                if(transactionEnlistmentProxy != null)
                {
                    transactionEnlistmentProxy.Enlist(ActorName);
                }
                else
                {
                    string message = "TransactionEnlistmentProxy is null.";
                    Logger.LogWarn(message);
                    //TODO: retry logic?
                    throw new NullReferenceException(message);
                }
            }


            Logger.LogInfo("Calculation Engine SUCCESSFULLY notified about network model update.");
            return true;
        }
    }
}
