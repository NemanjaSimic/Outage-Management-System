using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.TmsContracts;
using OMS.Common.TmsContracts.Notifications;
using OMS.Common.WcfClient.TMS;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace SCADA.ModelProviderImplementation.DistributedTransaction
{
    public class ScadaNotifyNetworkModelUpdate : INotifyNetworkModelUpdateContract
    {
        private readonly string baseLogString;
        private readonly INotifyNetworkModelUpdateContract contractProvider;

        #region Private Properties
        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public ScadaNotifyNetworkModelUpdate(INotifyNetworkModelUpdateContract contractProvider)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            this.contractProvider = contractProvider;
        }

        public async Task<bool> Notify(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            bool result = await this.contractProvider.Notify(modelChanges);

            if (!result)
            {
                return false;
            }

            ITransactionEnlistmentContract transactionEnlistmentClient = TransactionEnlistmentClient.CreateClient();
            bool success = await transactionEnlistmentClient.Enlist(DistributedTransactionNames.NetworkModelUpdateTransaction, MicroserviceNames.ScadaModelProviderService);

            if (success)
            {
                Logger.LogInformation($"{baseLogString} Notify => SCADA SUCCESSFULLY notified about network model update.");
            }
            else
            {
                Logger.LogInformation($"{baseLogString} Notify => SCADA UNSUCCESSFULLY notified about network model update.");
            }

            return success;
        }
    }
}
