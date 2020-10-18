using Common.CeContracts.ModelProvider;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.TmsContracts;
using OMS.Common.WcfClient.CE;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CE.ModelProviderImplementation
{
    public class CeTransactionActor : ITransactionActorContract
    {
        private readonly string baseLogString;

        #region Private Properties
        private ICloudLogger logger;

        public ReliableDictionaryAccess<byte, List<long>> ModelChanges { get; private set; }

        private bool isModelChangesInitialized;

        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
		#endregion

		public CeTransactionActor()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            Logger.LogDebug($"{baseLogString} ctor initialized.");
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }

        #region ITransactionActorContract
        public async Task<bool> Prepare()
        {
            logger.LogDebug($"{baseLogString} Prepare entered.");
            bool success;

            try
            {
                var modelProviderClient = CeModelProviderClient.CreateClient();
                success = await modelProviderClient.Prepare();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{baseLogString} Prepare => Exception: {ex.Message}", ex);
                success = false;
            }

            if (success)
            {
                Logger.LogInformation($"{baseLogString} Prepare => Preparation on CE Transaction actor SUCCESSFULLY finished.");
            }
            else
            {
                Logger.LogInformation($"{baseLogString} Prepare => Preparation on CE Transaction actor UNSUCCESSFULLY finished.");
            }

            return success;
        }

        public async Task Commit()
        {
            logger.LogDebug($"{baseLogString} Commit entered.");

            try
            {
                var modelProviderClient = CeModelProviderClient.CreateClient();
                await modelProviderClient.Commit();
                Logger.LogInformation($"{baseLogString} Commit => Commit on CE Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"{baseLogString} Commit => " +
                    $"{Environment.NewLine} Exception: {ex.Message} " +
                    $"{Environment.NewLine} Stack trace: {ex.StackTrace}", ex);
                Logger.LogInformation($"{baseLogString} Commit => Commit on CE Transaction actor UNSUCCESSFULLY finished.");
            }
        }

        public async Task Rollback()
        {
            logger.LogDebug($"{baseLogString} Rollback entered.");

            try
            {
                var modelProviderClient = CeModelProviderClient.CreateClient();
                await modelProviderClient.Rollback();
                Logger.LogInformation($"{baseLogString} Rollback => Rollback on CE Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"{baseLogString} Rollback => Exception: {ex.Message}", ex);
                Logger.LogInformation($"{baseLogString} Rollback => Rollback on CE Transaction actor UNSUCCESSFULLY finished.");
            }
        }
        #endregion
    }
}
