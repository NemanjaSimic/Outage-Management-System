using OMS.Common.Cloud.Logger;
using OMS.Common.TmsContracts;
using System;
using System.Threading.Tasks;

namespace SCADA.ModelProviderImplementation.DistributedTransaction
{
    public class ScadaTransactionActor : ITransactionActorContract
    {
        private readonly string baseLogString;
        private readonly ITransactionActorContract contractProvider;

        #region Private Properties
        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public ScadaTransactionActor(ITransactionActorContract contractProvider)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            this.contractProvider = contractProvider;
        }

        public async Task<bool> Prepare()
        {
            bool success;

            try
            {
                success = await this.contractProvider.Prepare();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{baseLogString} Prepare => Exception: {ex.Message}", ex);
                success = false;
            }

            if (success)
            {
                Logger.LogInformation($"{baseLogString} Prepare => Preparation on SCADA Transaction actor SUCCESSFULLY finished.");
            }
            else
            {
                Logger.LogInformation($"{baseLogString} Prepare => Preparation on SCADA Transaction actor UNSUCCESSFULLY finished.");
            }

            return success;
        }

        public async Task Commit()
        {
            try
            {
                await this.contractProvider.Commit();
                Logger.LogInformation($"{baseLogString} Commit => Commit on SCADA Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"{baseLogString} Commit => Exception: {ex.Message}", ex);
                Logger.LogInformation($"{baseLogString} Commit => Commit on SCADA Transaction actor UNSUCCESSFULLY finished.");
            }
        }

        public async Task Rollback()
        {
            try
            {
                await this.contractProvider.Rollback();
                Logger.LogInformation($"{baseLogString} Rollback => Rollback on SCADA Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"{baseLogString} Rollback => Exception: {ex.Message}", ex);
                Logger.LogInformation($"{baseLogString} Rollback => Rollback on SCADA Transaction actor UNSUCCESSFULLY finished.");
            }
        }
    }
}
