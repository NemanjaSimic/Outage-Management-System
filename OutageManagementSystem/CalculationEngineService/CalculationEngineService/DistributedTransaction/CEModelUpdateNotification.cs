using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.DistributedTransaction;
using Outage.DistributedTransactionActor;
using System;
using System.Collections.Generic;

namespace CalculationEngineService.DistributedTransaction
{
    public class CEModelUpdateNotification : ModelUpdateNotification
    {
        public CEModelUpdateNotification()
            : base(EndpointNames.TransactionEnlistmentEndpoint, ServiceNames.CalculationEngineService)
        {
            proxyFactory = new ProxyFactory();
        }

        public override bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            bool success;

            TransactionManager.Intance.UpdateNotify(modelChanges);

            using (TransactionEnlistmentProxy transactionEnlistmentProxy = proxyFactory.CreateProxy<TransactionEnlistmentProxy, ITransactionEnlistmentContract>(EndpointNames.TransactionEnlistmentEndpoint))
            {
                if (transactionEnlistmentProxy == null)
                {
                    string message = "NotifyAboutUpdate => TransactionEnlistmentProxy is null.";
                    Logger.LogError(message);
                    throw new NullReferenceException(message);
                }

                success = transactionEnlistmentProxy.Enlist(ActorName);
            }

            Logger.LogInfo("Calculation Engine SUCCESSFULLY notified about network model update.");
            return success;
        }
    }
}
