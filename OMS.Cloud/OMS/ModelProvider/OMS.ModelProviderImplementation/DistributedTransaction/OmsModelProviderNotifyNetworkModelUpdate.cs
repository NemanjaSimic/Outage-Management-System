using Microsoft.ServiceFabric.Data;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.TmsContracts.Notifications;
using OMS.Common.WcfClient.TMS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.ModelProviderImplementation.DistributedTransaction
{
    public class OmsModelProviderNotifyNetworkModelUpdate : INotifyNetworkModelUpdateContract
    {
        private readonly string baseLogString;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public OmsModelProviderNotifyNetworkModelUpdate(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
        }

        #region INotifyNetworkModelUpdateContract
        public async Task<bool> Notify(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            //MODO: (ovaj servis nema nista u prepare pa zbog toga sledi komentar) logic of Notify for MicroserviceNames.OmsModelProviderService, if one should be required, would go here

            var transactionEnlistmentClient = TransactionEnlistmentClient.CreateClient();
            bool success = await transactionEnlistmentClient.Enlist(DistributedTransactionNames.NetworkModelUpdateTransaction, MicroserviceNames.OmsModelProviderService);

            if (success)
            {
                Logger.LogInformation($"{baseLogString} Notify => {MicroserviceNames.OmsModelProviderService} SUCCESSFULLY notified about network model update.");
            }
            else
            {
                Logger.LogInformation($"{baseLogString} Notify => {MicroserviceNames.OmsModelProviderService} UNSUCCESSFULLY notified about network model update.");
            }

            return success;
        }
        
        public Task<bool> IsAlive()
        {
            return Task.Run(() => true);
        }

        #endregion INotifyNetworkModelUpdateContract
    }
}
