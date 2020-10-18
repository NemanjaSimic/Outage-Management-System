using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.TmsContracts;
using OMS.Common.TmsContracts.Notifications;
using OMS.Common.WcfClient.TMS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CE.ModelProviderImplementation
{
    public class CeNetworkNotifyModelUpdate : INotifyNetworkModelUpdateContract
	{
        private readonly string baseLogString;

        private ICloudLogger logger;
        private ICloudLogger Logger => logger ?? (logger = CloudLoggerFactory.GetLogger());

        public CeNetworkNotifyModelUpdate()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            Logger.LogDebug($"{baseLogString} ctor initialized.");
        }
        public async Task<bool> Notify(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            logger.LogDebug($"{baseLogString } Notify method.");

            ITransactionEnlistmentContract transactionEnlistmentClient = TransactionEnlistmentClient.CreateClient();
            logger.LogDebug($"{baseLogString } Notify calling enlist.");
            bool success = await transactionEnlistmentClient.Enlist(DistributedTransactionNames.NetworkModelUpdateTransaction, MicroserviceNames.CeModelProviderService);

            if (success)
            {
                Logger.LogInformation($"{baseLogString} Notify => CE SUCCESSFULLY notified about network model update.");
            }
            else
            {
                Logger.LogInformation($"{baseLogString} Notify => CE UNSUCCESSFULLY notified about network model update.");
            }

            return success;
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
    }
}
