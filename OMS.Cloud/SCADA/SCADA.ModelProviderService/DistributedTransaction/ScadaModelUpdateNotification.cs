using OMS.Cloud.TMS.DistributedTransactionActor;
using OMS.Common.Cloud.WcfServiceFabricClients.TMS;
using OMS.Common.DistributedTransactionContracts;
using OMS.Common.NmsContracts.GDA;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Cloud.SCADA.ModelProviderService.DistributedTransaction
{
    internal class ScadaModelUpdateNotification : ModelUpdateNotification
    {
        private readonly IModelUpdateNotificationContract modelUpdateNotification;

        public ScadaModelUpdateNotification(IModelUpdateNotificationContract modelUpdateNotification)
            //: base(EndpointNames.TransactionEnlistmentEndpoint, ServiceNames.SCADAService)
        {
            this.modelUpdateNotification = modelUpdateNotification;
        }

        public override async Task<bool> NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            bool result = await this.modelUpdateNotification.NotifyAboutUpdate(modelChanges);
            
            if (!result)
            {
                return false;
            }

            if (this.transactionEnlistmentClient == null)
            {
                string message = "TransactionEnlistmentClient is null.";
                Logger.LogError(message);

                this.transactionEnlistmentClient = TransactionEnlistmentClient.CreateClient();
            }

            bool success = this.transactionEnlistmentClient.Enlist(ActorName);

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
