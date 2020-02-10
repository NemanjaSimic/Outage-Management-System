using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceProxies.DistributedTransaction;
using Outage.DistributedTransactionActor;
using Outage.SCADA.SCADAData.Repository;
using System;
using System.Collections.Generic;

namespace Outage.SCADA.SCADAService.DistributedTransaction
{
    public class SCADAModelUpdateNotification : ModelUpdateNotification
    {
        #region Static Members

        protected static SCADAModel scadaModel = null;

        public static SCADAModel SCADAModel
        {
            set
            {
                if (scadaModel == null)
                {
                    scadaModel = value;
                }
            }
        }

        #endregion

        public SCADAModelUpdateNotification()
            : base(EndpointNames.TransactionEnlistmentEndpoint, ServiceNames.SCADAService)
        {
        }

        public override bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            bool success = SCADAModelUpdateNotification.scadaModel.Notify(modelChanges);

            using (TransactionEnlistmentProxy transactionEnlistmentProxy = proxyFactory.CreateProxy<TransactionEnlistmentProxy, ITransactionEnlistmentContract>(EndpointNames.TransactionEnlistmentEndpoint))
            {
                if (transactionEnlistmentProxy == null)
                {
                    string message = "TransactionEnlistmentProxy is null.";
                    Logger.LogError(message);
                    throw new NullReferenceException(message);
                }

                success = transactionEnlistmentProxy.Enlist(ActorName);
            }


            if (success)
            {
                Logger.LogInfo("SCADA SUCCESSFULLY notified about network model update.");
            }
            else
            {
                Logger.LogInfo("SCADA UNSUCCESSFULLY notified about network model update.");
            }

            return success;
        }
    }
}