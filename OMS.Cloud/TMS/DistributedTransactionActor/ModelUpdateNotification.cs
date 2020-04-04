using OMS.Common.Cloud.WcfServiceFabricClients.TMS;
using OMS.Common.DistributedTransactionContracts;
using OMS.Common.NmsContracts.GDA;
using Outage.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Cloud.TMS.DistributedTransactionActor
{
    public abstract class ModelUpdateNotification : IModelUpdateNotificationContract
    {
        protected ILogger Logger = LoggerWrapper.Instance;
        protected TransactionEnlistmentClient transactionEnlistmentClient;

        public string TransactionEnlistmentEndpointName { get; private set; }
        public string ActorName { get; set; }

        protected ModelUpdateNotification(string transactionEnlistmentEndpointName="", string actorName="")
        {
            TransactionEnlistmentEndpointName = transactionEnlistmentEndpointName;
            ActorName = actorName;

            transactionEnlistmentClient = TransactionEnlistmentClient.CreateClient();
        }

        public abstract Task<bool> NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges);
    }
}
